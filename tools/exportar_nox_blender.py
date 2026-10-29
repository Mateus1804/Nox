import bpy
import math
import json
import os
from mathutils import Vector, Quaternion, Matrix
import argparse
import sys
import hashlib
from pathlib import Path

args = argparse.ArgumentParser()
args.add_argument("--source", required=True)
args.add_argument("--output", required=True)
opts = args.parse_args(sys.argv[sys.argv.index("--") + 1 :])
if (
    hashlib.sha256(Path(opts.source).read_bytes()).hexdigest()
    != "64f1fbfecaa22946aca67eafb90d7d5fbf9c9865377b3364482a54aa0ffc8f7e"
):
    raise RuntimeError("Arquivo Nox.blend não corresponde à referência analisada.")
bpy.ops.wm.open_mainfile(filepath=opts.source)
rig = bpy.data.objects["panda?"]
mesh = bpy.data.objects["panda"]
rig.name = "NoxRig"
rig.animation_data.action = next(a for a in bpy.data.actions if "150f" in a.name)
bpy.context.scene.frame_set(1)
keep = []
for a in list(bpy.data.actions):
    if "70f" in a.name:
        a.name = "Nox_Walk"
        keep.append(a)
    elif "150f" in a.name:
        a.name = "Nox_Idle"
        keep.append(a)
    else:
        bpy.data.actions.remove(a)
# Bake the evaluated original idle pose before removing IK on the working copy.
# Original files remain untouched. The new cycle uses FK keys on all 52 bones.
rig.animation_data.action = next(a for a in keep if a.name == "Nox_Idle")
bpy.context.scene.frame_set(1)
bpy.context.view_layer.update()
pose = {b.name: b.matrix.copy() for b in rig.pose.bones}
for b in rig.pose.bones:
    for constraint in list(b.constraints):
        b.constraints.remove(constraint)
for b in rig.pose.bones:
    b.matrix = pose[b.name]
bpy.context.view_layer.update()
base = {b.name: b.matrix_basis.copy() for b in rig.pose.bones}
for a in list(keep):
    bpy.data.actions.remove(a)
keep = []
# Rig local Y is vertical; each bone axis is converted from rig space.
for name, duration in [("Nox_Idle", 72), ("Nox_Walk", 24)]:
    a = bpy.data.actions.new(name)
    a.use_fake_user = True
    rig.animation_data.action = a
    for frame in range(duration + 1):
        phase = 2 * math.pi * frame / duration
        for b in rig.pose.bones:
            b.matrix_basis = base[b.name]
            b.rotation_mode = "QUATERNION"
            amount = 0
            if name == "Nox_Walk":
                swing = math.sin(phase)
                amount = {
                    "coxa.L": 0.36 * swing,
                    "coxa.R": -0.36 * swing,
                    "canela.L": -0.55 * max(0, swing),
                    "canela.R": -0.55 * max(0, -swing),
                    "braço1.L": -0.24 * swing,
                    "braço1.R": 0.24 * swing,
                    "braço2.L": -0.10 - 0.07 * swing,
                    "braço2.R": -0.10 + 0.07 * swing,
                    "peito": 0.035 * math.sin(phase * 2),
                }.get(b.name, 0)
            elif b.name == "peito":
                amount = 0.018 * math.sin(phase)
            axis = pose[b.name].to_3x3().inverted() @ Vector((1, 0, 0))
            b.rotation_quaternion = b.rotation_quaternion @ Quaternion(axis, amount)
            if b.name == "master":
                # Convert a small vertical bob from rig space into the root bone basis.
                bob = (
                    (0.035 * (1 - math.cos(phase * 2)))
                    if name == "Nox_Walk"
                    else 0.015 * math.sin(phase)
                )
                b.location += b.bone.matrix_local.to_3x3().inverted() @ Vector((0, bob, 0))
            b.keyframe_insert("rotation_quaternion", frame=frame)
            b.keyframe_insert("location", frame=frame)
            b.keyframe_insert("scale", frame=frame)
    for curve in a.fcurves:
        for key in curve.keyframe_points:
            key.interpolation = "LINEAR"
    keep.append(a)
bpy.context.scene.render.fps = 30
# Materials are new colors, not reference images packed in the original blend.
colors = [
    ("Nox_Fur", (0.62, 0.20, 0.075, 1)),
    ("Nox_Cream", (0.88, 0.78, 0.58, 1)),
    ("Nox_Cloak", (0.055, 0.13, 0.16, 1)),
    ("Nox_Dark", (0.045, 0.028, 0.022, 1)),
    ("Nox_Eyes", (0.045, 0.32, 0.30, 1)),
]
mesh.data.materials.clear()
for name, c in colors:
    m = bpy.data.materials.new(name)
    m.diffuse_color = c
    m.use_nodes = True
    m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = c
    m.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 0.8
    mesh.data.materials.append(m)
adj = [set() for _ in mesh.data.vertices]
for edge in mesh.data.edges:
    a, b = edge.vertices
    adj[a].add(b)
    adj[b].add(a)
seen = set()
which = {}
number = 0
for vertex in mesh.data.vertices:
    if vertex.index in seen:
        continue
    stack = [vertex.index]
    seen.add(vertex.index)
    while stack:
        i = stack.pop()
        which[i] = number
        for j in adj[i] - seen:
            seen.add(j)
            stack.append(j)
    number += 1
if number != 7:
    raise RuntimeError("Topologia da referência Nox.blend diferente da analisada.")
for p in mesh.data.polygons:
    c = p.center
    island = which[p.vertices[0]]
    if island == 6:
        idx = 2
    elif island == 0:
        idx = 3
    elif island in [2, 4]:
        idx = 4
    elif island in [3, 5]:
        idx = 1
    elif c.z < 4.3 or (8.2 < c.z < 9.1 and c.y < -2.65):
        idx = 3
    elif c.z > 8.1 and c.y < -1.3:
        idx = 1
    else:
        idx = 0
    p.material_index = idx
# Supplementary actions use the same evaluated rig-space axes as locomotion.
poses = [
    ("Nox_Jump", [(0, 0), (6, 1), (18, 1)]),
    ("Nox_Fall", [(0, 1), (18, 1)]),
    ("Nox_Land", [(0, 1), (8, 0)]),
    ("Nox_Death", [(0, 0), (10, 0.65), (24, 1)]),
    ("Nox_Attack", [(0, 0), (4, -0.35), (7, 1), (18, 0)]),
]
for name, frames in poses:
    a = bpy.data.actions.new(name)
    a.use_fake_user = True
    rig.animation_data.action = a
    for frame, factor in frames:
        for b in rig.pose.bones:
            b.matrix_basis = base[b.name]
            b.rotation_mode = "QUATERNION"
            if name == "Nox_Jump":
                amount = {
                    "coxa.L": 0.24,
                    "coxa.R": 0.24,
                    "canela.L": -0.35,
                    "canela.R": -0.35,
                    "braço1.L": -0.25,
                    "braço1.R": -0.25,
                }.get(b.name, 0) * factor
            elif name == "Nox_Fall":
                amount = {
                    "coxa.L": 0.10,
                    "coxa.R": 0.10,
                    "canela.L": -0.18,
                    "canela.R": -0.18,
                    "braço1.L": 0.24,
                    "braço1.R": 0.24,
                    "peito": 0.08,
                }.get(b.name, 0) * factor
            elif name == "Nox_Land":
                amount = {
                    "coxa.L": 0.18,
                    "coxa.R": 0.18,
                    "canela.L": -0.3,
                    "canela.R": -0.3,
                    "peito": 0.1,
                }.get(b.name, 0) * factor
            elif name == "Nox_Death":
                amount = {
                    "master": -1.3,
                    "canela.L": -0.3,
                    "canela.R": -0.3,
                    "braço1.L": -0.35,
                    "braço1.R": -0.35,
                }.get(b.name, 0) * factor
            else:
                amount = {
                    "peito": 0.35,
                    "barriga": 0.12,
                    "braço1.L": -0.8,
                    "braço1.R": -0.65,
                    "braço2.L": -0.3,
                    "braço2.R": -0.3,
                }.get(b.name, 0) * factor
            world_axis = (
                (0, 1, 0) if name == "Nox_Attack" and b.name in ["peito", "barriga"] else (1, 0, 0)
            )
            axis = pose[b.name].to_3x3().inverted() @ Vector(world_axis)
            b.rotation_quaternion = b.rotation_quaternion @ Quaternion(axis, amount)
            if b.name == "master" and name in ["Nox_Land", "Nox_Death"]:
                amount = -0.10 * factor if name == "Nox_Land" else -0.18 * factor
                b.location += b.bone.matrix_local.to_3x3().inverted() @ Vector((0, amount, 0))
            b.keyframe_insert("rotation_quaternion", frame=frame)
            b.keyframe_insert("location", frame=frame)
            b.keyframe_insert("scale", frame=frame)
    keep.append(a)
rig.animation_data.action = next(a for a in keep if a.name == "Nox_Idle")
bpy.context.scene.frame_set(1)
# Center/export using a separate wrapper; transforms of the original rig are retained.
wrapper = bpy.data.objects.new("Nox", None)
bpy.context.collection.objects.link(wrapper)
world = rig.matrix_world.copy()
rig.parent = wrapper
rig.matrix_world = world
coords = [mesh.matrix_world @ Vector(c) for c in mesh.bound_box]
center = sum(coords, Vector()) / 8
h = max(v.z for v in coords) - min(v.z for v in coords)
fwd = mesh.matrix_world.to_3x3() @ Vector((0, -1, 0))
fwd.z = 0
angle = math.atan2(fwd.y, fwd.x)
yaw = -math.pi / 2 - angle
rotation = Matrix.Rotation(yaw, 4, "Z")
scale = 1.7 / h
rotcenter = rotation @ center
wrapper.matrix_world = (
    Matrix.Translation(
        Vector((-rotcenter.x * scale, -rotcenter.y * scale, -min(v.z for v in coords) * scale))
    )
    @ rotation
    @ Matrix.Scale(scale, 4)
)
bpy.context.view_layer.update()
# Twin low-poly hatchets on the back, as described by the GDD.
props = []
metal = bpy.data.materials.new("Nox_AxeSteel")
metal.diffuse_color = (0.38, 0.47, 0.49, 1)
wood = bpy.data.materials.new("Nox_AxeWood")
wood.diffuse_color = (0.22, 0.10, 0.045, 1)


def part(name, verts, faces, mat):
    data = bpy.data.meshes.new(name)
    data.from_pydata(verts, [], faces)
    data.materials.append(mat)
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    obj.matrix_world = mesh.matrix_world.copy()
    group = obj.vertex_groups.new(name="peito")
    group.add(list(range(len(verts))), 1, "REPLACE")
    mod = obj.modifiers.new("NoxRig", "ARMATURE")
    mod.object = rig
    obj.parent = rig
    obj.matrix_world = mesh.matrix_world.copy()
    props.append(obj)


for sign in [-1, 1]:
    a = Vector((sign * -1.1, 1.1, 4.9))
    b = Vector((sign * 1.2, 1.1, 7.5))
    d = (b - a).normalized()
    side = Vector((d.z, 0, -d.x)) * 0.1
    verts = [
        tuple(p + side * u + Vector((0, 0.09 * v, 0)))
        for p in [a, b]
        for u, v in [(-1, -1), (-1, 1), (1, 1), (1, -1)]
    ]
    faces = [(0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)]
    part("MachadinhaCabo" + str(sign), verts, faces, wood)
    points = [b - d * 0.4, b + d * 0.35, b + d * 0.5 + side * 7, b - d * 0.55 + side * 7]
    verts = [tuple(p + Vector((0, y, 0))) for y in [-0.1, 0.1] for p in points]
    part("MachadinhaLamina" + str(sign), verts, faces, metal)

# Optional validation snapshots are local evidence, not shipped reference assets.
reference = {}
for action in keep:
    rig.animation_data.action = action
    for fraction in [0, 0.25, 0.5, 0.75, 1]:
        frame = int(
            action.frame_range[0] + (action.frame_range[1] - action.frame_range[0]) * fraction
        )
        bpy.context.scene.frame_set(frame)
        bpy.context.view_layer.update()
        evaluated_mesh = mesh.evaluated_get(bpy.context.evaluated_depsgraph_get())
        data = evaluated_mesh.to_mesh()
        reference[action.name + ":" + str(fraction)] = [
            list(evaluated_mesh.matrix_world @ vertex.co) for vertex in data.vertices
        ]
        evaluated_mesh.to_mesh_clear()
os.makedirs(os.path.abspath(opts.output), exist_ok=True)
with open(os.path.join(opts.output, "poses-antes-da-exportacao.json"), "w") as handle:
    json.dump(reference, handle)
rig.animation_data.action = next(a for a in keep if a.name == "Nox_Idle")
bpy.context.scene.frame_set(1)
bpy.context.view_layer.update()

# Normalize the actual bind hierarchy, not only the enclosing preview object.
# The exported armature and skinned meshes have identity object transforms.
# Animation is baked explicitly per bone; object rotations cannot tip the rig.
old_rig = rig
bpy.context.scene.frame_set(1)
bpy.context.view_layer.update()
rest = {b.name: old_rig.matrix_world @ b.matrix_local for b in old_rig.data.bones}
armature = bpy.data.armatures.new("NoxSkeleton")
clean = bpy.data.objects.new("NoxRigNormalized", armature)
bpy.context.collection.objects.link(clean)
bpy.context.view_layer.objects.active = clean
clean.select_set(True)
bpy.ops.object.mode_set(mode="EDIT")
for bone in old_rig.data.bones:
    item = armature.edit_bones.new(bone.name)
    item.head = old_rig.matrix_world @ bone.head_local
    item.tail = old_rig.matrix_world @ bone.tail_local
    item.align_roll(rest[bone.name].to_3x3().col[2])
    item.use_deform = bone.use_deform
    if bone.parent:
        item.parent = armature.edit_bones[bone.parent.name]
bpy.ops.object.mode_set(mode="OBJECT")
# Copy geometry in the same normalized world space, preserving weights/materials.
clean_meshes = []
for source in [mesh] + props:
    obj = source.copy()
    obj.data = source.data.copy()
    bpy.context.collection.objects.link(obj)
    obj.parent = clean
    obj.matrix_parent_inverse = Matrix.Identity(4)
    obj.data.transform(source.matrix_world)
    obj.matrix_world = Matrix.Identity(4)
    for modifier in obj.modifiers:
        if modifier.type == "ARMATURE":
            modifier.object = clean
    clean_meshes.append(obj)
clean.animation_data_create()
baked = []
for action in keep:
    old_rig.animation_data.action = action
    new = bpy.data.actions.new(action.name + "_normalized")
    new.use_fake_user = True
    clean.animation_data.action = new
    for frame in range(int(action.frame_range[0]), int(action.frame_range[1]) + 1):
        bpy.context.scene.frame_set(frame)
        bpy.context.view_layer.update()
        evaluated = {}
        for source_bone in old_rig.pose.bones:
            world_pose = old_rig.matrix_world @ source_bone.matrix
            location, rotation, scale = world_pose.decompose()
            rest_scale = rest[source_bone.name].to_scale()
            scale = Vector(tuple(scale[i] / rest_scale[i] for i in range(3)))
            evaluated[source_bone.name] = Matrix.LocRotScale(location, rotation, scale)
        for bone in clean.pose.bones:
            target = evaluated[bone.name]
            local_rest = bone.bone.matrix_local
            if bone.parent:
                target = evaluated[bone.parent.name].inverted() @ target
                local_rest = bone.parent.bone.matrix_local.inverted() @ local_rest
            bone.matrix_basis = local_rest.inverted() @ target
            bone.rotation_mode = "QUATERNION"
            bone.keyframe_insert("location", frame=frame)
            bone.keyframe_insert("rotation_quaternion", frame=frame)
            bone.keyframe_insert("scale", frame=frame)
    for curve in new.fcurves:
        for key in curve.keyframe_points:
            key.interpolation = "LINEAR"
    baked.append(new)
# Remove old datablocks only from this working copy, keeping source .blend intact.
for action in keep:
    bpy.data.actions.remove(action)
for action in baked:
    action.name = action.name.removesuffix("_normalized")
for obj in [mesh] + props + [old_rig, wrapper]:
    bpy.data.objects.remove(obj, do_unlink=True)
rig = clean
rig.name = "NoxRig"
mesh = clean_meshes[0]
mesh.name = "panda"
props = clean_meshes[1:]
wrapper = rig
keep = baked
rig.animation_data.action = next(a for a in keep if a.name == "Nox_Idle")
bpy.context.scene.frame_set(1)
bpy.context.view_layer.update()

# Only the playable model/rig are exported: no packed references or cameras.
bpy.ops.object.select_all(action="DESELECT")
for o in [rig, mesh] + props:
    o.select_set(True)
bpy.context.view_layer.objects.active = rig
out = os.path.abspath(opts.output)
os.makedirs(out, exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=out + "/Nox.fbx",
    use_selection=True,
    object_types={"EMPTY", "ARMATURE", "MESH"},
    add_leaf_bones=False,
    bake_anim=True,
    bake_anim_use_all_actions=True,
    bake_anim_use_nla_strips=False,
    bake_anim_simplify_factor=0,
    axis_forward="-Z",
    axis_up="Y",
    path_mode="STRIP",
    use_custom_props=False,
)
# Render the actual exported design.
for o in bpy.data.objects:
    o.hide_render = o not in [rig, mesh, wrapper] + props
coords = [mesh.matrix_world @ Vector(c) for c in mesh.bound_box]
center = sum(coords, Vector()) / 8
bpy.ops.object.camera_add(location=center + Vector((2.5, -4, 1.5)))
cam = bpy.context.object
cam.rotation_euler = (center - cam.location).to_track_quat("-Z", "Y").to_euler()
cam.data.type = "ORTHO"
cam.data.ortho_scale = 2.4
cam.hide_render = False
bpy.context.scene.camera = cam
bpy.ops.object.light_add(type="AREA", location=(2, -3, 5))
bpy.context.object.data.energy = 450
bpy.context.object.data.size = 4
bpy.context.object.hide_render = False
scene = bpy.context.scene
scene.world = bpy.data.worlds.new("PreviewWorld")
scene.world.color = (0.18, 0.18, 0.18)
scene.render.engine = "CYCLES"
scene.cycles.samples = 16
scene.render.resolution_x = 700
scene.render.resolution_y = 700
scene.render.resolution_percentage = 100
scene.render.filepath = out + "/nox-preview.png"
bpy.ops.render.render(write_still=True)
print("EXPORTED", os.path.getsize(out + "/Nox.fbx"), flush=True)
