"""Integração do preparo: arquivos fictícios, sem executar o Unity."""
import hashlib
import json
import shutil
import subprocess
import tempfile
import unittest
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ORIGINAL = '7da36c5a97651df26c9fd8197ab9e5203efd7082'


class PreparationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.base = Path(self.temp.name)
        self.archive = self.base / 'original.zip'
        self.destination = self.base / 'projeto com espaços'
        self.manifest = json.loads((ROOT / 'docs/importacao.json').read_text(encoding='utf-8'))
        self.files = {}
        for item in self.manifest['arquivos']:
            content = subprocess.check_output(['git', 'show', ORIGINAL + ':' + item['caminho']], cwd=ROOT)
            self.assertEqual(item['sha256'], hashlib.sha256(content).hexdigest())
            self.files[item['caminho']] = content
        for scene in ['Menu1.2', 'AirPhase1.4', 'AirPhase2.4', 'AirPhase3.5', 'FirePhase1.4']:
            self.files['Assets/Scenes/' + scene + '.unity'] = b'fixture: nao e uma cena jogavel\n'
        self.files['Assets/Images/fixture.bin'] = b'asset-preservado'
        self.files['Library/cache.bin'] = b'cache-nao-importado'

    def tearDown(self):
        self.temp.cleanup()

    def write_zip(self):
        with zipfile.ZipFile(self.archive, 'w') as archive:
            for path, value in self.files.items():
                archive.writestr('Nox_Project/' + path, value)

    def prepare(self):
        return subprocess.run([
            'pwsh', '-NoProfile', '-File', str(ROOT / 'tools/Preparar-Nox.ps1'),
            '-OriginalZip', str(self.archive), '-Destino', str(self.destination)
        ], text=True, capture_output=True)

    def test_prepares_separate_copy_preserves_assets_and_ignores_cache(self):
        self.write_zip()
        before = hashlib.sha256(self.archive.read_bytes()).hexdigest()
        result = self.prepare()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertEqual(before, hashlib.sha256(self.archive.read_bytes()).hexdigest())
        self.assertEqual(b'asset-preservado', (self.destination / 'Assets/Images/fixture.bin').read_bytes())
        self.assertFalse((self.destination / 'Library').exists())
        for item in self.manifest['arquivos']:
            self.assertEqual((ROOT / item['caminho']).read_bytes(), (self.destination / item['caminho']).read_bytes())
        self.assertTrue((self.destination / 'Assets/Editor/NoxBuild.cs').exists())

    def test_rejects_modified_original(self):
        self.files['Assets/Scripts/PlayerScript.cs'] += b'// outra versao'
        self.write_zip()
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse(self.destination.exists())

    def test_never_overwrites_existing_destination(self):
        self.write_zip()
        self.destination.mkdir()
        sentinel = self.destination / 'sentinela.txt'
        sentinel.write_text('preservar')
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertEqual('preservar', sentinel.read_text())

    def test_rejects_path_traversal(self):
        self.files['Assets/../../escape.txt'] = b'nao gravar'
        self.write_zip()
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse((self.base / 'escape.txt').exists())
        self.assertFalse(self.destination.exists())

    def test_requires_all_build_scenes(self):
        del self.files['Assets/Scenes/FirePhase1.4.unity']
        self.write_zip()
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse(self.destination.exists())

    def test_rejects_symlink(self):
        self.write_zip()
        with zipfile.ZipFile(self.archive, 'a') as archive:
            link = zipfile.ZipInfo('Nox_Project/Assets/link')
            link.create_system = 3
            link.external_attr = 0o120777 << 16
            archive.writestr(link, '../../outside')
        self.assertNotEqual(0, self.prepare().returncode)
        self.assertFalse(self.destination.exists())


if __name__ == '__main__':
    if shutil.which('pwsh') is None:
        raise SystemExit('PowerShell 7 (pwsh) é necessário para esta suíte; os testes não foram executados.')
    unittest.main(verbosity=2)
