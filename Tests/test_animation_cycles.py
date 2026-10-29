"""Regressão das curvas reais exportadas, sem depender do Unity ou do Blender no CI."""

import unittest
from pathlib import Path
from fbx_reader import read_nodes

ROOT = Path(__file__).resolve().parents[1]


class AnimationCycleTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        tree = read_nodes((ROOT / "Assets/Characters/Nox/Nox.fbx").read_bytes())
        cls.objects = {n[1][0]: n for n in next(n[2] for n in tree if n[0] == "Objects")}
        cls.connections = [n[1] for n in next(n[2] for n in tree if n[0] == "Connections")]

    def curves(self, suffix):
        stack = next(
            k for k, v in self.objects.items() if v[0] == "AnimationStack" and suffix in v[1][1]
        )
        layers = {c[1] for c in self.connections if c[2] == stack}
        nodes = {c[1] for c in self.connections if c[2] in layers}
        ids = {c[1] for c in self.connections if c[2] in nodes}
        return [self.objects[k] for k in ids if self.objects[k][0] == "AnimationCurve"]

    def test_walk_and_idle_are_closed_at_all_curve_endpoints(self):
        for suffix in ["Nox_Walk", "Nox_Idle"]:
            curves = self.curves(suffix)
            self.assertGreater(len(curves), 100)
            for curve in curves:
                values = next(n[1][0] for n in curve[2] if n[0] == "KeyValueFloat")
                self.assertAlmostEqual(values[0], values[-1], delta=0.005, msg=suffix)

    def test_walk_keeps_moving_in_both_halves_of_the_cycle(self):
        moving_bones = set()
        knees = set()
        for curve in self.curves("Nox_Walk"):
            values = next(n[1][0] for n in curve[2] if n[0] == "KeyValueFloat")
            node = next(c[2] for c in self.connections if c[1] == curve[1][0])
            model = next(c[2] for c in self.connections if c[1] == node and c[0] == "OP")
            bone = self.objects[model][1][1].split("\x00")[0]
            half = len(values) // 2
            if (
                max(values[:half]) - min(values[:half]) > 5
                and max(values[half:]) - min(values[half:]) > 5
            ):
                moving_bones.add(bone)
            # Curvas Euler dividem a rotação entre eixos conforme o roll do osso.
            if max(values) - min(values) > 20:
                knees.add(bone)
        self.assertTrue({"coxa.L", "coxa.R", "braço1.L", "braço1.R"} <= moving_bones)
        self.assertTrue({"canela.L", "canela.R"} <= knees)

    def test_mesh_and_rig_share_one_fbx_axis_and_unit_conversion(self):
        models = [
            obj for obj in self.objects.values() if obj[0] == "Model" and obj[1][2] != "LimbNode"
        ]
        self.assertEqual(6, len(models))
        for model in models:
            props = {
                n[1][0]: n[1][4:]
                for group in model[2]
                if group[0] == "Properties70"
                for n in group[2]
            }
            for value in props["Lcl Scaling"]:
                self.assertAlmostEqual(
                    100, value, places=4
                )  # conversão uniforme de metros para centímetros FBX.
            for actual, expected in zip(props.get("Lcl Rotation", [0, 0, 0]), [-90, 0, 0]):
                self.assertAlmostEqual(expected, actual, places=3)
            self.assertTrue(all(abs(v) < 0.0001 for v in props.get("Lcl Translation", [0, 0, 0])))

    def test_walk_duration_is_eight_tenths_of_a_second(self):
        for curve in self.curves("Nox_Walk"):
            times = next(n[1][0] for n in curve[2] if n[0] == "KeyTime")
            self.assertAlmostEqual(0.8, (times[-1] - times[0]) / 46186158000, places=5)


if __name__ == "__main__":
    unittest.main(verbosity=2)
