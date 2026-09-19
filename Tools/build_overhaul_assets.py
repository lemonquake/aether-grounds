"""Seven completely new silhouettes, editable alongside the original three cars."""
from pathlib import Path
source = Path(__file__).with_name('build_assets.py').read_text()
exec(source.split('allcars=[]')[0])
for kind, name in [(0,'Vanta'),(2,'Miso'),(3,'Nomad')]:
    car(name,kind)

def make_car(name, kind):
    before=set(bpy.data.objects)
    w=1.03; axle=1.36; radius=.46; length=2.3
    if kind=='truck': w=1.08; axle=1.5; radius=.55; length=2.5
    if kind=='formula': w=1.08; axle=1.55; radius=.45; length=2.65
    if kind=='micro': w=.87; axle=1.12; radius=.43; length=1.75
    if kind=='hotrod': w=.98; axle=1.4; radius=.49
    if kind=='van': w=1; axle=1.35; radius=.49
    hull('Carbon_chassis',[(-length,.65,.23,.33),(-axle,w,.23,.35),(axle,w,.24,.38),(length,.8,.25,.38)],dark)
    if kind=='muscle':
        hull('Paint_body',[(-2.3,.93,.35,.84),(-1.9,1.04,.35,1.00),(-.65,1.02,.36,.98),(1.55,1.04,.36,1.0),(2.2,.97,.38,.9)],paint)
        hull('Glass_cabin',[(-.55,.76,.94,.98),(-.12,.70,.97,1.52),(.83,.70,.97,1.52),(1.33,.78,.96,1.02)],glass)
        box('Paint_roof',(.0,.35,1.54),(1.43,1.03,.09),paint)
        box('Carbon_grille',(0,-2.32,.65),(1.69,.045,.27),dark)
        for s in [-1,1]:
            for x in [.56,.82]: tube('Headlight_round',[(s*x,-2.34,.72),(s*x,-2.38,.72)],.105,light,24)
            box('Accent_stripe',(s*.23,-1.46,1.005),(.22,1.53,.012),trim,0)
        box('Paint_hood_scoop',(0,-1.14,1.07),(.52,.62,.18),paint)
        box('Carbon_scoop_mouth',(0,-1.46,1.08),(.42,.018,.10),dark)
    elif kind=='formula':
        hull('Paint_monocoque',[(-2.6,.19,.32,.5),(-1.0,.38,.34,.82),(.1,.55,.35,.96),(1.9,.48,.37,.9),(2.4,.24,.4,.62)],paint)
        ellipsoid('Carbon_cockpit',(0,.15,1.01),(.38,.66,.12),dark)
        ellipsoid('Glass_windscreen',(0,-.3,1.06),(.35,.2,.22),glass)
        box('Carbon_frontwing',(0,-2.36,.37),(2.35,.43,.08),dark)
        box('Accent_rearwing',(0,2.09,1.36),(2.15,.48,.12),trim)
        for s in [-1,1]:
            hull('Paint_sidepod',[(-.7,.15,.38,.60),(.9,.15,.38,.75),(1.4,.15,.38,.64)],paint).location.x=s*.77
            tube('Metal_suspension',[(0,-1.35,.55),(s*1.07,-1.55,.46)],.045,chrome)
            tube('Metal_suspension',[(s*.4,1,.6),(s*1.07,1.55,.46)],.045,chrome)
            box('Carbon_wingmount',(s*.5,2.05,1.01),(.06,.15,.65),dark)
            box('Accent_wing_end',(s*1.08,2.09,1.36),(.055,.5,.28),trim)
        tube('Accent_halo',[(-.38,.6,1.07),(-.38,-.3,1.25),(0,-.58,1.25),(.38,-.3,1.25),(.38,.6,1.07)],.04,trim)
    elif kind=='truck':
        box('Paint_body',(0,0,.76),(2.12,4.9,.65),paint,.14)
        box('Paint_cab',(0,-.58,1.22),(1.88,2.25,.8),paint,.15)
        box('Glass_windscreen',(0,-1.72,1.41),(1.61,.035,.54),glass)
        box('Carbon_bed',(0,1.64,1.12),(1.73,1.38,.06),dark)
        box('Carbon_bumper',(0,-2.56,.62),(2.28,.25,.28),dark)
        for s in [-1,1]:
            box('Glass_side',(s*.95,-.53,1.41),(.02,1.75,.52),glass)
            box('Paint_bedrail',(s*.99,1.6,1.23),(.13,1.72,.4),paint)
            box('Headlight_square',(s*.76,-2.46,.95),(.46,.035,.2),light)
        for j in range(6):box('Metal_grille',(-.5+j*.2,-2.48,.87),(.05,.02,.34),chrome)
    elif kind=='micro':
        hull('Paint_body',[(-1.75,.62,.35,.64),(-1.2,.9,.32,1.05),(.9,.90,.32,1.06),(1.72,.72,.36,.82)],paint)
        ellipsoid('Glass_bubble',(0,.0,1.05),(.76,1.12,.66),glass)
        box('Paint_roof',(0,.12,1.67),(1.09,.88,.09),paint,.1)
        for s in [-1,1]:
            tube('Headlight_round',[(s*.53,-1.60,.88),(s*.53,-1.68,.88)],.16,light,24)
            tube('Accent_belt',[(s*.86,-.9,.89),(s*.9,.85,.89)],.034,trim)
        box('Carbon_grille',(0,-1.73,.53),(.66,.04,.17),dark)
    elif kind=='hotrod':
        hull('Paint_body',[(-2.25,.39,.36,.94),(-.45,.44,.35,1.04),(.3,.78,.33,.95),(1.97,.8,.35,.81)],paint)
        box('Carbon_seats',(0,.5,1.02),(1.27,.72,.17),dark)
        box('Glass_windscreen',(0,-.14,1.30),(1.27,.055,.43),glass)
        box('Metal_grille',(0,-2.27,.7),(.72,.04,.64),chrome)
        for j in range(9):box('Carbon_grilleslit',(-.3+j*.075,-2.3,.71),(.027,.025,.54),dark,0)
        for s in [-1,1]:
            tube('Headlight_round',[(s*.67,-1.91,.91),(s*.67,-2.02,.91)],.16,light,24)
            tube('Metal_exhaust',[(s*.51,-1.4,.73),(s*.79,-.6,.51),(s*.8,.99,.5)],.075,chrome,16)
            tube('Accent_rollbar',[(s*.65,.98,.83),(s*.65,.98,1.48),(0,.98,1.53)],.055,trim)
    elif kind=='van':
        box('Paint_body',(0,0,.88),(1.97,4.24,1.1),paint,.18)
        box('Paint_roof',(0,.05,1.73),(1.81,3.95,.27),paint,.18)
        box('Glass_windscreen',(0,-2.05,1.4),(1.65,.04,.58),glass)
        for s in [-1,1]:
            for y in [-1.28,-.1,1.1]:box('Glass_side',(s*.987,y,1.4),(.023,.93,.51),glass)
            box('Headlight',(s*.62,-2.15,.88),(.45,.04,.17),light)
            box('Accent_belt',(s*1,-.02,1.04),(.02,3.98,.065),trim)
        box('Carbon_bumper',(0,-2.18,.47),(2.01,.17,.18),dark)
    elif kind=='prototype':
        hull('Paint_spine',[(-2.45,.45,.29,.53),(-1.4,.68,.29,.66),(.2,.68,.29,.73),(2.35,.52,.31,.55)],paint)
        ellipsoid('Glass_single_canopy',(0,.0,.83),(.44,1.04,.47),glass)
        for s in [-1,1]:
            h=hull('Paint_pontoon',[(-2.45,.25,.32,.61),(-1.3,.30,.32,1.0),(.9,.30,.31,.91),(2.4,.24,.32,.6)],paint);h.location.x=s*.83
            tube('Headlight_blade',[(s*.81,-2.45,.64),(s*.81,-1.95,.81)],.035,light)
            box('Accent_fin',(s*.87,1.69,1.01),(.06,1.03,.49),trim)
        box('Carbon_diffuser',(0,2.34,.40),(1.82,.16,.21),dark)
        for j in range(7):box('Metal_diffuserfin',(-.7+j*.233,2.34,.38),(.028,.32,.29),chrome,0)
    for s in [-1,1]:
        for y in [-axle,axle]:wheel(s*w,y,radius,radius,.32)
        box('Taillight',(s*.66,length-.02,.75),(.39,.045,.09),tail)
        box('Metal_mirror',(s*w,-.52,1.11),(.22,.19,.1),chrome)
    obs=list(set(bpy.data.objects)-before);export(name,obs)
    collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
    for ob in obs:
        for c in list(ob.users_collection):c.objects.unlink(ob)
        collection.objects.link(ob)
        ob.location.x+=len(bpy.data.collections)*6

for n,k in [('Comet','muscle'),('Spectre','formula'),('Atlas','truck'),('Pip','micro'),('Cinder','hotrod'),('Relay','van'),('Manta','prototype')]:make_car(n,k)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/OverhaulCars.blend')
print('AETHER_OVERHAUL_ASSETS_COMPLETE')
