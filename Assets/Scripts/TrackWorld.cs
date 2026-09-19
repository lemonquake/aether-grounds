using System;
using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class TrackWorld : MonoBehaviour {
  public static readonly string[] Names={"Downtown Minda","Malasugue Town","Midnight Gardens","Airah Mountains"};
  public static readonly string[] Descriptions={"Philippine city streets, a raised flyover, busy markets and a damaged beach road.","Davao neighborhood circuit: Maya-Maya, Bolcan, Del Pilar and Malasugue. Mapped streets and building footprints.","Purple forest, luminous gardens and sweeping night turns.","A vast sunny mountain circuit with forested slopes, trestle bridges, summit overlooks and civilian traffic."};
  public static readonly Vector3[][] Shapes={
    new[]{new Vector3(-170,0,-250),new Vector3(120,0,-260),new Vector3(270,0,-140),new Vector3(300,0,60),new Vector3(160,0,220),new Vector3(-40,0,270),new Vector3(-255,0,175),new Vector3(-280,0,-70)},
    new[]{new Vector3(-200,0,-230),new Vector3(60,0,-300),new Vector3(280,0,-170),new Vector3(210,0,15),new Vector3(340,0,220),new Vector3(45,0,320),new Vector3(-260,0,205),new Vector3(-160,0,35),new Vector3(-330,0,-75)},
    new[]{new Vector3(-210,0,-220),new Vector3(100,0,-240),new Vector3(290,0,-80),new Vector3(175,0,100),new Vector3(220,0,290),new Vector3(-45,0,230),new Vector3(-280,0,170),new Vector3(-320,0,-50)}
   };
  public const int Count=480;public const float Width=24;
  public Vector3[] points=new Vector3[Count];public float length;public int map;
  public List<Vector3> pickups=new List<Vector3>();
  Transform decor;System.Random rng;Color ground,stone,foliage,glow;
  public void Generate(int index){
   if(index==AirahMap){GenerateAirah();return;}
   if(index==AirahMap){GenerateAirah();return;}
   if(index==1){map=1;PhilippineCity.BuildMalasugue(this);return;}
   if(index==0){map=0;PhilippineCity.Build(this);return;}
   map=index;rng=new System.Random(720+index);decor=new GameObject("Landscape").transform;decor.SetParent(transform);
   ground=new[]{new Color(.27f,.36f,.40f),new Color(.63f,.30f,.24f),new Color(.19f,.23f,.34f)}[index];
   stone=new[]{new Color(.35f,.30f,.53f),new Color(.70f,.37f,.28f),new Color(.32f,.25f,.48f)}[index];
   foliage=new[]{new Color(.16f,.61f,.56f),new Color(.36f,.53f,.48f),new Color(.58f,.28f,.65f)}[index];
   glow=new[]{new Color(.20f,.87f,.9f),new Color(.97f,.65f,.35f),new Color(.39f,.91f,.73f)}[index];
   var cp=Shapes[index];
   for(int i=0;i<Count;i++){float t=i/(float)Count*cp.Length;int n=(int)t;float f=t-n;Vector3 a=cp[(n+cp.Length-1)%cp.Length],b=cp[n],c=cp[(n+1)%cp.Length],d=cp[(n+2)%cp.Length];points[i]=.5f*((2*b)+(-a+c)*f+(2*a-5*b+4*c-d)*f*f+(-a+3*b-3*c+d)*f*f*f);points[i].y=5+Mathf.Sin(i/(float)Count*Mathf.PI*4)*3.8f+(index==1?Mathf.Sin(i/(float)Count*Mathf.PI*6)*4:0);}
   for(int i=0;i<Count;i++)length+=Vector3.Distance(points[i],points[(i+1)%Count]);
   MakeRoad();MakeTerrain();MakeFeatures();
   var water=Cube("Water",new Vector3(0,-5,0),new Vector3(2500,1,2500),WaterMaterial(),false);water.transform.SetParent(transform,true);
   for(int i=0;i<Count;i++){
    if(i%2==0){for(int s=-1;s<=1;s+=2){Vector3 p=points[i]+Right(i)*s*(Width/2+1);var post=Cube("Reflector",p+Vector3.up*.45f,new Vector3(.22f,.85f,.22f),ModelLibrary.Material("post",new Color(.08f,.13f,.21f)),false);post.transform.SetParent(decor,true);var cap=Cube("Track light",p+Vector3.up*.92f,new Vector3(.27f,.14f,.27f),ModelLibrary.Material("edgeglow"+index,glow,.5f),false);cap.transform.SetParent(decor,true);}}
    if(i%42==14){for(int s=-1;s<=1;s++){Vector3 p=points[i]+Right(i)*s*6+Vector3.up*1.25f;pickups.Add(p);}}
    if(i%80==0){var arch=ModelLibrary.Create("RaceArch",decor);arch.transform.position=points[i];arch.transform.rotation=Quaternion.LookRotation(Forward(i));Label(i==0?"AETHER GROUNDS":"CHECKPOINT",points[i]+Vector3.up*8-Forward(i)*1.04f,Quaternion.LookRotation(Forward(i)),.29f);}
    if(i%32==16){for(int s=-1;s<=1;s+=2){var station=ModelLibrary.Create("TrackStation",decor);station.transform.position=points[i]+Right(i)*s*29;station.transform.rotation=Quaternion.LookRotation(Forward(i));}}
    if(i%7==0) {var p=points[i]+Vector3.up*.035f;var stripe=Cube("Center marking",p,new Vector3(.18f,.015f,2.2f),ModelLibrary.Material("mark",new Color(.69f,.85f,.85f),.05f,0),false);stripe.transform.rotation=Quaternion.LookRotation(Forward(i));stripe.transform.SetParent(decor,true);}
   }
   // A gridded starting line across the full road.
   for(int x=0;x<16;x++)for(int z=0;z<2;z++){Vector3 p=points[0]+Right(0)*(x*1.5f-11.25f)+Forward(0)*(z*1.3f)+Vector3.up*.06f;var q=Cube("Finish grid",p,new Vector3(1.5f,.02f,1.3f),ModelLibrary.Material((x+z)%2==0?"gridwhite":"gridblack",(x+z)%2==0?new Color(.8f,.94f,.94f):new Color(.06f,.08f,.12f),0,0),false);q.transform.rotation=Quaternion.LookRotation(Forward(0));q.transform.SetParent(decor,true);}
   for(int i=0;i<600;i++){
    Vector3 p=new Vector3(R(-630,630),0,R(-630,630));int nearest=Nearest(p);float dist=FlatDistance(p,points[nearest]);if(dist<34)continue;
    string asset=index==1?"Rock":i%4==0?"CrystalCluster":i%5==0?"Rock":"AlienTree";var o=ModelLibrary.Create(asset,decor);p.y=Height(p.x,p.z);o.transform.position=p;o.transform.localScale=Vector3.one*R(.65f,2.8f);o.transform.rotation=Quaternion.Euler(0,R(0,360),0);
    foreach(var r in o.GetComponentsInChildren<Renderer>()){string n=r.name;Color col=n=="Leaves"?foliage:n=="Rock"?stone:n=="Crystal"?glow:new Color(.14f,.17f,.26f);r.sharedMaterial=ModelLibrary.Material("world"+index+n,col,n=="Crystal"?.2f:0);}
   }
   // Dense designed verges keep detail visible at racing speed.
   for(int i=0;i<Count;i+=2)for(int s=-1;s<=1;s+=2){
    Vector3 p=points[i]+Right(i)*s*R(22,48)+Forward(i)*R(-4,4);p.y=Height(p.x,p.z);
    string asset=index==1?"Rock":i%10==0?"AlienTree":i%6==0?"CrystalCluster":"Rock";var o=ModelLibrary.Create(asset,decor);o.transform.position=p;o.transform.rotation=Quaternion.Euler(0,R(0,360),0);o.transform.localScale=Vector3.one*(asset=="AlienTree"?R(1.2f,2.8f):asset=="Rock"?R(.3f,1.1f):R(.7f,1.6f));
    foreach(var r in o.GetComponentsInChildren<Renderer>()){string n=r.name;Color c=n=="Leaves"?(i%20==0?new Color(.63f,.28f,.56f):foliage):n=="Rock"?stone:n=="Crystal"?glow:new Color(.13f,.18f,.27f);r.sharedMaterial=ModelLibrary.Material("verge"+map+n+(i%20==0),c,n=="Crystal"?.2f:0);}
    for(int k=0;k<3;k++){Vector3 a=points[i]+Right(i)*s*R(15,29)+Forward(i)*R(-6,6);a.y=Height(a.x,a.z);var plant=ModelLibrary.Create("CrystalCluster",decor);plant.transform.position=a;plant.transform.localScale=new Vector3(.65f,.23f,.65f);foreach(var r in plant.GetComponentsInChildren<Renderer>())r.sharedMaterial=ModelLibrary.Material("smallflora"+map+k,k==0?foliage:glow*.72f,.08f);}
   }
   // A turquoise basin and an island inside Prism Coast.
   if(index==0){var lake=GameObject.CreatePrimitive(PrimitiveType.Cylinder);lake.name="Lagoon";lake.transform.SetParent(transform);lake.transform.position=new Vector3(15,2.5f,20);lake.transform.localScale=new Vector3(264,.3f,250);Destroy(lake.GetComponent<Collider>());lake.GetComponent<Renderer>().sharedMaterial=WaterMaterial();for(int i=0;i<20;i++){float a=i*Mathf.PI*2/20;var o=ModelLibrary.Create("Rock",decor);o.transform.position=new Vector3(15+Mathf.Cos(a)*125,7,20+Mathf.Sin(a)*110);o.transform.localScale=new Vector3(7,3,5);}}
   for(int i=0;i<75;i++){
    float a=i*Mathf.PI*2/75;Vector3 p=new Vector3(Mathf.Cos(a)*R(690,1100),-12,Mathf.Sin(a)*R(690,1100));var rock=ModelLibrary.Create("Rock",decor);rock.transform.position=p;rock.transform.localScale=new Vector3(R(15,40),R(20,85),R(18,45));foreach(var r in rock.GetComponentsInChildren<Renderer>())r.sharedMaterial=ModelLibrary.Material("mountain"+index,stone*.8f);
   }
   Planet();CombineDecor();Presentation.Lighting(index);Presentation.Probe(transform,points[0]+Vector3.up*5);
   RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00085f;RenderSettings.fogColor=new[]{new Color(.46f,.66f,.75f),new Color(.70f,.43f,.28f),new Color(.12f,.15f,.26f)}[index];
   var sky=new Material(Shader.Find("Aether/Sky"));sky.SetColor("_Top",new[]{new Color(.085f,.28f,.52f),new Color(.28f,.15f,.22f),new Color(.018f,.035f,.095f)}[index]);sky.SetColor("_Horizon",RenderSettings.fogColor*1.15f);sky.SetFloat("_Night",index==2?1:0);RenderSettings.skybox=sky;
  }
  float R(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
  public Vector3 Forward(int i)=>isStunts?(StuntPosition(Mathf.Clamp(i,0,Count-1)*length/(Count-1)+1)-StuntPosition(Mathf.Clamp(i,0,Count-1)*length/(Count-1)-1)).normalized:isRush?Vector3.forward:(points[(i+1+Count)%Count]-points[(i-1+Count)%Count]).normalized;
  public Vector3 Right(int i)=>Vector3.Cross(Vector3.up,Forward(i)).normalized;
  public int Nearest(Vector3 p){if(isRush||isStunts)return Mathf.Clamp(Mathf.RoundToInt(p.z/length*(Count-1)),0,Count-1);int best=0;float d=float.MaxValue;for(int i=0;i<Count;i++){float q=(new Vector2(points[i].x-p.x,points[i].z-p.z)).sqrMagnitude;if(q<d){d=q;best=i;}}return best;}
  public static float FlatDistance(Vector3 a,Vector3 b)=>new Vector2(a.x-b.x,a.z-b.z).magnitude;
  public float Height(float x,float z){if(IsAirah)return AirahHeight(x,z);if(IsAirah)return AirahHeight(x,z);float baseH=-3+Mathf.PerlinNoise(x*.007f+17,z*.007f+39)*14;if(map==0){float basin=new Vector2(x-15,z-20).magnitude;baseH=Mathf.Lerp(-2,baseH,Mathf.SmoothStep(0,1,Mathf.InverseLerp(107,147,basin)));}int n=Nearest(new Vector3(x,0,z));float dist=FlatDistance(points[n],new Vector3(x,0,z));return Mathf.Lerp(points[n].y-.4f,baseH,Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,65,dist)));}
  void MakeRoad(){
   var vs=new List<Vector3>();var ts=new List<int>();var colors=new List<Color>();
   for(int i=0;i<=Count;i++){int j=i%Count;vs.Add(points[j]-Right(j)*Width/2);vs.Add(points[j]+Right(j)*Width/2);if(i<Count&&!Broken(i)){int k=i*2;ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
   MeshObject("Road",vs.ToArray(),ts.ToArray(),RoadMaterial(),true,transform);
   for(int i=0;i<Count;i++)if(Broken(i)){int j=(i+1)%Count;MeshObject("Dry bypass",new[]{points[i]+Right(i)*3.5f,points[i]+Right(i)*12,points[j]+Right(j)*3.5f,points[j]+Right(j)*12},new[]{0,2,1,1,2,3},RoadMaterial(),true,transform);}
   for(int s=-1;s<=1;s+=2){
    for(int i=0;i<Count;i++){
     Vector3 a=points[i]+Right(i)*s*(Width/2+.45f),b=points[(i+1)%Count]+Right((i+1)%Count)*s*(Width/2+.45f);
     var barrier=Cube("Safety barrier",(a+b)*.5f+Vector3.up*.58f,new Vector3(.65f,1.15f,Vector3.Distance(a,b)+.12f),ModelLibrary.Material((i%4<2?"railA":"railB")+map,i%4<2?new Color(.15f,.22f,.28f):glow*.65f),true);barrier.transform.rotation=Quaternion.LookRotation(b-a);barrier.transform.SetParent(decor,true);
     var curb=Cube("Curb",(a+b)*.5f-Right(i)*s*.8f+Vector3.up*.08f,new Vector3(.75f,.16f,Vector3.Distance(a,b)+.06f),ModelLibrary.Material((i%2==0?"curbA":"curbB")+map,i%2==0?new Color(.82f,.82f,.86f):glow*.65f,0,0),true);curb.transform.rotation=Quaternion.LookRotation(b-a);curb.transform.SetParent(decor,true);
    }
   }
  }
  Material RoadMaterial(){var m=ModelLibrary.Material("Road theme "+map,new[]{new Color(.19f,.26f,.29f),new Color(.38f,.24f,.16f),new Color(.22f,.22f,.31f)}[map],0,0);m.SetFloat("_Surface",map+1);m.SetFloat("_Smoothness",map==2?.5f:.24f);return m;}
  Material GroundMaterial(){var m=ModelLibrary.Material("ground"+map,ground,0,0);m.SetFloat("_Surface",map==1?2:3);m.SetFloat("_Smoothness",.12f);return m;}
  void MakeTerrain(){int size=140;var vs=new Vector3[(size+1)*(size+1)];var ts=new List<int>();for(int z=0;z<=size;z++)for(int x=0;x<=size;x++){float xx=(x-size/2)*10,zz=(z-size/2)*10;vs[z*(size+1)+x]=new Vector3(xx,Height(xx,zz),zz);if(x<size&&z<size){int a=z*(size+1)+x;ts.AddRange(new[]{a,a+size+1,a+1,a+1,a+size+1,a+size+2});}}MeshObject("Terrain",vs,ts.ToArray(),GroundMaterial(),true,transform);}
  void Planet(){var p=GameObject.CreatePrimitive(PrimitiveType.Sphere);p.name="Distant planet";p.transform.SetParent(transform);p.transform.position=new Vector3(250,580,1250);p.transform.localScale=Vector3.one*300;Destroy(p.GetComponent<Collider>());p.GetComponent<Renderer>().sharedMaterial=ModelLibrary.Material("planet",new Color(.77f,.58f,.70f),.15f,0);
   var ring=new GameObject("Planet rings").AddComponent<LineRenderer>();ring.transform.SetParent(transform);ring.useWorldSpace=true;ring.loop=true;ring.positionCount=128;ring.widthMultiplier=13;ring.sharedMaterial=ModelLibrary.Material("rings",new Color(.93f,.74f,.69f),.3f,0);for(int i=0;i<128;i++){float a=i*Mathf.PI*2/128;ring.SetPosition(i,p.transform.position+Quaternion.Euler(22,0,18)*new Vector3(Mathf.Cos(a)*235,0,Mathf.Sin(a)*235));}
  }
  public static GameObject Cube(string name,Vector3 pos,Vector3 scale,Material m,bool collider){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.position=pos;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=m;if(!collider)UnityEngine.Object.Destroy(o.GetComponent<Collider>());return o;}
  public static GameObject MeshObject(string name,Vector3[] vs,int[] ts,Material mat,bool collision,Transform parent){var m=new Mesh(){name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.vertices=vs;m.triangles=ts;m.RecalculateNormals();m.RecalculateBounds();var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=m;o.AddComponent<MeshRenderer>().sharedMaterial=mat;if(collision)o.AddComponent<MeshCollider>().sharedMesh=m;return o;}
  void Label(string text,Vector3 p,Quaternion q,float size){var o=new GameObject(text);o.transform.SetParent(decor);o.transform.position=p;o.transform.rotation=q;var t=o.AddComponent<TextMesh>();t.text=text;t.characterSize=size;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.8f,.96f,1);t.gameObject.AddComponent<StructureSign>();}
  void CombineDecor(){
   var groups=new Dictionary<Material,List<CombineInstance>>();foreach(var mf in decor.GetComponentsInChildren<MeshFilter>()){var r=mf.GetComponent<MeshRenderer>();if(!r)continue;if(!groups.ContainsKey(r.sharedMaterial))groups[r.sharedMaterial]=new List<CombineInstance>();groups[r.sharedMaterial].Add(new CombineInstance(){mesh=mf.sharedMesh,transform=mf.transform.localToWorldMatrix});r.enabled=false;}
   foreach(var g in groups){var m=new Mesh(){indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.CombineMeshes(g.Value.ToArray());var o=new GameObject("Combined landscape "+g.Key.name);o.transform.SetParent(transform);o.AddComponent<MeshFilter>().sharedMesh=m;o.AddComponent<MeshRenderer>().sharedMaterial=g.Key;}
  }
 }
}
