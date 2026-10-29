"""Contratos de componentes da revisão anterior; não executa o Unity."""

import re
import subprocess
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = "06d3335f311628436e4a61fbe29c62b8a61b44d7"


def original(path):
    raw = subprocess.check_output(["git", "show", BASE + ":" + path], cwd=ROOT)
    try:
        return raw.decode("utf-8-sig")
    except UnicodeDecodeError:
        return raw.decode("cp1252")


def public_fields(source):
    names = set()
    for match in re.finditer(r"\bpublic\s+(?:static\s+)?[\w<>.\[\]]+\s+([^;{}\n]+);", source):
        for declaration in match[1].split(","):
            name = declaration.split("=")[0].strip()
            if re.fullmatch(r"[A-Za-z_]\w*", name):
                names.add(name)
    return names


def public_methods(source):
    return set(re.findall(r"\bpublic\s+(?:static\s+)?[\w<>.\[\]]+\s+(\w+)\s*\(", source))


class SourceContracts(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.paths = subprocess.check_output(
            ["git", "ls-tree", "-r", "--name-only", BASE, "--", "Assets"], cwd=ROOT, text=True
        ).splitlines()

    def test_existing_component_guids_are_preserved(self):
        for name in self.paths:
            if name.endswith(".cs.meta"):
                with self.subTest(path=name):
                    before = re.search(r"^guid: (\w+)$", original(name), re.M)[1]
                    current = (ROOT / name).read_text(encoding="utf-8-sig")
                    self.assertEqual(before, re.search(r"^guid: (\w+)$", current, re.M)[1])

    def test_existing_public_fields_and_methods_remain(self):
        for name in self.paths:
            if name.endswith(".cs"):
                with self.subTest(path=name):
                    before = original(name)
                    current = (ROOT / name).read_text(encoding="utf-8-sig")
                    self.assertFalse(
                        public_fields(before) - public_fields(current), "Campo público removido"
                    )
                    self.assertFalse(
                        public_methods(before) - public_methods(current), "Método público removido"
                    )

    def test_metadata_guids_are_unique(self):
        guids = {}
        for path in (ROOT / "Assets").rglob("*.meta"):
            match = re.search(r"^guid: (\w+)$", path.read_text(encoding="utf-8-sig"), re.M)
            if match:
                self.assertNotIn(match[1], guids, str(path))
                guids[match[1]] = path

    def test_runtime_does_not_import_editor_namespace(self):
        for path in (ROOT / "Assets/Scripts").glob("*.cs"):
            self.assertNotRegex(
                path.read_text(encoding="utf-8-sig"), r"(?m)^\s*using\s+UnityEditor\b", str(path)
            )


if __name__ == "__main__":
    unittest.main(verbosity=2)
