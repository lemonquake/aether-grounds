"""Original wheel catalog, suspension meshes and motion studies, authored in Blender.
Run: blender --background --python Tools/build_wheel_assets.py
Unity uses the exported local mesh geometry and drives it from WheelCollider state.
The Blender actions demonstrate spin, steering and compression for every design.
"""
import bpy
import json
import math
import hashlib
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Assets/Resources/Models'
SOURCE = ROOT / 'ArtSource/Wheels'
EVIDENCE = ROOT / 'Evidence/VehiclePhysics'
for directory in (OUT, SOURCE, EVIDENCE):
    directory.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

def material(name, color, metallic=0, roughness=.4):
    m=bpy.data.materials.new(name)
    m.diffuse_color=(*color,1)
    m.use_nodes=True
    bsdf=m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value=(*color,1)
    bsdf.inputs['Metallic'].default_value=metallic
    bsdf.inputs['Roughness'].default_value=roughness
    return m

rubber=material('Tire',(.032,.037,.042),0,.72)
rim=material('Wheel',(.42,.52,.61),.85,.24)
steel=material('BrakeRotor',(.30,.33,.36),.82,.35)
caliper=material('BrakeCaliper',(.67,.075,.038),.6,.3)
springmat=material('SuspensionSpring',(.96,.59,.085),.72,.28)
armmat=material('SuspensionMetal',(.13,.17,.20),.78,.35)
chromemat=material('DamperChrome',(.61,.65,.70),.96,.16)

def mesh(name, vertices, faces, mat):
    data=bpy.data.meshes.new(name)
    data.from_pydata(vertices,[],faces)
    data.update()
    obj=bpy.data.objects.new(name,data)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    return obj

def box(name, location, size, mat, bevel=.006):
    bpy.ops.mesh.primitive_cube_add(size=1,location=location)
    obj=bpy.context.object
    obj.name=name
    obj.scale=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    obj.data.materials.append(mat)
    if bevel:
        mod=obj.modifiers.new('Machined edges','BEVEL')
        mod.width=bevel
        mod.segments=2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return obj

def ring(name, profile, mat, sides=48):
    verts=[(x,math.sin(a*math.tau/sides)*r,math.cos(a*math.tau/sides)*r) for x,r in profile for a in range(sides)]
    faces=[]
    for row in range(len(profile)):
        for j in range(sides):
            faces.append((row*sides+j,row*sides+(j+1)%sides,((row+1)%len(profile))*sides+(j+1)%sides,((row+1)%len(profile))*sides+j))
    obj=mesh(name,verts,faces,mat)
    for p in obj.data.polygons:p.use_smooth=True
    return obj

def tube(name, points, radius, mat, sides=8):
    verts=[];faces=[]
    for i,point in enumerate(points):
        tangent=(Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])).normalized()
        u=tangent.cross(Vector((1,0,0)))
        if u.length<.01:u=tangent.cross(Vector((0,1,0)))
        u.normalize();v=tangent.cross(u).normalized()
        for j in range(sides):verts.append(Vector(point)+radius*(u*math.cos(j*math.tau/sides)+v*math.sin(j*math.tau/sides)))
    for i in range(len(points)-1):
        for j in range(sides):
            a=i*sides+j;b=i*sides+(j+1)%sides
            faces.append((a,b,b+sides,a+sides))
    faces.extend([tuple(range(sides-1,-1,-1)),tuple(range((len(points)-1)*sides,len(points)*sides))])
    return mesh(name,verts,faces,mat)

manifest=[]
def export_model(name, objects):
    bpy.context.view_layer.update()
    deps=bpy.context.evaluated_depsgraph_get();parts=[];triangles=0
    for obj in objects:
        if obj.type!='MESH':continue
        evaluated=obj.evaluated_get(deps);m=evaluated.to_mesh();m.calc_loop_triangles()
        vertices=[]
        for vertex in m.vertices:
            p=obj.matrix_world@vertex.co
            vertices.extend([round(p.x,6),round(p.z,6),round(-p.y,6)])
        indices=[i for face in m.loop_triangles for i in face.vertices]
        mat=obj.data.materials[0]
        parts.append(dict(name=obj.name,material=mat.name,color=list(mat.diffuse_color),vertices=vertices,triangles=indices))
        triangles+=len(indices)//3
        evaluated.to_mesh_clear()
    path=OUT/(name+'.json')
    path.write_text(json.dumps(dict(name=name,parts=parts),separators=(',',':')))
    assert triangles<12000,(name,triangles)
    manifest.append(dict(name=name,triangles=triangles,parts=len(parts),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))

names=['Five spoke','Split spoke','Turbofan','Wire mesh','Rally disc','Beadlock','Six spoke','Deep dish','Aero blade','Honeycomb']
catalog=[]
for kind,name in enumerate(names):
    start=set(bpy.data.objects)
    r=.5;w=.34
    # Rounded shoulders, inner bead, visible tread blocks and a hollow tire.
    ring('Tire sidewall',[(w/2,.34),(w/2,.445),(w*.40,.49),(w*.25,.5),(-w*.25,.5),(-w*.40,.49),(-w/2,.445),(-w/2,.34)],rubber,64)
    ring('Wheel barrel',[(-.178,.335),(-.178,.395),(-.145,.402),(.145,.402),(.178,.395),(.178,.335)],rim)
    for side in (-1,1):
        depth=side*(.09 if kind==7 else .17)
        ring('Wheel polished lip',[(depth,.34),(depth,.394),(depth+side*.018,.397),(depth+side*.018,.34)],rim)
        ring('Wheel center',[(depth-.018,.001),(depth-.018,.083),(depth+.018,.083),(depth+.018,.001)],rim,24)
        count=[5,10,12,20,8,8,6,5,7,12][kind]
        for j in range(count):
            a=j*math.tau/count
            bend=.28 if kind in (1,8) else (.18 if kind==3 and j%2 else -.18 if kind==3 else 0)
            if kind!=9:
                thickness=.016 if kind==3 else .025 if kind==1 else .045 if kind==2 else .031
                tube('Wheel spoke',[(depth,math.sin(a)*.07,math.cos(a)*.07),(depth,math.sin(a+bend)*.21,math.cos(a+bend)*.21),(depth,math.sin(a+bend)*.36,math.cos(a+bend)*.36)],thickness,rim,6)
            if kind in (5,9):
                ring_obj=ring('Wheel bead bolt',[(-.012,.001),(-.012,.018),(.012,.018),(.012,.001)],steel,8)
                ring_obj.location=(depth+side*.027,math.sin(a)*.367,math.cos(a)*.367)
        if kind==9:
            for cell in range(6):
                a=cell*math.tau/6
                center=Vector((depth,math.sin(a)*.23,math.cos(a)*.23))
                points=[center+Vector((0,math.sin(j*math.tau/6)*.105,math.cos(j*math.tau/6)*.105)) for j in range(7)]
                tube('Wheel hex cell',points,.016,rim,6)
        if kind==4:
            ring('Wheel rally cover',[(depth,.075),(depth,.31),(depth+side*.018,.31),(depth+side*.018,.075)],rim,40)
        for j in range(5):
            a=j*math.tau/5
            bolt=ring('Wheel lug nut',[(-.01,.001),(-.01,.014),(.01,.014),(.01,.001)],steel,6)
            bolt.location=(depth+side*.022,math.sin(a)*.058,math.cos(a)*.058)
    for j in range(40):
        a=j*math.tau/40
        for side in (-1,1):
            tread=box('Tire tread block',(side*.075,math.sin(a)*.497,math.cos(a)*.497),(.137,.048 if kind==5 else .027,.012),rubber,0)
            tread.rotation_euler.x=-a
    ring('Brake ventilated rotor',[(-.125,.09),(-.125,.28),(-.105,.28),(-.105,.09)],steel,48)
    objects=list(set(bpy.data.objects)-start)
    export_model('WheelStyle'+str(kind),objects)
    # Authored rig: body attachment -> steering knuckle -> spinning assembly.
    bpy.ops.object.empty_add();mount=bpy.context.object;mount.name='Suspension mount '+name
    bpy.ops.object.empty_add();knuckle=bpy.context.object;knuckle.name='Steering knuckle '+name;knuckle.parent=mount
    bpy.ops.object.empty_add();spin=bpy.context.object;spin.name='Wheel rotation '+name;spin.parent=knuckle
    for obj in objects:obj.parent=spin
    for frame,angle in [(1,0),(61,math.tau),(121,math.tau*2)]:
        spin.rotation_euler.x=angle;spin.keyframe_insert(data_path='rotation_euler',frame=frame)
    for frame,angle in [(1,0),(31,.55),(61,0),(91,-.55),(121,0)]:
        knuckle.rotation_euler.z=angle;knuckle.keyframe_insert(data_path='rotation_euler',frame=frame)
    for frame,travel in [(1,0),(21,-.16),(31,.16),(46,-.08),(61,0),(91,.1),(121,0)]:
        knuckle.location.z=travel;knuckle.keyframe_insert(data_path='location',frame=frame)
    mount.location=(kind%5*1.55,kind//5*1.65,0)
    catalog.append(mount)

# Shared parts retain a unit Y (Unity) / Z (Blender) length for real travel scaling.
for name,build in [
    ('SuspensionSpring',lambda: tube('Suspension coil',[(.067*math.cos(i*math.tau/16),.067*math.sin(i*math.tau/16),i/128) for i in range(129)],.012,springmat,6)),
    ('SuspensionArm',lambda: tube('Suspension control arm',[(0,0,0),(0,0,1)],.027,armmat,8)),
    ('SuspensionDamper',lambda: tube('Damper cylinder',[(0,0,0),(0,0,1)],.041,chromemat,12)),
    ('BrakeCaliper',lambda: box('Brake caliper',(-.105,0,.23),(.085,.19,.16),caliper,.015)),
    ('WheelUpright',lambda: box('Steering upright',(0,0,0),(.055,.095,.28),armmat,.012)),
]:
    start=set(bpy.data.objects);build();objects=list(set(bpy.data.objects)-start)
    export_model(name,objects)
    for obj in objects:obj.hide_render=True;obj.hide_set(True)

scene=bpy.context.scene
scene.frame_start=1;scene.frame_end=121;scene.render.fps=60;scene.frame_set(1)
scene.world.color=(.12,.12,.12)
bpy.ops.object.camera_add(location=(10,-9,7))
camera=bpy.context.object;camera.rotation_euler=(Vector((3,.75,0))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=9;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(3,-3,8));bpy.context.object.data.energy=1800;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=7
bpy.ops.object.light_add(type='AREA',location=(3,6,4));bpy.context.object.data.energy=1300;bpy.context.object.data.size=6
scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.resolution_x=1600;scene.render.resolution_y=950;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(EVIDENCE/'wheel-catalog.png')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'AetherWheels.blend'))
bpy.ops.render.render(write_still=True)
# GLB is an inspectable animated interchange copy; runtime meshes use JSON.
bpy.ops.export_scene.gltf(filepath=str(SOURCE/'AetherWheels.glb'),export_format='GLB',export_animations=True,export_cameras=False,export_lights=False)
report=dict(license='Original project-authored geometry',units='metres',wheel_radius=.5,designs=names,assets=manifest,
    blender_version=bpy.app.version_string,animation_frames=[1,121],fps=60,
    source='ArtSource/Wheels/AetherWheels.blend',interchange='ArtSource/Wheels/AetherWheels.glb',
    runtime_animation='Independent wheel contact, steering, spin, caliper, damper and spring motion driven by WheelCollider telemetry')
(EVIDENCE/'wheel-assets.json').write_text(json.dumps(report,indent=2))
print('WHEEL_ASSETS_COMPLETE '+json.dumps(dict(assets=len(manifest),triangles=sum(a['triangles'] for a in manifest))))
