"""Import selected CC0 Kenney OBJ geometry and bake its palette into our model format.
Original OBJ/MTL/texture archives are retained under ArtSource/ThirdParty.
No downloaded executable or importer is run. Requires Pillow.
"""
from pathlib import Path
from PIL import Image
import json, hashlib

ROOT = Path(__file__).resolve().parents[1]
selection = {
 'suburban': ['building-type-a','building-type-c','fence','fence-low','planter','tree-small'],
 'commercial': ['building-'+c for c in 'abcdefghijklmn'] + ['building-skyscraper-'+c for c in 'abcde'],
 'nature': ['tree_palmDetailedTall','tree_palmDetailedShort','tree_palmBend','tree_oak','rock_largeA','rock_largeC','rock_smallA','rock_smallC'],
 'food': ['apple','banana','orange','watermelon','pineapple','coconut','corn','fish','skewer','pot','soda-bottle','barrel','bag','plate','bowl','bread'],
 'cars': ['sedan','hatchback-sports','van','truck','taxi'],
}
out = ROOT/'Assets/Resources/Models'
manifest=[]
for pack, names in selection.items():
 for name in names:
  path=ROOT/'ArtSource/ThirdParty'/pack/'Models/OBJ format'/(name+'.obj')
  if not path.exists():
   print('SKIP',path); continue
  mats={}; current='default'
  for line in path.with_suffix('.mtl').read_text().splitlines():
   a=line.split()
   if not a: continue
   if a[0]=='newmtl': current=a[1]; mats[current]={'color':[.6,.6,.6]}
   elif a[0]=='Kd': mats[current]['color']=list(map(float,a[1:4]))
   elif a[0]=='map_Kd':
    tex=path.parent/' '.join(a[1:])
    if not tex.exists(): tex=next((ROOT/'ArtSource/ThirdParty'/pack).rglob(tex.name))
    mats[current]['image']=Image.open(tex).convert('RGB')
  verts=[]; uvs=[]; groups={}; current=next(iter(mats))
  for line in path.read_text().splitlines():
   a=line.split()
   if not a: continue
   if a[0]=='v': verts.append(list(map(float,a[1:4])))
   elif a[0]=='vt': uvs.append(list(map(float,a[1:3])))
   elif a[0]=='usemtl': current=a[1]
   elif a[0]=='f':
    face=[list(map(lambda s:int(s) if s else 0, item.split('/'))) for item in a[1:]]
    mat=mats[current]; color=mat['color']
    if 'image' in mat and len(face[0])>1:
     im=mat['image']; uv=uvs[face[0][1]-1]
     color=[v/255 for v in im.getpixel((min(im.width-1,max(0,int(uv[0]*im.width))),min(im.height-1,max(0,int((1-uv[1])*im.height)))))]
    key=tuple(round(c,3) for c in color)
    if key not in groups: groups[key]={'name':'Surface'+str(len(groups)), 'material':'Kenney_'+''.join('%02x'%round(c*255) for c in key),'color':list(key),'vertices':[],'triangles':[]}
    g=groups[key]
    for j in range(1,len(face)-1):
     # OBJ right-handed to Unity left-handed: reflect Z and reverse winding.
     for idx in (0,j+1,j):
      v=verts[face[idx][0]-1]; g['triangles'].append(len(g['vertices'])//3);g['vertices'].extend([v[0],v[1],-v[2]])
  alias='K_'+name
  (out/(alias+'.json')).write_text(json.dumps({'name':alias,'parts':list(groups.values())},separators=(',',':')))
  manifest.append({'model':alias,'pack':pack,'source':str(path.relative_to(ROOT)),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'triangles':sum(len(g['triangles'])//3 for g in groups.values()),'bounds':[[min(v[k] for v in verts) for k in range(3)],[max(v[k] for v in verts) for k in range(3)]]})
  print(alias,manifest[-1]['triangles'])
(ROOT/'ArtSource/ThirdParty/import-manifest.json').write_text(json.dumps(manifest,indent=2))
