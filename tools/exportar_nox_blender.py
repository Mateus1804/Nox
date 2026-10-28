import bpy,math,json,os
from mathutils import Vector,Quaternion,Matrix
import argparse,sys
args=argparse.ArgumentParser();args.add_argument('--source',required=True);args.add_argument('--output',required=True);opts=args.parse_args(sys.argv[sys.argv.index('--')+1:])
import hashlib
if hashlib.sha256(open(opts.source,'rb').read()).hexdigest() != '64f1fbfecaa22946aca67eafb90d7d5fbf9c9865377b3364482a54aa0ffc8f7e':raise RuntimeError('Arquivo Nox.blend não corresponde à referência analisada.')
bpy.ops.wm.open_mainfile(filepath=opts.source)
rig=bpy.data.objects['panda?'];mesh=bpy.data.objects['panda'];rig.name='NoxRig'
rig.animation_data.action=next(a for a in bpy.data.actions if '150f' in a.name);bpy.context.scene.frame_set(1)
base={b.name:b.matrix_basis.copy() for b in rig.pose.bones}
keep=[]
for a in list(bpy.data.actions):
 if '70f' in a.name:a.name='Nox_Walk';keep.append(a)
 elif '150f' in a.name:a.name='Nox_Idle';keep.append(a)
 else:bpy.data.actions.remove(a)
# Bake the evaluated original idle pose before removing IK on the working copy.
# Original files remain untouched. The new cycle uses FK keys on all 52 bones.
rig.animation_data.action=next(a for a in keep if a.name=='Nox_Idle')
bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
pose={b.name:b.matrix.copy() for b in rig.pose.bones}
for b in rig.pose.bones:
 for constraint in list(b.constraints):b.constraints.remove(constraint)
for b in rig.pose.bones:b.matrix=pose[b.name]
bpy.context.view_layer.update()
base={b.name:b.matrix_basis.copy() for b in rig.pose.bones}
for a in list(keep):bpy.data.actions.remove(a)
keep=[]
# Rig local Y is vertical; rotate limbs around their local X axes.
for name,duration in [('Nox_Idle',72),('Nox_Walk',24)]:
 a=bpy.data.actions.new(name);a.use_fake_user=True;rig.animation_data.action=a
 for frame in range(duration+1):
  phase=2*math.pi*frame/duration
  for b in rig.pose.bones:
   b.matrix_basis=base[b.name];b.rotation_mode='QUATERNION'
   amount=0
   if name=='Nox_Walk':
    swing=math.sin(phase)
    amount={'coxa.L':.36*swing,'coxa.R':-.36*swing,
      'canela.L':-.55*max(0,swing),'canela.R':-.55*max(0,-swing),
      'braço1.L':-.24*swing,'braço1.R':.24*swing,
      'braço2.L':-.10-.07*swing,'braço2.R':-.10+.07*swing,
      'peito':.035*math.sin(phase*2)}.get(b.name,0)
   elif b.name=='peito':amount=.018*math.sin(phase)
   axis=pose[b.name].to_3x3().inverted()@Vector((1,0,0))
   b.rotation_quaternion=b.rotation_quaternion@Quaternion(axis,amount)
   if b.name=='master':
    # Convert a small vertical bob from rig space into the root bone basis.
    bob=(.035*(1-math.cos(phase*2))) if name=='Nox_Walk' else .015*math.sin(phase)
    b.location+=b.bone.matrix_local.to_3x3().inverted()@Vector((0,bob,0))
   b.keyframe_insert('rotation_quaternion',frame=frame)
   b.keyframe_insert('location',frame=frame);b.keyframe_insert('scale',frame=frame)
 for curve in a.fcurves:
  for key in curve.keyframe_points:key.interpolation='LINEAR'
 keep.append(a)
bpy.context.scene.render.fps=30
# Materials are new colors, not reference images packed in the original blend.
colors=[('Nox_Fur',(0.62,.20,.075,1)),('Nox_Cream',(.88,.78,.58,1)),('Nox_Cloak',(.055,.13,.16,1)),('Nox_Dark',(.045,.028,.022,1)),('Nox_Eyes',(.045,.32,.30,1))]
mesh.data.materials.clear()
for name,c in colors:
 m=bpy.data.materials.new(name);m.diffuse_color=c;m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=c;m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.8;mesh.data.materials.append(m)
adj=[set() for _ in mesh.data.vertices]
for edge in mesh.data.edges:
 a,b=edge.vertices;adj[a].add(b);adj[b].add(a)
seen=set();which={};number=0
for vertex in mesh.data.vertices:
 if vertex.index in seen:continue
 stack=[vertex.index];seen.add(vertex.index)
 while stack:
  i=stack.pop();which[i]=number
  for j in adj[i]-seen:seen.add(j);stack.append(j)
 number+=1
if number!=7:raise RuntimeError('Topologia da referência Nox.blend diferente da analisada.')
for p in mesh.data.polygons:
 c=p.center;island=which[p.vertices[0]]
 if island==6:idx=2
 elif island==0:idx=3
 elif island in [2,4]:idx=4
 elif island in [3,5]:idx=1
 elif c.z<4.3 or (8.2 < c.z < 9.1 and c.y < -2.65):idx=3
 elif c.z>8.1 and c.y< -1.3:idx=1
 else:idx=0
 p.material_index=idx
# Supplementary poses: original mesh and skeleton remain unchanged.
for name,frames in [('Nox_Jump',[(0,0),(8,1),(24,1),(32,0)]),('Nox_Attack',[(0,0),(5,-.45),(10,1),(18,0)])]:
 a=bpy.data.actions.new(name);a.use_fake_user=True;rig.animation_data.action=a
 for frame,factor in frames:
  for b in rig.pose.bones:
   b.matrix_basis=base[b.name];b.rotation_mode='QUATERNION'
   q=b.rotation_quaternion.copy()
   if name=='Nox_Jump':
    amount={'coxa.L':.3,'coxa.R':.3,'braço1.L':-.35,'braço1.R':-.35}.get(b.name,0)*factor;axis=(1,0,0)
   else:amount={'peito':.45,'barriga':.15,'braço1.L':-.9,'braço1.R':-.7,'braço2.L':-.4,'braço2.R':-.4}.get(b.name,0)*factor;axis=(0,1,0) if b.name in ['peito','barriga'] else (1,0,0)
   b.rotation_quaternion=q@Quaternion(axis,amount)
   b.keyframe_insert('rotation_quaternion',frame=frame);b.keyframe_insert('location',frame=frame);b.keyframe_insert('scale',frame=frame)
 keep.append(a)
rig.animation_data.action=next(a for a in keep if a.name=='Nox_Idle');bpy.context.scene.frame_set(1)
# Center/export using a separate wrapper; transforms of the original rig are retained.
wrapper=bpy.data.objects.new('Nox',None);bpy.context.collection.objects.link(wrapper);world=rig.matrix_world.copy();rig.parent=wrapper;rig.matrix_world=world
coords=[mesh.matrix_world@Vector(c) for c in mesh.bound_box];center=sum(coords,Vector())/8;h=max(v.z for v in coords)-min(v.z for v in coords)
fwd=mesh.matrix_world.to_3x3()@Vector((0,-1,0));fwd.z=0;angle=math.atan2(fwd.y,fwd.x);yaw=-math.pi/2-angle
rotation=Matrix.Rotation(yaw,4,'Z');scale=1.7/h;rotcenter=rotation@center
wrapper.matrix_world=Matrix.Translation(Vector((-rotcenter.x*scale,-rotcenter.y*scale,-min(v.z for v in coords)*scale)))@rotation@Matrix.Scale(scale,4)
bpy.context.view_layer.update()
# Twin low-poly hatchets on the back, as described by the GDD.
props=[]
metal=bpy.data.materials.new('Nox_AxeSteel');metal.diffuse_color=(.38,.47,.49,1)
wood=bpy.data.materials.new('Nox_AxeWood');wood.diffuse_color=(.22,.10,.045,1)
def part(name,verts,faces,mat):
 data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.materials.append(mat)
 obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.matrix_world=mesh.matrix_world.copy()
 group=obj.vertex_groups.new(name='peito');group.add(list(range(len(verts))),1,'REPLACE')
 mod=obj.modifiers.new('NoxRig','ARMATURE');mod.object=rig
 obj.parent=rig;obj.matrix_world=mesh.matrix_world.copy();props.append(obj)
for sign in [-1,1]:
 a=Vector((sign*-1.1,1.1,4.9));b=Vector((sign*1.2,1.1,7.5));d=(b-a).normalized();side=Vector((d.z,0,-d.x))*.1
 verts=[tuple(p+side*u+Vector((0,.09*v,0))) for p in [a,b] for u,v in [(-1,-1),(-1,1),(1,1),(1,-1)]]
 faces=[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
 part('MachadinhaCabo'+str(sign),verts,faces,wood)
 points=[b-d*.4,b+d*.35,b+d*.5+side*7,b-d*.55+side*7]
 verts=[tuple(p+Vector((0,y,0))) for y in [-.1,.1] for p in points]
 part('MachadinhaLamina'+str(sign),verts,faces,metal)

# Only the playable model/rig are exported: no packed references or cameras.
bpy.ops.object.select_all(action='DESELECT')
for o in [wrapper,rig,mesh]+props:o.select_set(True)
bpy.context.view_layer.objects.active=rig
out=os.path.abspath(opts.output);os.makedirs(out,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=out+'/Nox.fbx',use_selection=True,object_types={'EMPTY','ARMATURE','MESH'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='STRIP',use_custom_props=False)
# Render the actual exported design.
for o in bpy.data.objects:o.hide_render=o not in [rig,mesh,wrapper]+props
from mathutils import Vector
coords=[mesh.matrix_world@Vector(c) for c in mesh.bound_box];center=sum(coords,Vector())/8
bpy.ops.object.camera_add(location=center+Vector((2.5,-4,1.5)));cam=bpy.context.object;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.4;cam.hide_render=False;bpy.context.scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(2,-3,5));bpy.context.object.data.energy=450;bpy.context.object.data.size=4;bpy.context.object.hide_render=False
scene=bpy.context.scene;scene.world=bpy.data.worlds.new('PreviewWorld');scene.world.color=(.18,.18,.18);scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=700;scene.render.resolution_y=700;scene.render.resolution_percentage=100;scene.render.filepath=out+'/nox-preview.png';bpy.ops.render.render(write_still=True)
print('EXPORTED',os.path.getsize(out+'/Nox.fbx'),flush=True)
