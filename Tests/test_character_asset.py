"""Confere o arquivo FBX real versionado; não substitui importação no Unity."""

import hashlib
import json
import struct
import unittest
from pathlib import Path

from fbx_reader import read_nodes

ROOT = Path(__file__).resolve().parents[1]


class CharacterAssetTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.raw = (ROOT / "Assets/Characters/Nox/Nox.fbx").read_bytes()
        cls.tree = read_nodes(cls.raw)
        cls.objects = next(n[2] for n in cls.tree if n[0] == "Objects")

    def test_native_binary_fbx_and_fingerprint(self):
        self.assertTrue(self.raw.startswith(b"Kaydara FBX Binary"))
        self.assertEqual(7400, struct.unpack_from("<I", self.raw, 23)[0])
        manifest = json.loads((ROOT / "docs/personagem-nox.json").read_text())
        self.assertEqual(manifest["fbx_sha256"], hashlib.sha256(self.raw).hexdigest())

    def test_seven_animation_stacks(self):
        clips = [n[1][1].split("\x00")[0] for n in self.objects if n[0] == "AnimationStack"]
        self.assertEqual(7, len(clips))
        for name in [
            "Nox_Idle",
            "Nox_Walk",
            "Nox_Jump",
            "Nox_Attack",
            "Nox_Fall",
            "Nox_Land",
            "Nox_Death",
        ]:
            self.assertTrue(any(name in clip for clip in clips), name)

    def test_meshes_materials_and_skin_are_present(self):
        self.assertEqual(5, sum(n[0] == "Geometry" for n in self.objects))
        self.assertEqual(7, sum(n[0] == "Material" for n in self.objects))
        self.assertTrue(any(n[0] == "Deformer" for n in self.objects))

    def test_no_reference_images_embedded(self):
        self.assertFalse(any(n[0] in ["Video", "Texture"] for n in self.objects))
        self.assertNotIn(b"aluno.SENAIFIEMG", self.raw)


if __name__ == "__main__":
    unittest.main(verbosity=2)
