"""Integração do preparo: arquivos fictícios, sem executar o Unity."""

import hashlib
import json
import os
import shutil
import subprocess
import tempfile
import unittest
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ORIGINAL = "7da36c5a97651df26c9fd8197ab9e5203efd7082"
POWERSHELL = os.environ.get("NOX_TEST_POWERSHELL", "pwsh")
MIGRATION = json.loads(
    (ROOT / "Assets/Editor/Build/script-migration.json").read_text(encoding="utf-8")
)
CURRENT_PATHS = {entry["from"]: entry["to"] for entry in MIGRATION["scripts"]}


def current_path(path):
    if path.endswith(".cs.meta"):
        return CURRENT_PATHS.get(path[:-5], path[:-5]) + ".meta"
    return CURRENT_PATHS.get(path, path)


class PreparationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.base = Path(self.temp.name)
        self.archive = self.base / "original.zip"
        self.destination = self.base / "projeto com espaços"
        self.manifest = json.loads((ROOT / "docs/importacao.json").read_text(encoding="utf-8"))
        self.preparation_backups = set()
        self.files = {}
        for item in self.manifest["arquivos"]:
            content = subprocess.check_output(
                ["git", "show", ORIGINAL + ":" + item["caminho"]], cwd=ROOT
            )
            self.assertEqual(item["sha256"], hashlib.sha256(content).hexdigest())
            self.files[item["caminho"]] = content
        for scene in ["Menu1.2", "AirPhase1.4", "AirPhase2.4", "AirPhase3.5", "FirePhase1.4"]:
            self.files["Assets/Scenes/" + scene + ".unity"] = b"fixture: nao e uma cena jogavel\n"
        self.files["Assets/Images/fixture.bin"] = b"asset-preservado"
        self.files["Library/cache.bin"] = b"cache-nao-importado"

    def tearDown(self):
        self.temp.cleanup()

    def write_zip(self):
        with zipfile.ZipFile(self.archive, "w") as archive:
            for path, value in self.files.items():
                archive.writestr("Nox_Project/" + path, value)

    def prepare(self):
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Preparar-Nox.ps1"),
                "-OriginalZip",
                str(self.archive),
                "-Destino",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )

        if result.returncode == 0:
            self.preparation_backups = set(self.base.glob("*-Backup-Scripts-*"))
        return result

    def update_backups(self):
        return sorted(set(self.base.glob("*-Backup-Scripts-*")) - self.preparation_backups)

    def modified_project(self):
        self.write_zip()
        result = self.prepare()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        (self.destination / "Assets/Scripts/Compatibility/PlayerScript.cs").write_bytes(
            b"// previous player\n"
        )
        (self.destination / "Assets/Art/NoxSky.shader").write_bytes(b"// previous shader\n")
        for name in ["Player/NoxPlayerController.cs", "Player/NoxPlayerController.cs.meta"]:
            (self.destination / "Assets/Scripts" / name).unlink()
        return self.project_snapshot()

    def project_snapshot(self):
        return {
            path.relative_to(self.destination).as_posix(): hashlib.sha256(
                path.read_bytes()
            ).hexdigest()
            for path in self.destination.rglob("*")
            if path.is_file()
        }

    def faulty_update(self, mode):
        wrapper = ROOT / "Tests/fixtures/Updater-Faults.ps1"
        return subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(wrapper),
                "-Updater",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-ProjectRoot",
                str(self.destination),
                "-Fault",
                mode,
            ],
            text=True,
            capture_output=True,
        )

    def recovery_report(self):
        backup = next(iter(self.update_backups()))
        return backup, json.loads((backup / "recuperacao.json").read_text(encoding="utf-8-sig"))

    def test_updater_restores_partial_write_and_removes_new_files(self):
        before = self.modified_project()
        result = self.faulty_update("copy")
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(before, self.project_snapshot())
        backup, report = self.recovery_report()
        self.assertEqual("restaurado", report["status"])
        self.assertEqual([], report["falhasDeRecuperacao"])
        self.assertIn(
            "Assets/Scripts/Player/NoxPlayerController.cs",
            {path.replace("\\", "/") for path in report["arquivosTentados"]},
        )
        self.assertEqual(
            b"// previous player\n",
            (backup / "Assets/Scripts/Compatibility/PlayerScript.cs").read_bytes(),
        )

    def test_updater_detects_corruption_and_restores_previous_bytes(self):
        before = self.modified_project()
        result = self.faulty_update("corrupt")
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(before, self.project_snapshot())
        _, report = self.recovery_report()
        self.assertEqual("restaurado", report["status"])
        self.assertIn("esperado", report["erroOriginal"])

    def test_updater_leaves_project_untouched_if_backup_fails(self):
        before = self.modified_project()
        result = self.faulty_update("backup")
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(before, self.project_snapshot())
        _, report = self.recovery_report()
        self.assertEqual("sem-alteracoes", report["status"])
        self.assertEqual([], report["arquivosTentados"])

    def test_updater_reports_partial_recovery_and_keeps_original_error(self):
        before = self.modified_project()
        result = self.faulty_update("restore")
        self.assertNotEqual(0, result.returncode)
        after = self.project_snapshot()
        self.assertEqual(
            {"Assets/Art/NoxSky.shader"}, {key for key in before if before[key] != after[key]}
        )
        self.assertEqual(set(before), set(after))
        _, report = self.recovery_report()
        self.assertEqual("recuperacao-parcial", report["status"])
        self.assertEqual("simulated copy failure", report["erroOriginal"])
        self.assertEqual(1, len(report["falhasDeRecuperacao"]))
        self.assertIn("NoxSky.shader", report["falhasDeRecuperacao"][0])
        self.assertIn("incompleta", result.stdout + result.stderr)

    def test_updater_restores_if_completion_manifest_fails(self):
        before = self.modified_project()
        result = self.faulty_update("manifest")
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(before, self.project_snapshot())
        _, report = self.recovery_report()
        self.assertEqual("restaurado", report["status"])
        self.assertEqual("simulated manifest failure", report["erroOriginal"])

    def test_updates_existing_project_preserving_scenes_and_backing_up_scripts(self):
        self.write_zip()
        result = self.prepare()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        player = self.destination / "Assets/Scripts/Compatibility/PlayerScript.cs"
        player.write_bytes(b"// script local anterior\n")
        sky = self.destination / "Assets/Art/NoxSky.shader"
        sky.write_bytes(b"// shader local anterior\n")
        scene = self.destination / "Assets/Scenes/Menu1.2.unity"
        before_scene = scene.read_bytes()
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertEqual(
            (ROOT / "Assets/Scripts/Compatibility/PlayerScript.cs").read_bytes(),
            player.read_bytes(),
        )
        self.assertEqual(before_scene, scene.read_bytes())
        self.assertEqual(
            (ROOT / "Assets/Characters/Nox/Nox.fbx").read_bytes(),
            (self.destination / "Assets/Characters/Nox/Nox.fbx").read_bytes(),
        )
        backups = self.update_backups()
        self.assertEqual(1, len(backups))
        self.assertEqual(
            b"// script local anterior\n",
            (backups[0] / "Assets/Scripts/Compatibility/PlayerScript.cs").read_bytes(),
        )
        self.assertEqual(
            b"// shader local anterior\n", (backups[0] / "Assets/Art/NoxSky.shader").read_bytes()
        )
        self.assertEqual((ROOT / "Assets/Art/NoxSky.shader").read_bytes(), sky.read_bytes())
        self.assertTrue((backups[0] / "arquivos-atualizados.json").exists())

    def test_updater_preserves_previous_version_and_custom_generated_assets(self):
        self.write_zip()
        self.assertEqual(0, self.prepare().returncode)
        files = {
            "ProjectSettings/NoxScenesPrepared-v5.txt": b"preparacao anterior",
            "Assets/Characters/Nox/Nox-v5.controller": b"animator personalizado",
            "Assets/Art/Generated-v5/personalizado.mat": b"material personalizado",
        }
        for name, content in files.items():
            path = self.destination / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(content)
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        for name, content in files.items():
            self.assertEqual(content, (self.destination / name).read_bytes())
        backup = next(iter(self.update_backups()))
        self.assertEqual(
            b"material personalizado",
            (backup / "Assets/Art/Generated-v5/personalizado.mat").read_bytes(),
        )
        self.assertEqual(
            (ROOT / "Assets/Editor/Scenes/NoxScenePolish.cs").read_bytes(),
            (self.destination / "Assets/Editor/Scenes/NoxScenePolish.cs").read_bytes(),
        )

    def test_updater_rejects_directory_at_file_path_before_writing(self):
        self.write_zip()
        self.assertEqual(0, self.prepare().returncode)
        shader = self.destination / "Assets/Art/NoxSky.shader"
        shader.unlink()
        shader.mkdir()
        player = self.destination / "Assets/Scripts/Compatibility/PlayerScript.cs"
        player.write_bytes(b"// preservar antes da validacao\n")
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(b"// preservar antes da validacao\n", player.read_bytes())
        self.assertEqual([], list(shader.iterdir()))
        self.assertFalse(self.update_backups())

    def test_updater_rejects_file_at_directory_path_before_writing(self):
        self.write_zip()
        self.assertEqual(0, self.prepare().returncode)
        art = self.destination / "Assets/Art"
        shutil.rmtree(art)
        art.write_bytes(b"pasta bloqueada")
        player = self.destination / "Assets/Scripts/Compatibility/PlayerScript.cs"
        player.write_bytes(b"// preservar antes da validacao\n")
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(b"// preservar antes da validacao\n", player.read_bytes())
        self.assertEqual(b"pasta bloqueada", art.read_bytes())
        self.assertFalse(self.update_backups())

    def install_old_player_path(self, different_guid=False):
        entry = next(item for item in MIGRATION["scripts"] if item["oldType"] == "PlayerScript")
        target = self.destination / entry["from"]
        target.write_bytes(
            subprocess.check_output(
                ["git", "show", MIGRATION["sourceCommit"] + ":" + entry["from"]], cwd=ROOT
            )
        )
        guid = "00000000000000000000000000000001" if different_guid else entry["guid"]
        target.with_name(target.name + ".meta").write_text(
            "fileFormatVersion: 2\nguid: " + guid + "\n"
        )
        for name in [entry["to"], entry["to"] + ".meta"]:
            (self.destination / name).unlink()
        return entry

    def test_updater_moves_old_layout_and_preserves_script_guid(self):
        self.modified_project()
        entry = self.install_old_player_path()
        scene = self.destination / "Assets/Scenes/Menu1.2.unity"
        before = scene.read_bytes()
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertFalse((self.destination / entry["from"]).exists())
        self.assertFalse((self.destination / (entry["from"] + ".meta")).exists())
        self.assertEqual(
            (ROOT / entry["to"]).read_bytes(), (self.destination / entry["to"]).read_bytes()
        )
        self.assertIn(entry["guid"], (self.destination / (entry["to"] + ".meta")).read_text())
        self.assertEqual(before, scene.read_bytes())

    def test_updater_rejects_old_path_owned_by_another_guid(self):
        self.modified_project()
        self.install_old_player_path(different_guid=True)
        before = self.project_snapshot()
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(before, self.project_snapshot())
        self.assertFalse(self.update_backups())

    def test_updater_restores_deleted_old_paths_when_retirement_fails(self):
        self.modified_project()
        self.install_old_player_path()
        before = self.project_snapshot()
        result = self.faulty_update("retire")
        self.assertNotEqual(0, result.returncode)
        self.assertEqual(before, self.project_snapshot())
        _, report = self.recovery_report()
        self.assertEqual("restaurado", report["status"])
        self.assertEqual("simulated retirement failure", report["erroOriginal"])

    def test_updater_removes_known_orphaned_old_meta(self):
        self.modified_project()
        entry = self.install_old_player_path()
        (self.destination / entry["from"]).unlink()
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertFalse((self.destination / (entry["from"] + ".meta")).exists())
        self.assertTrue((self.destination / entry["to"]).exists())

    def test_preparation_does_not_leave_flat_duplicate_scripts(self):
        self.write_zip()
        result = self.prepare()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        for entry in MIGRATION["scripts"]:
            if entry["from"] != entry["to"]:
                self.assertFalse((self.destination / entry["from"]).exists())
                self.assertFalse((self.destination / (entry["from"] + ".meta")).exists())

    def test_updater_rejects_folder_without_unity_project(self):
        self.destination.mkdir()
        result = subprocess.run(
            [
                POWERSHELL,
                "-NoProfile",
                "-File",
                str(ROOT / "tools/Atualizar-Nox.ps1"),
                "-Projeto",
                str(self.destination),
            ],
            text=True,
            capture_output=True,
        )
        self.assertNotEqual(0, result.returncode)
        self.assertFalse(self.update_backups())

    def test_prepares_separate_copy_preserves_assets_and_ignores_cache(self):
        self.write_zip()
        before = hashlib.sha256(self.archive.read_bytes()).hexdigest()
        result = self.prepare()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertEqual(before, hashlib.sha256(self.archive.read_bytes()).hexdigest())
        self.assertEqual(
            b"asset-preservado", (self.destination / "Assets/Images/fixture.bin").read_bytes()
        )
        self.assertFalse((self.destination / "Library").exists())
        for item in self.manifest["arquivos"]:
            self.assertEqual(
                (ROOT / current_path(item["caminho"])).read_bytes(),
                (self.destination / current_path(item["caminho"])).read_bytes(),
            )
        self.assertTrue((self.destination / "Assets/Editor/Build/NoxBuild.cs").exists())
        backup = next(iter(self.preparation_backups))
        self.assertEqual(
            self.files["Assets/Scripts/PlayerScript.cs"],
            (backup / "Assets/Scripts/PlayerScript.cs").read_bytes(),
        )

    def test_rejects_modified_original(self):
        self.files["Assets/Scripts/PlayerScript.cs"] += b"// outra versao"
        self.write_zip()
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse(self.destination.exists())

    def test_never_overwrites_existing_destination(self):
        self.write_zip()
        self.destination.mkdir()
        sentinel = self.destination / "sentinela.txt"
        sentinel.write_text("preservar")
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertEqual("preservar", sentinel.read_text())

    def test_rejects_path_traversal(self):
        self.files["Assets/../../escape.txt"] = b"nao gravar"
        self.write_zip()
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse((self.base / "escape.txt").exists())
        self.assertFalse(self.destination.exists())

    def test_requires_all_build_scenes(self):
        del self.files["Assets/Scenes/FirePhase1.4.unity"]
        self.write_zip()
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse(self.destination.exists())

    def test_rejects_symlink(self):
        self.write_zip()
        with zipfile.ZipFile(self.archive, "a") as archive:
            link = zipfile.ZipInfo("Nox_Project/Assets/link")
            link.create_system = 3
            link.external_attr = 0o120777 << 16
            archive.writestr(link, "../../outside")
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse(self.destination.exists())


if __name__ == "__main__":
    if shutil.which(POWERSHELL) is None:
        raise SystemExit(
            f"PowerShell não encontrado: {POWERSHELL}; os testes não foram executados."
        )
    unittest.main(verbosity=2)
