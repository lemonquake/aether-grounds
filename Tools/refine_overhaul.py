from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=root/'Assets/Scripts/Vehicle.cs';s=p.read_text(encoding='utf-8');s=s.replace('float launchGrace,rollGrace,splashTimer,aiLane;','float launchGrace,rollGrace,splashTimer,aiLane,righting;')
s=s.replace('if(active&&upright<.2f&&overturned>.10f){','if(active&&upright<.2f&&overturned>.10f)righting=.3f;if(active&&righting>0){righting-=Time.fixedDeltaTime;')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/Game.cs';s=p.read_text(encoding='utf-8')
for sig in ['void MenuGUI(){','void GarageGUI(){','void Upgrade(string']:
 start=s.index(sig);brace=s.index('{',start);depth=1;i=brace+1
 while depth:
  if s[i]=='{':depth+=1
  if s[i]=='}':depth-=1
  i+=1
 s=s[:start]+s[i:]
s=s.replace('int max,float x,float y,string[] names=null','int max,float x,float y,string[] names=null')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/TrackWorld.cs';s=p.read_text(encoding='utf-8');s=s.replace('string asset=i%4==0?"CrystalCluster":i%5==0?"Rock":"AlienTree";','string asset=index==1?"Rock":i%4==0?"CrystalCluster":i%5==0?"Rock":"AlienTree";').replace('string asset=i%10==0?"AlienTree":i%6==0?"CrystalCluster":"Rock";','string asset=index==1?"Rock":i%10==0?"AlienTree":i%6==0?"CrystalCluster":"Rock";')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Editor/BuildGame.cs';s=p.read_text(encoding='utf-8').replace('if(em)renderer.sharedMaterial=em;','if(em){EditorUtility.CopySerialized(material,em);renderer.sharedMaterial=em;}')
s=s.replace('File.WriteAllText("Evidence/build-result.txt",','foreach(var shader in shaders)if(shader&&ShaderUtil.ShaderHasError(shader))throw new System.Exception("Shader compilation failed: "+shader.name);\n  File.WriteAllText("Evidence/build-result.txt",')
p.write_text(s,encoding='utf-8')
for p in (root/'Assets/Scripts').glob('*.cs'):
 s=p.read_text(encoding='utf-8').replace('FindFirstObjectByType<','FindAnyObjectByType<').replace('(FindObjectsSortMode.None)','()');p.write_text(s,encoding='utf-8')
