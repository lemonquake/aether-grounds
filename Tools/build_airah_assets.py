"""Original Airah Mountains assets, authored and exported with Blender. Run with blender -b -P."""
import bpy, math, json, random, os
from mathutils import Vector
ROOT = 'A:/Python/aether-grounds'
OUT = ROOT + '/Assets/Resources/Models'
random.seed(190926)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
def material(name, color):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*color,1)
    m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.84
    return m
bark=material('Airah Bark',(.24,.17,.10))
needles=material('Airah Needles',(.16,.29,.09))
leaves=material('Airah Leaves',(.30,.43,.13))
stone=material('Airah Granite',(.50,.51,.43))
wood=material('Airah Timber',(.38,.22,.105))
roof=material('Airah Roof',(.17,.25,.22))
paint=material('Airah Car Paint',(.64,.34,.12))
glass=material('Airah Glass',(.09,.20,.24))
tire=material('Airah Tire',(.035,.04,.042))
metal=material('Airah Metal',(.38,.43,.43))
lamp=material('Airah Headlight',(.94,.88,.64))
tex=bpy.data.images.load(ROOT+'/Assets/Resources/Airah/GraniteMoss.png')
node=stone.node_tree.nodes.new('ShaderNodeTexImage');node.image=tex
stone.node_tree.links.new(node.outputs['Color'],stone.node_tree.nodes['Principled BSDF'].inputs['Base Color'])
def box(name,p,s,mat):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(mat);return o
def cone(name,p,r1,r2,h,mat,n=8):
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=h,location=p);o=bpy.context.object;o.name=name;o.data.materials.append(mat);return o
def beam(name,a,b,r,mat):
    a,b=Vector(a),Vector(b);o=cone(name,(a+b)/2,r,r,(b-a).length,mat,6);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def export(name,objects):
    parts=[];deps=bpy.context.evaluated_depsgraph_get()
    for o in objects:
        if o.type!='MESH':continue
        ev=o.evaluated_get(deps);me=ev.to_mesh();me.calc_loop_triangles();vs=[]
        for v in me.vertices:
            p=o.matrix_world@v.co;vs.extend([round(p.x,5),round(p.z,5),round(-p.y,5)])
        mat=o.data.materials[0];parts.append(dict(name=o.name,material=mat.name,color=list(mat.diffuse_color),vertices=vs,triangles=[v for t in me.loop_triangles for v in t.vertices]))
        ev.to_mesh_clear()
    with open(OUT+'/'+name+'.json','w') as f:json.dump(dict(name=name,parts=parts),f,separators=(',',':'))
    print('AIRAH ASSET',name,'triangles',sum(len(p['triangles'])//3 for p in parts))
    return parts
catalog=[]
def finish(name,before):
    objects=list(set(bpy.data.objects)-before);export(name,objects)
    empty=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(empty)
    for o in objects:o.parent=empty
    empty.location=(len(catalog)%5*18,len(catalog)//5*20,0);catalog.append(empty)
for variant in range(3):
    before=set(bpy.data.objects);h=12+variant*3
    cone('Pine trunk',(0,0,h*.36),.38,.12,h*.72,bark,7)
    for k in range(6):
        z=3+k*(h-4)/6;radius=(h-z)*.27
        o=cone('Whorled pine needles',(0,0,z+1.7),radius,.04,4.3,needles,9)
        for v in o.data.vertices:
            if v.co.z<0:v.co.z+=random.uniform(-.6,.3);v.co.x*=random.uniform(.83,1.12);v.co.y*=random.uniform(.83,1.12)
        o.rotation_euler.z=k*.83+variant
    finish('AirahPine'+str(variant),before)
before=set(bpy.data.objects)
cone('Broadleaf trunk',(0,0,3),.43,.2,6,bark)
for j in range(7):
    a=j*2.4;p=(math.cos(a)*2.2,math.sin(a)*2.2,5.5+j%3)
    beam('Branch',(0,0,3.5),p,.13,bark)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=p);o=bpy.context.object;o.name='Broadleaf crown';o.scale=(2.9,2.5,2.5);o.data.materials.append(leaves)
finish('AirahBroadleaf',before)
before=set(bpy.data.objects)
cone('Distant pine trunk',(0,0,3),.3,.15,6,bark,5)
cone('Distant pine silhouette',(0,0,8.5),2.5,.02,11,needles,7)
finish('AirahPineFar',before)
before=set(bpy.data.objects)
cone('Distant broadleaf trunk',(0,0,2.8),.38,.16,5.6,bark,5)
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(0,0,6));o=bpy.context.object;o.name='Distant leafy crown';o.scale=(4,3.5,3.4);o.data.materials.append(leaves)
finish('AirahBroadleafFar',before)
for variant in range(4):
    before=set(bpy.data.objects)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1);o=bpy.context.object;o.name='Layered fractured granite';o.data.materials.append(stone)
    for v in o.data.vertices:
        v.co.x*=random.uniform(.78,1.3)*2.4;v.co.y*=random.uniform(.8,1.3)*1.7;v.co.z=max(-.2,v.co.z*random.uniform(.8,1.2)*2+1)
        v.co.x+=v.co.z*.18
    finish('AirahRock'+str(variant),before)
before=set(bpy.data.objects)
for j in range(9):
    a=j*2.4;beam('Fern stem',(0,0,.05),(math.cos(a)*.7,math.sin(a)*.7,.5+j%3*.2),.025,leaves)
    o=cone('Fern frond',(math.cos(a)*.45,math.sin(a)*.45,.4),.25,.02,.75,leaves,5);o.rotation_euler=(.6*math.sin(a),.6*math.cos(a),a)
finish('AirahFern',before)
before=set(bpy.data.objects)
beam('Fallen cedar',(-2.7,0,.5),(2.7,0,.5),.48,bark)
for j in range(3):beam('Snapped branch',(j-1,0,.5),(j-.8,.9,.9),.12,bark)
finish('AirahLog',before)
before=set(bpy.data.objects)
for x in [-2.8,2.8]:
    for y in [-2,2]:box('Shelter posts',(x,y,2),( .22,.22,4),wood)
for s in [-1,1]:
    o=box('Pitched metal roof',(s*1.65,0,4.4),(3.7,5.5,.16),roof);o.rotation_euler.y=s*math.radians(22)
box('Picnic table',(0,0,1.05),(3,1.6,.16),wood)
for s in [-1,1]:
    box('Bench',(0,s*1.4,.65),(3.4,.5,.14),wood)
    for x in [-1,1]:box('Bench feet',(x,s*1.4,.3),(.15,.3,.6),wood)
finish('AirahShelter',before)
for variant in range(2):
    before=set(bpy.data.objects)
    box('Civilian chassis',(0,0,.65),(1.82,4.1,.62),paint)
    box('Civilian hood',(0,-1.35,1.02),(1.76,1.1,.2),paint)
    box('Cabin glass',(0,.15,1.36),(1.55,1.92,.66),glass)
    box('Cabin roof',(0,.22,1.73),(1.64,1.94,.13),paint)
    for x in [-.78,.78]:box('Window pillar',(x,.1,1.36),(.07,.10,.68),paint)
    for x in [-.62,.62]:box('Headlamps',(x,-2.065,.89),(.43,.04,.24),lamp)
    for y in [-2.09,2.09]:box('Bumper',(0,y,.52),(1.82,.11,.15),metal)
    if variant:box('Luggage rack',(0,.25,1.9),(1.3,1.45,.12),metal);box('Travel luggage',(0,.25,2.15),(1.1,1.25,.45),roof)
    for x in [-.92,.92]:
        for y in [-1.3,1.3]:
            o=cone('Road wheel',(x,y,.42),.39,.39,.24,tire,12);o.rotation_euler.y=math.pi/2
            o=cone('Wheel hub',(x*1.025,y,.42),.22,.22,.255,metal,10);o.rotation_euler.y=math.pi/2
    finish('AirahCivilian'+str(variant),before)
bpy.ops.object.select_all(action='DESELECT')
for ob in catalog:ob.select_set(True)
bpy.context.scene.world.color=(.35,.35,.35)
bpy.ops.object.light_add(type='SUN',location=(20,-20,40));bpy.context.object.rotation_euler=(.4,-.5,-.4);bpy.context.object.data.energy=2
bpy.ops.object.camera_add(location=(90,-100,86));cam=bpy.context.object;cam.rotation_euler=(Vector((34,20,4))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=115;bpy.context.scene.camera=cam
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1500;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
for a in bpy.context.screen.areas:
    if a.type=='VIEW_3D':a.spaces.active.region_3d.view_distance=100;a.spaces.active.region_3d.view_location=(34,20,4)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/AirahMountains.blend')
scene.render.filepath=ROOT+'/Evidence/Airah/blender-assets.png';bpy.ops.render.render(write_still=True)
