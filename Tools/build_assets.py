import bpy, math, json, os, random
from mathutils import Vector
ROOT = r'A:/Python/aether-grounds'
OUT = ROOT + '/Assets/Resources/Models'
os.makedirs(OUT, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
random.seed(44)
def mat(name, color, metal=0, emission=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Metallic'].default_value=metal; p.inputs['Roughness'].default_value=.32
    p.inputs['Emission Color'].default_value=(*color,1); p.inputs['Emission Strength'].default_value=emission
    return m
paint=mat('Paint',(.045,.057,.078),.55); trim=mat('Accent',(.95,.025,.055),.5)
glass=mat('Glass',(.065,.19,.24),.7); rubber=mat('Tire',(.017,.024,.034)); rim=mat('Wheel',(.26,.33,.39),.8)
dark=mat('Carbon',(.025,.035,.045)); light=mat('Headlight',(.62,.95,1),.2,2)
tail=mat('Taillight',(1,.018,.065),.2,1.5); chrome=mat('Metal',(.39,.47,.53),.8)
bark=mat('Bark',(.105,.16,.24)); leaf=mat('Leaves',(.18,.67,.55)); crystal=mat('Crystal',(.22,.85,.87),.2,.3)
rock=mat('Rock',(.28,.23,.45)); road=mat('Road',(.11,.15,.21)); building=mat('Building',(.19,.23,.33))
def mesh(name, verts, faces, material):
    me=bpy.data.meshes.new(name); me.from_pydata(verts,[],faces); me.update()
    ob=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(ob); ob.data.materials.append(material); return ob
def box(name, loc, scale, material, bevel=.04):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc); o=bpy.context.object; o.name=name; o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); o.data.materials.append(material)
    if bevel:
        mod=o.modifiers.new('Soft machined edges','BEVEL');mod.width=bevel;mod.segments=2
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    return o
def tube(name, points, radius, material, sides=8):
    vs=[];fs=[]
    for i,p in enumerate(points):
        t=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])
        t.normalize(); u=t.cross(Vector((0,0,1)))
        if u.length<.01:u=t.cross(Vector((0,1,0)))
        u.normalize();v=t.cross(u).normalized()
        for j in range(sides):vs.append(Vector(p)+radius*(u*math.cos(j*2*math.pi/sides)+v*math.sin(j*2*math.pi/sides)))
    for i in range(len(points)-1):
        for j in range(sides):a=i*sides+j;b=i*sides+(j+1)%sides;fs.append((a,b,b+sides,a+sides))
    fs.extend([tuple(range(sides-1,-1,-1)),tuple(range((len(points)-1)*sides,len(points)*sides))])
    return mesh(name,vs,fs,material)
def ellipsoid(name,loc,scale,material):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;o.data.materials.append(material);return o
def hull(name,sections,material):
    # sculpted octagonal cross sections along the car, front at negative Y
    vs=[]
    for y,w,b,t in sections:
        e=min(.085,(t-b)*.25)
        vs.extend([(-w*.88,y,b),(-w,y,b+e),(-w,y,t-e),(-w*.83,y,t),(w*.83,y,t),(w,y,t-e),(w,y,b+e),(w*.88,y,b)])
    fs=[]
    for i in range(len(sections)-1):
        for j in range(8):fs.append((i*8+j,i*8+(j+1)%8,(i+1)*8+(j+1)%8,(i+1)*8+j))
    fs += [tuple(range(7,-1,-1)),tuple(range((len(sections)-1)*8,len(sections)*8))]
    return mesh(name,vs,fs,material)
def export(name, objects):
    deps=bpy.context.evaluated_depsgraph_get(); parts=[]
    for ob in objects:
        if ob.type!='MESH':continue
        ev=ob.evaluated_get(deps);me=ev.to_mesh();me.calc_loop_triangles();vs=[]
        for v in me.vertices:
            p=ob.matrix_world@v.co;vs.extend([round(p.x,5),round(p.z,5),round(-p.y,5)])
        tris=[]
        for t in me.loop_triangles:tris.extend(t.vertices)
        m=ob.data.materials[0] if ob.data.materials else dark
        parts.append({'name':ob.name.split('.')[0],'material':m.name,'color':list(m.diffuse_color),'vertices':vs,'triangles':tris})
        ev.to_mesh_clear()
    with open(OUT+'/'+name+'.json','w') as f:json.dump({'name':name,'parts':parts},f,separators=(',',':'))
def wheel(x,y,z,r,width):
    side=1 if x>0 else -1; prefix=('WheelFR' if y<0 else 'WheelRR') if x>0 else ('WheelFL' if y<0 else 'WheelRL')
    def cyl(name,radius,depth,xx,material,verts=40):
        bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=radius,depth=depth,location=(xx,y,z),rotation=(0,math.pi/2,0));o=bpy.context.object;o.name=prefix+'_'+name;o.data.materials.append(material);return o
    cyl('Tire',r,width,x,rubber); cyl('Rim',r*.77,width+.01,x,rim);cyl('Recess',r*.69,width+.016,x,dark)
    xx=x+side*(width/2+.016)
    cyl('Hub',r*.20,.045,xx,chrome)
    for j in range(10):
        a=j*math.tau/10
        tube(prefix+'_Spoke',[(xx,y+math.sin(a)*r*.17,z+math.cos(a)*r*.17),(xx,y+math.sin(a+.15)*r*.67,z+math.cos(a+.15)*r*.67)],.023,trim,6)
    for j in range(32):
        a=j*math.tau/32
        o=box(prefix+'_Tread',(x,y+math.sin(a)*r,z+math.cos(a)*r),(width*.92,.035,.018),dark,0);o.rotation_euler.x=-a
    for j in range(5):
        a=j*math.tau/5;cyl('Bolt'+str(j),.021,.05,xx+side*.025,chrome,8).location+=(Vector((0,math.sin(a)*r*.12,math.cos(a)*r*.12)))
def car(name,kind):
    before=set(bpy.data.objects)
    configs=[(1.03,2.38,.44,1.14,1.34),(1.0,2.24,.46,1.20,1.45),(.94,1.91,.47,1.20,1.22),(1.02,2.02,.59,1.39,1.22),(1.10,2.44,.45,1.08,1.52)]
    w,L,r,roof,axle=configs[kind]
    h=.86 if kind!=3 else 1.10
    hull('Paint_body',[(-L,w*.7,.29,h*.59),(-L*.82,w*.97,.29,h*.88),(-axle,w,.3,h),(-.3,w*.91,.31,h*.88),(axle,w,.32,h*1.05),(L*.92,w*.89,.35,h*.85)],paint)
    hull('Carbon_splitter',[(-L-.12,w*.84,.22,.28),(-L*.72,w*1.03,.22,.29),(L*.91,w,.23,.31)],dark)
    # cabin glazing and roof with strongly sloping windscreen
    hull('Glass_canopy',[(-.91,w*.62,h*.91,h*.93),(-.22,w*.68,h*.96,roof+.13),(.68,w*.61,h*.99,roof+.15),(1.18,w*.53,h*.94,h*1.02)],glass)
    hull('Paint_roof',[(-.19,w*.65,roof+.105,roof+.14),(.30,w*.66,roof+.13,roof+.17),(.69,w*.59,roof+.12,roof+.145)],paint)
    for s in [-1,1]:
        # sculpted fender caps; angular wheel arch rims
        for y in [-axle,axle]:
            # Wide sculpted fender shell above each wheel, with red arch edging.
            vs=[]
            for j in range(25):
                a=j*math.pi/24
                for xx,rr in [(w+.03,r*1.10),(w+.035,r*1.24),(w-.23,r*1.30)]:
                    vs.append((s*xx,y+math.cos(a)*rr,r+math.sin(a)*rr))
            fs=[]
            for j in range(24):
                for k in range(2):
                    face=(j*3+k,j*3+k+1,(j+1)*3+k+1,(j+1)*3+k)
                    fs.append(face if s>0 else tuple(reversed(face)))
            mesh('Paint_sculpted_fender',vs,fs,paint)
            pts=[(s*(w+.008),y+math.cos(a)*r*1.12,r+math.sin(a)*r*1.13) for a in [i*math.pi/20 for i in range(21)]]
            tube('Accent_fender',pts,.025,trim)
            pts2=[(s*(w-.045),p[1],p[2]+.026) for p in pts];tube('Paint_fender',pts2,.064,paint)
            wheel(s*w*.96,y,r,r,.29 if kind!=3 else .38)
        tube('Accent_sill',[(s*w,-axle+.12,.3),(s*(w+.025),.45,.28),(s*w,axle-.10,.36)],.036,trim)
        tube('Carbon_windowframe',[(s*w*.63,-.90,h*.94),(s*w*.68,-.22,roof+.15),(s*w*.61,.7,roof+.17),(s*w*.55,1.16,h)],.027,dark)
        tube('Accent_roofline',[(s*w*.68,-.19,roof+.16),(s*w*.61,.69,roof+.18),(s*w*.75,1.2,h+.01)],.018,trim)
        o=box('Paint_mirror',(s*w*.92,-.48,1.08),(.25,.26,.105),paint);o.rotation_euler.z=s*.2
        tube('Metal_mirror_stem',[(s*.68,-.43,.97),(s*w*.91,-.48,1.06)],.027,dark)
        # headlights with three light strips
        o=box('Carbon_headlamp',(s*w*.66,-L*.845,h*.78),(.5,.40,.05),dark);o.rotation_euler.z=s*-.38
        for j in range(3):
            tube('Headlight_strip',[(s*w*.40,-L*.92+j*.075,h*.81+j*.025),(s*w*.85,-L*.83+j*.075,h*.88+j*.025)],.019,light)
        o=box('Carbon_intake',(s*w*.68,-L*.97,.43),(.51,.055,.21),dark)
        for j in range(5):box('Metal_grille',(s*w*.68+(.05*j-.1),-L*.99,.43),(.012,.018,.16),chrome,0)
        tube('Accent_bumper',[(s*w*.98,-L*.81,.32),(s*w*.9,-L*.95,.54),(s*w*.29,-L-.02,.52)],.03,trim)
        tube('Accent_hood',[(s*.10,-L*.98,h*.64),(s*.31,-L*.66,h+.012),(s*.43,-.82,h*.97)],.022,trim)
        box('Carbon_sidevent',(s*w*.943,.43,.65),(.055,.77,.23),dark)
        for j in range(4):box('Accent_sidevent',(s*w*.974,.16+j*.16,.65),(.015,.028,.21),trim,.006)
        box('Metal_doorhandle',(s*w*.929,.23,.84),(.035,.19,.035),chrome,.01)
        box('Taillight',(s*w*.64,L*.921,h*.79),(.57,.025,.075),tail)
        tube('Metal_exhaust',[(s*.51,L*.79,.4),(s*.51,L*.99,.4)],.07,chrome,16)
        if kind in [0,1,4]:
            o=box('Carbon_wingstand',(s*.66,L*.71,1.15),(.07,.24,.62),dark);o.rotation_euler.x=-.22
            box('Accent_wingend',(s*(w+.04),L*.77,1.48),(.06,.55,.29),trim,.02)
    if kind in [0,1,4]:box('Carbon_rearwing',(0,L*.77,1.43),(w*2.14,.51,.085),dark,.045)
    # Central intake and splitter braces match the reference's low, open nose.
    box('Carbon_centerintake',(0,-L-.012,.40),(.90,.055,.22),dark,.025)
    for j in range(13):box('Metal_centergrille',(-.39+j*.065,-L-.045,.40),(.012,.015,.17),chrome,0)
    for s in [-1,1]:
        tube('Accent_splitterbrace',[(s*.43,-L-.045,.52),(s*.35,-L-.09,.25)],.026,trim)
    if kind==1:
        box('Paint_hoodscoop',(0,-1.16,.92),(.55,.65,.18),paint)
        for s in [-1,1]:box('Accent_racingstripe',(s*.2,-1.55,.899),(.15,.89,.015),trim,0)
    if kind==2:box('Paint_hatchback',(0,1.12,1.04),(1.43,.52,.35),paint)
    if kind==3:
        for s in [-1,1]:tube('Metal_rollcage',[(s*.74,-.7,1.1),(s*.74,-.17,1.75),(s*.74,.7,1.75),(s*.74,1.3,1.1)],.06,trim)
        box('Carbon_roofrack',(0,.35,1.79),(1.59,1.05,.07),dark)
    if kind==4:
        for s in [-1,1]:box('Accent_fin',(s*.86,1.4,1.11),(.06,.95,.39),trim,.025)
    obs=list(set(bpy.data.objects)-before);export(name,obs)
    collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
    for ob in obs:
        for c in list(ob.users_collection):c.objects.unlink(ob)
        collection.objects.link(ob)
        ob.location.x+=kind*6
    return obs
allcars=[]
for k,n in enumerate(['Vanta','Comet','Miso','Nomad','Spectre']):allcars+=car(n,k)
# reusable environment library, exported at origin
def asset(name,fn):
    before=set(bpy.data.objects);fn();obs=list(set(bpy.data.objects)-before);export(name,obs)
    for o in obs:o.location.y+=12
def tree():
    tube('Bark_trunk',[(0,0,0),(.2,0,2.6),(-.15,.1,4.8)],.27,bark,7)
    for i in range(8):
        a=i*math.tau/8;end=(math.cos(a)*1.6,math.sin(a)*1.6,4+random.random()*1.6)
        tube('Bark_branch',[(0,0,2.2),end],.10,bark,6)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=end);o=bpy.context.object;o.name='Leaves_crown';o.scale=(1.6,1.2,.85);o.data.materials.append(leaf)
asset('AlienTree',tree)
def crystals():
    for i in range(5):
        a=i*2.4;h=1.3+random.random()*2.2;x=math.sin(a)*.7;y=math.cos(a)*.7
        vs=[(x+math.cos(j*math.tau/5)*.36,y+math.sin(j*math.tau/5)*.36,z) for z in [0,h*.7] for j in range(5)]+[(x+.18,y-.12,h)]
        fs=[(j,(j+1)%5,(j+1)%5+5,j+5) for j in range(5)]+[(j+5,(j+1)%5+5,10) for j in range(5)];mesh('Crystal_shard',vs,fs,crystal)
asset('CrystalCluster',crystals)
def rocks():
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1);o=bpy.context.object;o.name='Rock_facets';o.scale=(2,1.6,2.5);o.data.materials.append(rock)
asset('Rock',rocks)
def arch():
    for s in [-1,1]:
        box('Building_pillar',(s*11,0,4),(1.3,2,8),building,.15)
        box('Crystal_pillarstrip',(s*11,-1.02,4),(.24,.1,6),crystal)
    box('Building_arch',(0,0,8),(23.5,2,1.1),building,.2)
    box('Crystal_header',(0,-1.02,8),(19,.1,.17),crystal)
asset('RaceArch',arch)
asset('RoadModule',lambda:box('Road_surface',(0,0,-.2),(22,12,.4),road,.05))
def station():
    box('Building_base',(0,0,1.5),(7,5,3),building,.25)
    box('Glass_window',(0,-2.51,1.9),(5.9,.07,1.25),glass)
    box('Accent_roof',(0,0,3.1),(7.6,5.5,.3),trim)
    tube('Metal_antenna',[(2,0,3.2),(2,0,6.6)],.075,chrome)
    ellipsoid('Crystal_beacon',(2,0,6.7),(.28,.28,.28),crystal)
asset('TrackStation',station)
# Editable source and car beauty render.
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/AetherGrounds.blend')
for o in bpy.context.scene.objects:
    o.hide_render=o not in allcars[:len([o for o in bpy.data.collections['Vanta'].objects])]
for c in bpy.data.collections:
    if c.name!='Vanta':
        for o in c.objects:o.hide_render=True
for o in bpy.data.collections['Vanta'].objects:o.hide_render=False
floor=box('StudioFloor',(0,0,-.09),(200,200,.1),mat('Studio',(.12,.16,.22)),0)
bpy.ops.object.camera_add(location=(6,-8,4));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,.7))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=53;bpy.context.scene.camera=cam
for loc,power,size in [((3,-5,7),2000,5),((-5,-2,4),1600,5),((2,5,5),2200,4)]:
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1400;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.world.color=(.25,.25,.25);scene.render.filepath=ROOT+'/Evidence/starting-car.png'
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/AetherGrounds.blend')
bpy.ops.render.render(write_still=True)
print('AETHER_ASSETS_COMPLETE')
