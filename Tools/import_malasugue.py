"""Build a reproducible local-metre map from the retained OSM extract (ODbL).
Google Maps was used for visual reference; no Google imagery is shipped.
The circuit follows real connected streets. Facade appearance is reconstructed.
"""
import xml.etree.ElementTree as ET
import math, json, heapq
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
r=ET.parse(ROOT/'ArtSource/References/Malasugue/neighborhood.osm').getroot()
LAT,LON=7.08915,125.6262
def xy(lat,lon): return [round((lon-LON)*111320*math.cos(math.radians(LAT)),3),3.1,round((lat-LAT)*110574,3)]
nodes={n.get('id'):xy(float(n.get('lat')),float(n.get('lon'))) for n in r.findall('node')}
def tags(e): return {t.get('k'):t.get('v') for t in e.findall('tag')}
ways=[(w,tags(w),[n.get('ref') for n in w.findall('nd')]) for w in r.findall('way')]
def dist(a,b): return math.hypot(a[0]-b[0],a[2]-b[2])
graph={}; roads=[]; buildings=[]; landmarks=[]
for w,t,ids in ways:
 p=[nodes[i] for i in ids]
 if 'highway' in t and t['highway'] not in ['footway','steps','path','construction','cycleway','proposed']:
  name=t.get('name',t.get('alt_name','Neighborhood lane'))
  width=float(t.get('width','0').split()[0]) if t.get('width','0').split()[0].replace('.','').isdigit() else 0
  if not width: width={'primary':14,'secondary':12,'tertiary':8,'residential':6,'service':3.5}.get(t['highway'],6)
  roads.append(dict(name=name,points=[dict(x=x,y=y,z=z) for x,y,z in p],width=width,asphalt=t.get('surface')=='asphalt'))
  for a,b in zip(ids,ids[1:]):
   d=dist(nodes[a],nodes[b]);graph.setdefault(a,[]).append((b,d,name));graph.setdefault(b,[]).append((a,d,name))
 if 'building' in t and ids[0]==ids[-1] and len(ids)>3:
  p=p[:-1];cx=sum(v[0] for v in p)/len(p);cz=sum(v[2] for v in p)/len(p)
  if not (-440<cx<445 and -510<cz<510): continue
  try: height=float(t.get('height',float(t.get('building:levels',1))*3.1))
  except ValueError: height=4
  buildings.append(dict(id=w.get('id'),name=t.get('name',''),points=[dict(x=x,y=y,z=z) for x,y,z in p],height=max(3,min(height,28)),surveyHeight='height' in t or 'building:levels' in t))
  if t.get('name'): landmarks.append(dict(name=t['name'],position=dict(x=cx,y=3.1,z=cz)))
for n in r.findall('node'):
 t=tags(n)
 if t.get('name') and ('amenity' in t or 'shop' in t or 'tourism' in t):
  x,y,z=nodes[n.get('id')]
  if -380<x<380 and -430<z<470: landmarks.append(dict(name=t['name'],position=dict(x=x,y=y,z=z)))
def nearest(lat,lon):
 p=xy(lat,lon);return min(graph,key=lambda n:dist(nodes[n],p))
def path(a,b,avoid=set()):
 queue=[(0,a)];cost={a:0};prev={}
 while queue:
  c,n=heapq.heappop(queue)
  if n==b: break
  if c>cost[n]:continue
  for v,d,name in graph[n]:
   c2=c+d*(100 if frozenset([n,v]) in avoid else 1)
   if c2<cost.get(v,1e20):cost[v]=c2;prev[v]=n;heapq.heappush(queue,(c2,v))
 result=[b]
 while result[-1]!=a:result.append(prev[result[-1]])
 return result[::-1]
# Malasugue/Maya-Maya -> Bolcan -> Cabaguio/Del Pilar -> Holy Cross/Tulingan -> Malasugue.
start='260129359'; end='1226821355'
out=path(start,'259745543')[:-1]+path('259745543','1226821404')[:-1]+path('1226821404',end)
avoid={frozenset([a,b]) for a,b in zip(out,out[1:])}
back=path(end,'260125388',avoid)[:-1]+path('260125388','260129295',avoid)[:-1]+path('260129295','308727404',avoid)[:-1]+path('308727404',start,avoid)
route=out[:-1]+back[:-1]
assert len(route)==len(set(route)), 'Circuit intersects itself at an OSM node'
# Uniform distance resampling preserves the surveyed corners without Catmull-Rom overshoot.
poly=[nodes[i] for i in route];edges=[dist(a,b) for a,b in zip(poly,poly[1:]+poly[:1])];length=sum(edges)
samples=[];j=0;acc=0
for k in range(480):
 d=k*length/480
 while acc+edges[j]<d:acc+=edges[j];j+=1
 t=(d-acc)/edges[j];a=poly[j];b=poly[(j+1)%len(poly)];samples.append(dict(x=a[0]+(b[0]-a[0])*t,y=3.16,z=a[2]+(b[2]-a[2])*t))
data=dict(originLatitude=LAT,originLongitude=LON,roads=roads,buildings=buildings,landmarks=landmarks,route=samples,residence=dict(zip(['x','y','z'],xy(7.0922515226973175,125.62697382377056))),destination=dict(zip(['x','y','z'],xy(7.086026624224303,125.62414461720785))))
(ROOT/'Assets/Resources/Malasugue.json').write_text(json.dumps(data,separators=(',',':')),encoding='utf-8')
im=Image.new('RGB',(1300,1400),'#e5e2d4');draw=ImageDraw.Draw(im)
def px(p):return (int(650+p['x']*1.15),int(710-p['z']*1.15))
for b in buildings:draw.polygon([px(p) for p in b['points']],fill='#a9a59a',outline='#817e77')
for road in roads:draw.line([px(p) for p in road['points']],fill='#f9f8f3',width=max(2,int(road['width']*1.15)))
draw.line([px(p) for p in samples]+[px(samples[0])],fill='#187eac',width=4)
for key,col in [('residence','red'),('destination','blue')]:
 x,y=px(data[key]);draw.ellipse((x-6,y-6,x+6,y+6),fill=col);draw.text((x+10,y),key,fill='black')
im.save(ROOT/'ArtSource/References/Malasugue/geometry-plan.png')
print('Circuit metres',round(length),'route nodes',len(route),'footprints',len(buildings),'roads',len(roads),'landmarks',len(landmarks))
print('Circuit road names',sorted(set(name for a,b in zip(route,route[1:]+route[:1]) for v,d,name in graph[a] if v==b)))
