from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=root/'Assets/Scripts/CarParts.cs';s=p.read_text(encoding='utf-8')
s=s.replace('c.Validate();ModelLibrary.Customize','c.Validate();foreach(string part in new[]{"StockWing","StockExhaust","StockScoop"}){var old=model.transform.Find(part);if(old)old.gameObject.SetActive(part=="StockWing"?c.spoiler==0:part=="StockExhaust"?c.exhaust==0:c.engine==0);}\n   ModelLibrary.Customize')
old='root.localPosition=new Vector3(0,body==3?1.1f:body==8?1.85f:body==4?.9f:body==5?1.17f:1.02f,body==4?-1.3f:body==8?-.5f:.95f);'
new='root.localPosition=new[]{new Vector3(0,.95f,1.32f),new Vector3(0,1.05f,1.35f),new Vector3(0,.95f,1.26f),new Vector3(0,1.15f,1.28f),new Vector3(0,.96f,-1.42f),new Vector3(0,1.1f,2.02f),new Vector3(0,.9f,1.38f),new Vector3(0,1.07f,1.25f),new Vector3(0,1.87f,-.9f),new Vector3(0,.8f,-1.55f)}[body];'
assert old in s;s=s.replace(old,new);p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/ModelLibrary.cs';s=p.read_text(encoding='utf-8')
s=s.replace('string group=(p.name.StartsWith','string pn=p.name.ToLowerInvariant();string accessory=pn.Contains("rearwing")||pn.Contains("wingstand")||pn.Contains("wingend")||pn.Contains("wingmount")||pn.Contains("wing_end")?"StockWing":pn.Contains("exhaust")?"StockExhaust":pn.Contains("hoodscoop")||pn.Contains("hood_scoop")||pn.Contains("scoop_mouth")?"StockScoop":"Body";\n    string group=(p.name.StartsWith')
s=s.replace(':"Body/")+p.material',':accessory+"/")+p.material')
s=s.replace('var wheelRoots=new Dictionary<string,Transform>();','var wheelRoots=new Dictionary<string,Transform>();var partRoots=new Dictionary<string,Transform>();')
s=s.replace('var o=new GameObject(materialName);','if(kv.Key.StartsWith("Stock")){string pn=kv.Key.Split(\'/\')[0];if(!partRoots.TryGetValue(pn,out par)){par=new GameObject(pn).transform;par.SetParent(root.transform,false);partRoots[pn]=par;}}\n    var o=new GameObject(materialName);')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/TrackWorld.cs';s=p.read_text(encoding='utf-8')
s=s.replace('ModelLibrary.Material(i%4<2?"railA":"railB",i%4<2?new Color(.19f,.29f,.39f):new Color(.62f,.26f,.42f))','ModelLibrary.Material((i%4<2?"railA":"railB")+map,i%4<2?new Color(.15f,.22f,.28f):glow*.65f)')
s=s.replace('ModelLibrary.Material(i%2==0?"curbA":"curbB",i%2==0?new Color(.82f,.82f,.86f):new Color(.67f,.22f,.42f),0,0)','ModelLibrary.Material((i%2==0?"curbA":"curbB")+map,i%2==0?new Color(.82f,.82f,.86f):glow*.65f,0,0)')
p.write_text(s,encoding='utf-8')
