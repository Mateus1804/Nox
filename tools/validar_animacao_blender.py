"""Amostragem do FBX importado pelo Blender; não executa o Animator do Unity.

blender -b --python tools/validar_animacao_blender.py -- --fbx Assets/Characters/Nox/Nox.fbx --output evidencias
"""
import argparse
import json
import os
import sys

import bpy

parser = argparse.ArgumentParser()
parser.add_argument('--fbx', required=True)
parser.add_argument('--output', required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.abspath(args.fbx))
rig = next(obj for obj in bpy.data.objects if obj.type == 'ARMATURE')
rig.animation_data.action = next(action for action in bpy.data.actions if 'Nox_Walk' in action.name)
start, end = rig.animation_data.action.frame_range
samples = {}
for fraction in [0, 0.25, 0.5, 0.75, 1]:
    bpy.context.scene.frame_set(int(start+(end-start)*fraction))
    bpy.context.view_layer.update()
    samples[str(fraction)] = {name: list(rig.pose.bones[name].matrix.translation)
                              for name in ['pe.L', 'pe.D']}
for name in ['pe.L', 'pe.D']:
    assert max(abs(a-b) for a, b in zip(samples['0'][name], samples['1'][name])) < 0.0001, name
    assert max(abs(a-b) for a, b in zip(samples['0.25'][name], samples['0.75'][name])) > 0.1, name
os.makedirs(args.output, exist_ok=True)
with open(os.path.join(args.output, 'animacao-amostras.json'), 'w') as handle:
    json.dump({'quadros': [start, end], 'pes_avaliados': samples, 'resultado': 'aprovado'}, handle, indent=2)
print('Ciclo fechado e alternância dos dois pés conferidos no FBX reimportado.')
