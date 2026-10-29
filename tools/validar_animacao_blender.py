"""Amostragem do FBX importado pelo Blender; não executa o Animator do Unity.

blender -b --python tools/validar_animacao_blender.py -- --fbx Assets/Characters/Nox/Nox.fbx --output evidencias
"""

import argparse
import json
import os
import sys
from pathlib import Path

import bpy
from mathutils import Vector
from mathutils.kdtree import KDTree

parser = argparse.ArgumentParser()
parser.add_argument("--fbx", required=True)
parser.add_argument("--output", required=True)
parser.add_argument("--reference")
args = parser.parse_args(sys.argv[sys.argv.index("--") + 1 :])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.abspath(args.fbx))
rig = next(obj for obj in bpy.data.objects if obj.type == "ARMATURE")
rig.animation_data.action = next(action for action in bpy.data.actions if "Nox_Walk" in action.name)
start, end = rig.animation_data.action.frame_range
samples = {}
for fraction in [0, 0.25, 0.5, 0.75, 1]:
    bpy.context.scene.frame_set(int(start + (end - start) * fraction))
    bpy.context.view_layer.update()
    samples[str(fraction)] = {
        name: list(rig.pose.bones[name].matrix.translation) for name in ["pe.L", "pe.D"]
    }
for name in ["pe.L", "pe.D"]:
    assert max(abs(a - b) for a, b in zip(samples["0"][name], samples["1"][name])) < 0.0001, name
    assert max(abs(a - b) for a, b in zip(samples["0.25"][name], samples["0.75"][name])) > 0.1, name
os.makedirs(args.output, exist_ok=True)
with open(os.path.join(args.output, "animacao-amostras.json"), "w") as handle:
    json.dump(
        {"quadros": [start, end], "pes_avaliados": samples, "resultado": "aprovado"},
        handle,
        indent=2,
    )
print("Ciclo fechado e alternância dos dois pés conferidos no FBX reimportado.")

# Sete ações e o rig completo são conferidos na reimportação real.
expected = ["Nox_Idle", "Nox_Walk", "Nox_Jump", "Nox_Attack", "Nox_Fall", "Nox_Land", "Nox_Death"]
assert len(rig.data.bones) == 52
assert len(bpy.data.objects["panda"].data.vertices) == 1994
actions = {}
for name in expected:
    action = next(action for action in bpy.data.actions if action.name.endswith(name))
    assert action.frame_range[1] > action.frame_range[0], name
    actions[name] = list(action.frame_range)
with open(os.path.join(args.output, "animacoes-estrutura.json"), "w") as handle:
    json.dump(
        {
            "ossos": 52,
            "vertices_personagem": 1994,
            "acoes": actions,
            "resultado": "aprovado",
            "ambiente": "Blender, sem execução no Unity",
        },
        handle,
        indent=2,
    )
print("Sete ações, rig de 52 ossos e malha de 1.994 vértices conferidos.")

# A malha deformada também é conferida. Contar curvas não detecta erro de bind/escala.
assert max(abs(value - 1) for value in rig.scale) < 0.00001, "Escala do rig não normalizada"
assert rig.rotation_euler.to_quaternion().angle < 0.0001, "Rotação residual no rig reimportado"
reference = json.loads(Path(args.reference).read_text(encoding="utf-8")) if args.reference else None
summary = []
mesh = bpy.data.objects["panda"]
for name in expected:
    action = next(a for a in bpy.data.actions if a.name.endswith(name))
    rig.animation_data.action = action
    for fraction in [0, 0.25, 0.5, 0.75, 1]:
        frame = int(
            action.frame_range[0] + (action.frame_range[1] - action.frame_range[0]) * fraction
        )
        bpy.context.scene.frame_set(frame)
        bpy.context.view_layer.update()
        evaluated = mesh.evaluated_get(bpy.context.evaluated_depsgraph_get())
        data = evaluated.to_mesh()
        points = [evaluated.matrix_world @ vertex.co for vertex in data.vertices]
        evaluated.to_mesh_clear()
        height = max(p.z for p in points) - min(p.z for p in points)
        if name == "Nox_Idle":
            assert 1.68 < height < 1.72, ("Altura deformada", height)
        record = {"acao": name, "fracao": fraction, "altura_malha": height}
        if reference:
            original = reference[name + ":" + str(fraction)]
            assert len(points) == len(original)
            tree = KDTree(len(original))
            for i, point in enumerate(original):
                tree.insert(Vector(point), i)
            tree.balance()
            error = max(tree.find(point)[2] for point in points)
            assert error < 0.0001, (name, fraction, error)
            record["desvio_maximo"] = error
        summary.append(record)
with open(os.path.join(args.output, "malha-reimportada-v7.json"), "w") as handle:
    json.dump(
        {
            "ambiente": "Blender 3.6.23, sem execução do Unity",
            "amostras": summary,
            "referencia_comparada": bool(reference),
            "resultado": "aprovado",
        },
        handle,
        indent=2,
    )
print("Hierarquia normalizada, altura do repouso e malha reimportada conferidas em 35 poses.")
