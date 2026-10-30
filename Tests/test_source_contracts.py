"""Contratos de migração de scripts; não substitui a importação dos assets no Unity."""

import json
import re
import subprocess
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MIGRATION = json.loads(
    (ROOT / "Assets/Editor/Build/script-migration.json").read_text(encoding="utf-8")
)
BASE = MIGRATION["sourceCommit"]


def original(path):
    raw = subprocess.check_output(["git", "show", BASE + ":" + path], cwd=ROOT)
    return raw.decode("utf-8-sig")


def mask_literals(source):
    # Mantém os índices e ignora chaves dentro de comentários, strings e caracteres.
    pattern = r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\''
    return re.sub(pattern, lambda match: " " * len(match[0]), source)


def classes(source):
    masked = mask_literals(source)
    result = {}
    for match in re.finditer(r"\bclass\s+(\w+)(?:\s*:\s*(\w+))?[^{}]*\{", masked):
        start = match.end()
        depth = 1
        end = start
        while depth and end < len(masked):
            depth += (masked[end] == "{") - (masked[end] == "}")
            end += 1
        if depth:
            raise ValueError("Classe sem fechamento: " + match[1])
        result[match[1]] = (source[start : end - 1], match[2])
    return result


def public_fields(source):
    names = set()
    for match in re.finditer(
        r"\bpublic\s+(?:static\s+)?[\w<>.\[\]]+\s+([^;{}\n]+);", mask_literals(source)
    ):
        for declaration in match[1].split(","):
            name = declaration.split("=")[0].strip()
            if re.fullmatch(r"[A-Za-z_]\w*", name):
                names.add(name)
    return names


def public_members(source):
    properties = set(
        re.findall(
            r"\bpublic\s+(?:static\s+)?[\w<>.\[\]]+\s+(\w+)\s*(?:\{|=>)", mask_literals(source)
        )
    )
    return public_fields(source) | properties


def public_methods(source):
    return set(
        re.findall(
            r"\bpublic\s+(?:(?:static|virtual|override)\s+)?[\w<>.\[\]]+\s+(\w+)\s*\(",
            mask_literals(source),
        )
    )


class SourceContracts(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.types = {}
        for path in (ROOT / "Assets").rglob("*.cs"):
            for name, definition in classes(path.read_text(encoding="utf-8-sig")).items():
                if name in cls.types:
                    raise ValueError("Classe repetida: " + name)
                cls.types[name] = definition

    def inherited_source(self, name):
        seen = set()
        bodies = []
        while name in self.types:
            self.assertNotIn(name, seen)
            seen.add(name)
            body, name = self.types[name]
            bodies.append(body)
        return "\n".join(bodies)

    def test_component_guids_survive_moves_and_renames(self):
        for entry in MIGRATION["scripts"]:
            if not entry["component"]:
                continue
            with self.subTest(script=entry["from"]):
                current = (ROOT / (entry["to"] + ".meta")).read_text(encoding="utf-8-sig")
                self.assertEqual(entry["guid"], re.search(r"^guid: (\w+)$", current, re.M)[1])
                self.assertIn(entry["newType"], self.types)
                if entry["oldType"] != entry["newType"]:
                    self.assertIn(
                        'sourceClassName: "' + entry["oldType"] + '"',
                        (ROOT / entry["to"]).read_text(),
                    )

    def test_public_callbacks_and_accessors_remain_available(self):
        for entry in MIGRATION["scripts"]:
            if entry["oldType"] == "NoxPlayerControllerSupport":
                continue  # Módulo estático aposentado; suas rotinas foram incorporadas ao controlador.
            with self.subTest(script=entry["from"]):
                before = classes(original(entry["from"]))[entry["oldType"]][0]
                after = self.inherited_source(entry["newType"])
                self.assertFalse(
                    public_fields(before) - public_members(after), "Acesso público removido"
                )
                self.assertFalse(
                    public_methods(before) - public_methods(after), "Callback público removido"
                )

    def test_renamed_serialized_fields_have_migration_names(self):
        for entry in MIGRATION["scripts"]:
            if not entry["component"]:
                continue
            with self.subTest(script=entry["from"]):
                before = classes(original(entry["from"]))[entry["oldType"]][0]
                static = set(re.findall(r"public static [\w.<>\[\]]+\s+(\w+)", before))
                fields = public_fields(before) - static
                after = self.inherited_source(entry["newType"])
                serialized = public_fields(after) | set(
                    re.findall(r'FormerlySerializedAs\("(\w+)"\)', after)
                )
                self.assertFalse(fields - serialized, "Campo sem nome serializado ou migração")

    def test_metadata_guids_are_unique(self):
        guids = {}
        for path in (ROOT / "Assets").rglob("*.meta"):
            match = re.search(r"^guid: (\w+)$", path.read_text(encoding="utf-8-sig"), re.M)
            if match:
                self.assertNotIn(match[1], guids, str(path))
                guids[match[1]] = path

    def test_runtime_does_not_import_editor_namespace(self):
        for path in (ROOT / "Assets/Scripts").rglob("*.cs"):
            self.assertNotRegex(
                path.read_text(encoding="utf-8-sig"), r"(?m)^\s*using\s+UnityEditor\b", str(path)
            )

    def test_retired_flat_paths_are_not_distributed_twice(self):
        for entry in MIGRATION["scripts"]:
            if entry["from"] != entry["to"]:
                self.assertFalse((ROOT / entry["from"]).exists(), entry["from"])
                self.assertFalse((ROOT / (entry["from"] + ".meta")).exists(), entry["from"])
            self.assertTrue((ROOT / entry["to"]).exists())


if __name__ == "__main__":
    unittest.main(verbosity=2)
