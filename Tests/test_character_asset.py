"""Confere o arquivo FBX real versionado; não substitui importação no Unity."""
import hashlib
import json
import struct
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def nodes(data, offset=27):
    result = []
    while offset + 13 < len(data):
        end, count, length, size = struct.unpack_from('<IIIB', data, offset)
        if not end:
            break
        name = data[offset+13:offset+13+size].decode('utf-8')
        pos = offset+13+size
        props = []
        stop = pos+length
        for _ in range(count):
            kind = chr(data[pos]);pos += 1
            if kind in 'SR':
                n = struct.unpack_from('<I', data, pos)[0];pos += 4
                props.append(data[pos:pos+n].decode('utf-8', errors='replace') if kind == 'S' else None);pos += n
            elif kind in 'YCLFID':
                pos += {'Y':2, 'C':1, 'L':8, 'F':4, 'I':4, 'D':8}[kind]
            elif kind in 'fdlibc':
                _, _, n = struct.unpack_from('<III', data, pos);pos += 12+n
            else:
                raise ValueError(kind)
        children = nodes(data, stop) if stop < end-13 else []
        result.append((name, props, children))
        offset = end
    return result


class CharacterAssetTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.raw = (ROOT/'Assets/Characters/Nox/Nox.fbx').read_bytes()
        cls.tree = nodes(cls.raw)
        cls.objects = next(n[2] for n in cls.tree if n[0] == 'Objects')

    def test_native_binary_fbx_and_fingerprint(self):
        self.assertTrue(self.raw.startswith(b'Kaydara FBX Binary'))
        self.assertEqual(7400, struct.unpack_from('<I', self.raw, 23)[0])
        manifest=json.loads((ROOT/'docs/personagem-nox.json').read_text())
        self.assertEqual(manifest['fbx_sha256'], hashlib.sha256(self.raw).hexdigest())

    def test_seven_animation_stacks(self):
        clips=[n[1][0].split('\x00')[0] for n in self.objects if n[0]=='AnimationStack']
        self.assertEqual(7, len(clips))
        for name in ['Nox_Idle','Nox_Walk','Nox_Jump','Nox_Attack','Nox_Fall','Nox_Land','Nox_Death']:
            self.assertTrue(any(name in clip for clip in clips), name)

    def test_meshes_materials_and_skin_are_present(self):
        self.assertEqual(5, sum(n[0]=='Geometry' for n in self.objects))
        self.assertEqual(7, sum(n[0]=='Material' for n in self.objects))
        self.assertTrue(any(n[0]=='Deformer' for n in self.objects))

    def test_no_reference_images_embedded(self):
        self.assertFalse(any(n[0] in ['Video','Texture'] for n in self.objects))
        self.assertNotIn(b'aluno.SENAIFIEMG', self.raw)


if __name__ == '__main__':
    unittest.main(verbosity=2)
