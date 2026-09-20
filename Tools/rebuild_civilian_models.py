"""Preserve Kenney CC0 bodies, replace wheel groups, normalize noses to Unity +Z."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
reports=[]
for name in ('sedan','taxi','hatchback-sports','van','truck'):
    source=ROOT/'ArtSource/ThirdParty/cars/Models/OBJ format'/(name+'.obj')
    mats={};current=''
    for line in source.with_suffix('.mtl').read_text().splitlines():
        a=line.split()
        if not a:continue
        if a[0]=='newmtl':current=a[1];mats[current]={'color':[.6,.6,.6]}
        elif a[0]=='Kd':mats[current]['color']=list(map(float,a[1:4]))
        elif a[0]=='map_Kd':
            texture=source.parent/' '.join(a[1:])
            if not texture.exists():texture=next((ROOT/'ArtSource/ThirdParty/cars').rglob(texture.name))
            mats[current]['image']=Image.open(texture).convert('RGB')
    vertices=[];uvs=[];groups={};wheelverts={};current=next(iter(mats));group='body'
    for line in source.read_text().splitlines():
        a=line.split()
        if not a:continue
        if a[0]=='v':vertices.append(list(map(float,a[1:4])))
        elif a[0]=='vt':uvs.append(list(map(float,a[1:3])))
        elif a[0] in ('g','o'):group=a[1]
        elif a[0]=='usemtl':current=a[1]
        elif a[0]=='f':
            face=[[int(n) if n else 0 for n in token.split('/')] for token in a[1:]]
            # Reflect Z from OBJ, then rotate the entire model 180 degrees.
            # Net coordinates are (-x,y,z), with reversed triangle winding.
            if group.startswith('wheel-'):
                wheelverts.setdefault(group,[]).extend([[-vertices[v[0]-1][0],vertices[v[0]-1][1],vertices[v[0]-1][2]] for v in face])
                continue
            mat=mats[current];color=mat['color']
            if 'image' in mat and len(face[0])>1:
                image=mat['image'];uv=uvs[face[0][1]-1]
                color=[v/255 for v in image.getpixel((min(image.width-1,max(0,int(uv[0]*image.width))),min(image.height-1,max(0,int((1-uv[1])*image.height)))))]
            key=tuple(round(c,3) for c in color)
            if key not in groups:groups[key]=dict(name='BodySurface'+str(len(groups)),material='Kenney_'+''.join('%02x'%round(c*255) for c in key),color=list(key),vertices=[],triangles=[])
            p=groups[key]
            for j in range(1,len(face)-1):
                for k in (0,j+1,j):
                    v=vertices[face[k][0]-1];p['triangles'].append(len(p['vertices'])//3);p['vertices'].extend([-v[0],v[1],v[2]])
    anchors=[]
    for group,points in wheelverts.items():
        lo=[min(p[k] for p in points) for k in range(3)];hi=[max(p[k] for p in points) for k in range(3)]
        center=[(a+b)/2 for a,b in zip(lo,hi)]
        label=('WheelF' if 'front' in group else 'WheelR')+('L' if center[0]<0 else 'R')
        anchors.append(dict(name=label,position=center,radius=(hi[1]-lo[1])*.5))
    assert len(anchors)==4,(name,len(anchors))
    output=ROOT/'Assets/Resources/Models'/('K_'+name+'.json')
    output.write_text(json.dumps(dict(name='K_'+name,parts=list(groups.values()),wheelAnchors=anchors),separators=(',',':')))
    reports.append(dict(model=name,source=str(source.relative_to(ROOT)),source_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),output_sha256=hashlib.sha256(output.read_bytes()).hexdigest(),front='+Z',wheels=anchors,license='CC0-1.0 (Kenney body); original Blender wheel replacement'))
(ROOT/'Evidence/VehiclePhysics/civilian-models.json').write_text(json.dumps(reports,indent=2))
print('CIVILIAN_MODELS_COMPLETE '+str(len(reports)))
