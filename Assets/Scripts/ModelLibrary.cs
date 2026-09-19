using System;
using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 [Serializable] public class ModelPart { public string name,material; public float[] color,vertices; public int[] triangles; }
 [Serializable] public class ModelData {public string name;public ModelPart[] parts;}
 public static class ModelLibrary {
  static Dictionary<string,ModelData> data=new Dictionary<string,ModelData>();
  static Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
  static Dictionary<string,Material> mats=new Dictionary<string,Material>();
  public static Material Material(string key,Color color,float glow=0,float outline=.012f){
   if(mats.TryGetValue(key,out var m))return m;
   m=new Material(Shader.Find("Aether/Toon")){name=key,color=color};m.SetFloat("_Metallic",key.Contains("Paint")?.55f:key.Contains("Metal")||key.Contains("Wheel")?.8f:key.Contains("Glass")?.35f:0);m.SetFloat("_Smoothness",key.Contains("Glass")?.93f:key.Contains("Paint")?.78f:key.Contains("Metal")?.7f:.35f);m.SetFloat("_Emission",glow);m.SetFloat("_Outline",outline);m.enableInstancing=true;mats[key]=m;return m;
  }
  public static GameObject Create(string name,Transform parent=null){
   if(!data.TryGetValue(name,out var d)){d=JsonUtility.FromJson<ModelData>(Resources.Load<TextAsset>("Models/"+name).text);data[name]=d;}
   var root=new GameObject(name);if(parent)root.transform.SetParent(parent,false);
   var groups=new Dictionary<string,List<CombineInstance>>();var colors=new Dictionary<string,Color>();
   foreach(var p in d.parts){
    string pn=p.name.ToLowerInvariant();string accessory=pn.Contains("rearwing")||pn.Contains("wingstand")||pn.Contains("wingend")||pn.Contains("wingmount")||pn.Contains("wing_end")?"StockWing":pn.Contains("exhaust")?"StockExhaust":pn.Contains("hoodscoop")||pn.Contains("hood_scoop")||pn.Contains("scoop_mouth")?"StockScoop":"Body";
    string group=(p.name.StartsWith("WheelF")||p.name.StartsWith("WheelR")?p.name.Substring(0,7)+"/":accessory+"/")+p.material;
    if(!groups.ContainsKey(group)){groups[group]=new List<CombineInstance>();colors[group]=new Color(p.color[0],p.color[1],p.color[2]);}
    string mk=name+"/"+p.name+"/"+Array.IndexOf(d.parts,p);
    if(!meshes.TryGetValue(mk,out Mesh m)){
     m=new Mesh(){name=mk};var v=new Vector3[p.vertices.Length/3];for(int i=0;i<v.Length;i++)v[i]=new Vector3(p.vertices[i*3],p.vertices[i*3+1],p.vertices[i*3+2]);m.vertices=v;m.triangles=p.triangles;m.RecalculateNormals();m.RecalculateBounds();meshes[mk]=m;
    }
    groups[group].Add(new CombineInstance(){mesh=m,transform=Matrix4x4.identity});
   }
   var wheelRoots=new Dictionary<string,Transform>();var partRoots=new Dictionary<string,Transform>();
   foreach(var kv in groups){
    string cache=name+"/combined/"+kv.Key;
    if(!meshes.TryGetValue(cache,out Mesh combined)){combined=new Mesh(){name=cache,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};combined.CombineMeshes(kv.Value.ToArray(),true,true);meshes[cache]=combined;}
    string materialName=kv.Key.Split('/')[1];Transform par=root.transform;
    if(kv.Key.StartsWith("Wheel")){
     string wn=kv.Key.Split('/')[0];if(!wheelRoots.TryGetValue(wn,out par)){par=new GameObject(wn).transform;par.SetParent(root.transform,false);wheelRoots[wn]=par;}
    }
    if(kv.Key.StartsWith("Stock")){string pn=kv.Key.Split('/')[0];if(!partRoots.TryGetValue(pn,out par)){par=new GameObject(pn).transform;par.SetParent(root.transform,false);partRoots[pn]=par;}}
    var o=new GameObject(materialName);o.transform.SetParent(par,false);o.AddComponent<MeshFilter>().sharedMesh=combined;
    var mr=o.AddComponent<MeshRenderer>();float glow=materialName=="Headlight"?.65f:materialName=="Crystal"?.18f:materialName=="Taillight"?.4f:0;
    mr.sharedMaterial=Material(materialName,colors[kv.Key],glow);
   }
   foreach(var kv in wheelRoots){var bounds=new Bounds();bool first=true;foreach(var r in kv.Value.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}Vector3 center=root.transform.InverseTransformPoint(bounds.center);kv.Value.localPosition=center;foreach(Transform child in kv.Value)child.localPosition=-center;}
   return root;
  }
  public static void Customize(GameObject model,Color paint,Color accent,int wheels,int windows){
   foreach(var r in model.GetComponentsInChildren<MeshRenderer>()){
    Color? color=r.name=="Paint"?paint:r.name=="Accent"?accent:r.name=="Wheel"?Game.Paints[Mathf.Clamp(wheels,0,Game.Paints.Length-1)]:r.name=="Glass"?CarParts.Glass[Mathf.Clamp(windows,0,12)]:(Color?)null;
    if(color.HasValue){var m=Material(r.name+" "+color.Value,color.Value);r.sharedMaterial=m;}
   }
  }
 }
 [Serializable] public class CarSpec {
  public string name,description;public float speed,accel,brake,grip,mass;
  public CarSpec(string n,string d,float s,float a,float b,float g,float m){name=n;description=d;speed=s;accel=a;brake=b;grip=g;mass=m;}
  public static CarSpec[] All={new CarSpec("Vanta","Wide-body supercar • balanced grip",72,13,23,9.3f,1220),new CarSpec("Comet","Classic muscle coupe • powerful launch",68,15,21,8.4f,1410),new CarSpec("Miso","Compact hatch • agile cornering",62,14,25,10.5f,970),new CarSpec("Nomad","Rally buggy • forgiving over bumps",60,13.5f,24,9.7f,1160),new CarSpec("Spectre","Open-wheel Formula racer • precise steering",79,13.2f,26,10.2f,980),new CarSpec("Atlas","Wide rally pickup • heavy and stable",62,13.5f,25,9.0f,1650),new CarSpec("Pip","Bubble compact • light and nimble",61,15,26,11,820),new CarSpec("Cinder","Open hot rod • explosive acceleration",71,16,22,8.3f,1130),new CarSpec("Relay","Retro van • relaxed and sturdy",60,12.8f,25,9.8f,1480),new CarSpec("Manta","Enclosed prototype • smooth high-speed grip",77,12.7f,24,10.1f,1160)};
 }
}
