using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Aether {
 public partial class TrackWorld {
  public const int AirahMap=3;
  public static readonly Vector3[] AirahShape={
   new Vector3(-950,105,-1000),new Vector3(-300,125,-1150),new Vector3(400,150,-1080),new Vector3(1000,205,-750),
   new Vector3(1160,270,-200),new Vector3(900,330,210),new Vector3(450,355,130),new Vector3(180,385,400),
   new Vector3(550,420,660),new Vector3(1020,465,850),new Vector3(930,490,1190),new Vector3(350,455,1270),
   new Vector3(-250,390,1130),new Vector3(-800,340,900),new Vector3(-1080,280,500),new Vector3(-880,230,160),
   new Vector3(-450,220,120),new Vector3(-280,195,-200),new Vector3(-650,160,-390),new Vector3(-1110,125,-570)};
  public bool IsAirah=>!isRush&&!isStunts&&map==AirahMap;
  public const int AirahGapStart=33,AirahGapEnd=37;
  public bool AirahGap(int n)=>IsAirah&&n>=AirahGapStart&&n<AirahGapEnd;
  public int airahTrees,airahRocks;public float airahMinHeight,airahMaxHeight;
  readonly List<AirahInstances> airahBatches=new List<AirahInstances>();
  static Vector3 AirahSpline(float t){t=Mathf.Repeat(t,1)*AirahShape.Length;int n=(int)t;float f=t-n;int c=AirahShape.Length;var a=AirahShape[(n+c-1)%c];var b=AirahShape[n];var d=AirahShape[(n+1)%c];var e=AirahShape[(n+2)%c];return .5f*(2*b+(-a+d)*f+(2*a-5*b+4*d-e)*f*f+(-a+3*b-3*d+e)*f*f*f);}
  public void GenerateAirah(){
   map=AirahMap;rng=new System.Random(190926);length=0;
   // Equal-distance samples keep driving lookahead, progress and recovery consistent over a long lap.
   const int samples=6000;var raw=new Vector3[samples+1];var dist=new float[samples+1];
   for(int i=0;i<=samples;i++){raw[i]=AirahSpline(i/(float)samples);if(i>0)dist[i]=dist[i-1]+Vector3.Distance(raw[i],raw[i-1]);}
   length=dist[samples];int cursor=1;airahMinHeight=9999;airahMaxHeight=-9999;
   for(int i=0;i<Count;i++){float d=i*length/Count;while(cursor<samples&&dist[cursor]<d)cursor++;points[i]=Vector3.Lerp(raw[cursor-1],raw[cursor],Mathf.InverseLerp(dist[cursor-1],dist[cursor],d));airahMinHeight=Mathf.Min(airahMinHeight,points[i].y);airahMaxHeight=Mathf.Max(airahMaxHeight,points[i].y);}
   points[31].y+=1;points[32].y+=3;points[33].y+=6;
   AirahTerrain();AirahRoad();AirahForest();MakeFeatures();AirahBrokenBridge();AirahPlaces();AirahTraffic.Build(this);AirahLighting();
   Debug.Log("AIRAH generated length="+length+" elevation="+airahMinHeight+".."+airahMaxHeight+" trees="+airahTrees+" rocks="+airahRocks);
  }
  static float Peak(float x,float z,float px,float pz,float h,float w){float d=((x-px)*(x-px)+(z-pz)*(z-pz))/(w*w);return h*Mathf.Exp(-d);}
  public float AirahNaturalHeight(float x,float z){
   float h=32+Peak(x,z,0,650,550,650)+Peak(x,z,-500,1700,850,660)+Peak(x,z,1450,1400,760,600)+Peak(x,z,-1700,800,780,660)+Peak(x,z,1700,-400,560,580)+Peak(x,z,-1800,-1400,500,700);
   h+=Mathf.PerlinNoise(x*.0019f+71,z*.0019f+18)*90;
   h+=(Mathf.PerlinNoise(x*.006f+11,z*.006f+81)-.5f)*55+(Mathf.PerlinNoise(x*.021f+10,z*.021f+21)-.5f)*12;
   var ravine=points[35];h-=Peak(x,z,ravine.x,ravine.z,95,190);return h;
  }
  public float AirahHeight(float x,float z){
   Vector3 p=new Vector3(x,0,z);int n=Nearest(p);float best=float.MaxValue,roadY=points[n].y;
   for(int k=-1;k<=0;k++){int i=(n+k+Count)%Count,j=(i+1)%Count;Vector3 a=points[i],b=points[j];var ab=new Vector2(b.x-a.x,b.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(new Vector2(x-a.x,z-a.z),ab)/Mathf.Max(.01f,ab.sqrMagnitude));Vector3 q=Vector3.Lerp(a,b,t);float d=FlatDistance(p,q);if(d<best){best=d;roadY=q.y;}}
   float natural=AirahNaturalHeight(x,z);float bed=natural<roadY-24?natural:roadY-.65f;
   return Mathf.Lerp(bed,natural,Mathf.SmoothStep(0,1,Mathf.InverseLerp(19,75,best)));
  }
  Material AirahMat(string name,Color color,float glow=0){var m=ModelLibrary.Material("Airah "+name,color,glow,0);m.SetFloat("_Smoothness",.12f);m.SetFloat("_Metallic",0);return m;}
  void AirahPaint(Transform root,Vector3 a,Vector3 b,float width,Material mat){var o=CarParts.Part(root,"Painted road line",PrimitiveType.Cube,(a+b)*.5f,new Vector3(width,.012f,Vector3.Distance(a,b)),mat);o.transform.rotation=Quaternion.LookRotation(b-a);}
  void AirahTerrain(){
   var mat=new Material(Shader.Find("Aether/AirahTerrain")){name="Airah mountain meadow and granite"};mat.mainTexture=Resources.Load<Texture2D>("Airah/GraniteMoss");mat.SetTexture("_Meadow",Resources.Load<Texture2D>("Airah/Meadow"));mat.enableInstancing=true;
   var owned=gameObject.AddComponent<AirahOwnedMaterials>();owned.materials.Add(mat);const int chunks=16,steps=24;const float span=360;
   for(int cz=0;cz<chunks;cz++)for(int cx=0;cx<chunks;cx++){
    var vs=new Vector3[(steps+1)*(steps+1)];var ts=new int[steps*steps*6];int k=0;
    for(int z=0;z<=steps;z++)for(int x=0;x<=steps;x++){float xx=(cx-chunks/2)*span+x*span/steps,zz=(cz-chunks/2)*span+z*span/steps;int i=z*(steps+1)+x;vs[i]=new Vector3(xx,AirahHeight(xx,zz),zz);if(x<steps&&z<steps){ts[k++]=i;ts[k++]=i+steps+1;ts[k++]=i+1;ts[k++]=i+1;ts[k++]=i+steps+1;ts[k++]=i+steps+2;}}
    var o=MeshObject("Airah terrain "+cx+","+cz,vs,ts,mat,true,transform);o.AddComponent<OwnedMesh>();o.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
   }
  }
  void AirahRoad(){
   Material asphalt=AirahMat("mountain asphalt",new Color(.25f,.27f,.27f));asphalt.mainTexture=Resources.Load<Texture2D>("Textures/DowntownAsphalt");asphalt.SetFloat("_TextureMix",.65f);asphalt.SetFloat("_TextureScale",.13f);
   var steel=AirahMat("weathered trestle",new Color(.31f,.19f,.115f));var rail=AirahMat("galvanized guardrail",new Color(.52f,.56f,.53f));var white=AirahMat("ivory road paint",new Color(.86f,.85f,.70f));var yellow=AirahMat("amber reflectors",new Color(.95f,.59f,.12f),.2f);var concrete=AirahMat("bridge piers",new Color(.46f,.44f,.37f));
   for(int start=0;start<Count;start+=12){var sector=DetailSector("Airah road structure "+start);var vs=new List<Vector3>();var ts=new List<int>();
    for(int s=0;s<=12;s++){int i=(start+s)%Count;vs.Add(points[i]-Right(i)*12);vs.Add(points[i]+Right(i)*12);if(s<12&&!AirahGap(i)){int k=s*2;ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
    MeshObject("Continuous mountain road",vs.ToArray(),ts.ToArray(),asphalt,true,sector).AddComponent<OwnedMesh>();
    for(int j=0;j<12;j++){int i=(start+j)%Count,n=(i+1)%Count;if(AirahGap(i))continue;Vector3 a=points[i],b=points[n];float len=Vector3.Distance(a,b);Quaternion q=Quaternion.LookRotation(b-a);
     for(int side=-1;side<=1;side+=2){Vector3 pa=a+Right(i)*side*12.5f,pb=b+Right(n)*side*12.5f;
      DetailBeam(sector,"Safety guardrail",pa+Vector3.up*.92f,pb+Vector3.up*.92f,.24f,rail);
      var barrier=new GameObject("Roadside collision rail");barrier.transform.SetParent(sector);barrier.transform.SetPositionAndRotation((pa+pb)*.5f+Vector3.up*.65f,q);barrier.AddComponent<BoxCollider>().size=new Vector3(.45f,1.3f,len+.15f);
      AirahPaint(sector,a+Right(i)*side*11.15f+Vector3.up*.025f,b+Right(n)*side*11.15f+Vector3.up*.025f,.14f,white);
      for(int r=0;r<3;r++){float t=r/3f;var p=Vector3.Lerp(pa,pb,t);DetailBox(sector,"Guardrail upright",p+Vector3.up*.4f,new Vector3(.15f,.85f,.17f),rail);DetailBox(sector,"Amber reflector",p+Vector3.up*1.03f,new Vector3(.18f,.14f,.22f),yellow);}
      if(AirahHeight(a.x,a.z)<a.y-20){
       DetailBeam(sector,"Trestle upper chord",pa-Vector3.up*1.2f,pb-Vector3.up*1.2f,.75f,steel);DetailBeam(sector,"Trestle lower chord",pa-Vector3.up*9,pb-Vector3.up*9,.6f,steel);
       for(int r=0;r<3;r++){var x=Vector3.Lerp(pa,pb,r/3f);var y=Vector3.Lerp(pa,pb,(r+1)/3f);DetailBeam(sector,"Trestle X brace",x-Vector3.up*1.5f,y-Vector3.up*9,.27f,steel);DetailBeam(sector,"Trestle X brace",x-Vector3.up*9,y-Vector3.up*1.5f,.27f,steel);}
       if(i%3==0){float baseY=AirahHeight(pa.x,pa.z);DetailBox(sector,"Concrete bridge footing",new Vector3(pa.x,baseY+1.8f,pa.z),new Vector3(5,4,5),concrete);DetailBeam(sector,"Trestle support pier",new Vector3(pa.x,baseY+2,pa.z),pa-Vector3.up*1.5f,1.9f,steel);}
      }
     }
     if(j%2==0){AirahPaint(sector,a+Vector3.up*.025f,Vector3.Lerp(a,b,.5f)+Vector3.up*.025f,.18f,white);DetailBeam(sector,"Under deck crossbeam",a-Right(i)*12.5f-Vector3.up*2,a+Right(i)*12.5f-Vector3.up*2,.7f,steel);}
    }
    // Keep road collider separate; merge only decorative structure into a few spatially culled meshes.
    AirahCombine(sector);
   }
  }
  void AirahCombine(Transform root){
   var groups=new Dictionary<Material,List<CombineInstance>>();var sources=new List<GameObject>();
   foreach(var mf in root.GetComponentsInChildren<MeshFilter>()){if(mf.GetComponent<Collider>()&&mf.GetComponent<Collider>().enabled)continue;var r=mf.GetComponent<MeshRenderer>();if(!r||!r.enabled)continue;if(!groups.ContainsKey(r.sharedMaterial))groups[r.sharedMaterial]=new List<CombineInstance>();groups[r.sharedMaterial].Add(new CombineInstance{mesh=mf.sharedMesh,transform=root.worldToLocalMatrix*mf.transform.localToWorldMatrix});sources.Add(mf.gameObject);}
   foreach(var g in groups){var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(g.Value.ToArray());var o=new GameObject("Airah batched detail");o.transform.SetParent(root,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=g.Key;o.AddComponent<OwnedMesh>();}
   foreach(var o in sources)Destroy(o);
  }
  void AirahForest(){
   var sets=new Dictionary<string,List<Matrix4x4>>();var bounds=new Dictionary<string,Bounds>();
   void Put(string asset,Vector3 p,Vector3 scale,float yaw){string key=asset+":"+Mathf.FloorToInt(p.x/240)+":"+Mathf.FloorToInt(p.z/240);if(!sets.ContainsKey(key)){sets[key]=new List<Matrix4x4>();bounds[key]=new Bounds(p,Vector3.one*35);}var b=bounds[key];b.Encapsulate(p+Vector3.up*25);bounds[key]=b;sets[key].Add(Matrix4x4.TRS(p,Quaternion.Euler(0,yaw,0),scale));}
   for(int i=0;i<19500;i++){
    float x=R(-2650,2650),z=R(-2400,2700);int near=Nearest(new Vector3(x,0,z));float distance=FlatDistance(new Vector3(x,0,z),points[near]);if(distance<30)continue;
    float y=AirahHeight(x,z);float slope=Mathf.Abs(AirahNaturalHeight(x+8,z)-AirahNaturalHeight(x-8,z))+Mathf.Abs(AirahNaturalHeight(x,z+8)-AirahNaturalHeight(x,z-8));
    bool rock=i%5==0||y>660||slope>28;
    if(rock){Put("AirahRock"+(i%4),new Vector3(x,y-.3f,z),new Vector3(R(1,4),R(1,3.6f),R(1,4)),R(0,360));airahRocks++;}
    else if(Mathf.PerlinNoise(x*.007f+45,z*.007f+98)>.30f){float scale=R(.6f,1.5f);Put(i%9==0?"AirahBroadleaf":"AirahPine"+i%3,new Vector3(x,y-.3f,z),Vector3.one*scale,R(0,360));airahTrees++;}
   }
   for(int i=0;i<Count;i++)for(int s=-1;s<=1;s+=2)for(int j=0;j<4;j++){
    Vector3 p=points[i]+Right(i)*s*R(20,65)+Forward(i)*R(-9,9);p.y=AirahHeight(p.x,p.z);if(FlatDistance(p,points[Nearest(p)])<18)continue;
    string asset=j==0?"AirahRock"+i%4:j==1?"AirahFern":j==2?"AirahPine"+i%3:i%7==0?"AirahLog":"AirahFern";float sc=asset.Contains("Fern")?R(.7f,1.7f):R(.7f,1.4f);Put(asset,p,Vector3.one*sc,R(0,360));if(asset.Contains("Pine"))airahTrees++;else if(asset.Contains("Rock"))airahRocks++;
   }
   var templates=new Dictionary<string,GameObject>();
   var pineFar=ModelLibrary.Create("AirahPineFar",transform);pineFar.SetActive(false);var broadleafFar=ModelLibrary.Create("AirahBroadleafFar",transform);broadleafFar.SetActive(false);
   foreach(var set in sets){string name=set.Key.Split(':')[0];if(!templates.TryGetValue(name,out var template)){template=ModelLibrary.Create(name,transform);template.SetActive(false);templates[name]=template;
     foreach(var r in template.GetComponentsInChildren<MeshRenderer>(true))if(r.sharedMaterial.name=="Airah Granite"){r.sharedMaterial.mainTexture=Resources.Load<Texture2D>("Airah/GraniteMoss");r.sharedMaterial.SetFloat("_TextureMix",.8f);r.sharedMaterial.SetFloat("_TextureScale",.15f);r.sharedMaterial.SetFloat("_Smoothness",.08f);}
    }
    var batch=new AirahInstances{bounds=bounds[set.Key],matrices=set.Value.ToArray(),range=name.Contains("Fern")?230:name.Contains("Log")?350:name.Contains("Rock")?1550:3600};
    if(name.Contains("Pine"))batch.farMeshes=pineFar.GetComponentsInChildren<MeshFilter>(true);else if(name.Contains("Broadleaf"))batch.farMeshes=broadleafFar.GetComponentsInChildren<MeshFilter>(true);
    batch.meshes=template.GetComponentsInChildren<MeshFilter>(true);airahBatches.Add(batch);
   }
   var draw=gameObject.AddComponent<AirahForestRenderer>();draw.batches=airahBatches;detailPropCount=airahTrees+airahRocks;detailTypes.Add("Original Blender mountain forest");
  }
  void AirahPlaces(){
   foreach(int n in new[]{0,82,162,254,347,420}){
    var root=DetailSector("Mountain overlook "+n,points[n]+Right(n)*24,Quaternion.LookRotation(Forward(n)));
    var deck=CarParts.Part(root,"Overlook deck",PrimitiveType.Cube,new Vector3(0,-.5f,0),new Vector3(23,1,28),AirahMat("overlook concrete",new Color(.47f,.45f,.37f)));var collision=new GameObject("Overlook collision");collision.transform.SetParent(root,false);collision.transform.localPosition=new Vector3(0,-.5f,0);collision.AddComponent<BoxCollider>().size=new Vector3(23,1,28);
    var shelter=ModelLibrary.Create("AirahShelter",root);shelter.transform.localPosition=new Vector3(5,0,0);
    for(int i=0;i<3;i++)Detail(root:root,pack:"nature",name:"flower_yellowA",p:new Vector3(8,0,-10+i*10),size:1.5f,yaw:i*40);
    for(int s=-1;s<=1;s+=2){DetailBeam(root,"Overlook railing",new Vector3(s*11,1,-14),new Vector3(s*11,1,14),.18f,DetailMetal);}
    AirahCombine(root);
    Sign(n,n==0?"Airah Mountains":n==82?"Pine Valley":n==162?"Granite Pass":n==254?"Summit View":n==347?"Cedar Ridge":"Valley Return",new Color(.91f,.85f,.61f));
   }
   // Small discoverable dedication at the uphill overlook, outside the race lane.
   int egg=254;var sign=new GameObject("Airah developer Easter egg").transform;sign.SetParent(transform);sign.SetPositionAndRotation(points[egg]+Right(egg)*19+Forward(egg)*11,Quaternion.LookRotation(Forward(egg)));
   CarParts.Part(sign,"Timber post",PrimitiveType.Cube,new Vector3(0,1.8f,.22f),new Vector3(.22f,3.6f,.22f),AirahMat("dedication timber",new Color(.24f,.14f,.07f)));
   CarParts.Part(sign,"Dedication sign",PrimitiveType.Cube,new Vector3(0,3.2f,0),new Vector3(7.8f,1.8f,.18f),AirahMat("forest sign",new Color(.055f,.15f,.10f)));
   var text=new GameObject("made by Aljay Leodones").AddComponent<TextMesh>();text.transform.SetParent(sign,false);text.transform.localPosition=new Vector3(0,3.2f,-.105f);text.text="Airah Mountains\nmade by Aljay Leodones";text.fontSize=64;text.characterSize=.095f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.98f,.92f,.73f);text.gameObject.AddComponent<StructureSign>();
   for(int x=0;x<16;x++)for(int z=0;z<2;z++){var o=Cube("Airah finish grid",points[0]+Right(0)*(x*1.5f-11.25f)+Forward(0)*z*1.5f+Vector3.up*.075f,new Vector3(1.5f,.025f,1.5f),AirahMat("grid "+(x+z)%2,(x+z)%2==0?Color.white:new Color(.05f,.06f,.06f)),false);o.transform.SetParent(transform);o.transform.rotation=Quaternion.LookRotation(Forward(0));}
   var arch=ModelLibrary.Create("RaceArch",transform);arch.transform.SetPositionAndRotation(points[0],Quaternion.LookRotation(Forward(0)));
   for(int n=18;n<Count;n+=28)for(int s=-1;s<=1;s++)pickups.Add(points[n]+Right(n)*s*6+Vector3.up*1.25f);
  }
  void AirahLighting(){
   Presentation.Lighting(0);var sun=GameObject.Find("Sun").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(29,-56,0);sun.color=new Color(1,.89f,.68f);sun.intensity=1.65f;
   RenderSettings.ambientSkyColor=new Color(.40f,.53f,.66f);RenderSettings.ambientEquatorColor=new Color(.37f,.40f,.30f);RenderSettings.ambientGroundColor=new Color(.20f,.24f,.14f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00027f;RenderSettings.fogColor=new Color(.64f,.76f,.79f);
   var sky=new Material(Shader.Find("Aether/AirahSky"));GetComponent<AirahOwnedMaterials>().materials.Add(sky);sky.SetVector("_SunDirection",-sun.transform.forward);RenderSettings.skybox=sky;DynamicGI.UpdateEnvironment();Presentation.Probe(transform,points[0]+Vector3.up*7);
  }
 }
 public class AirahInstances {public Bounds bounds;public Matrix4x4[] matrices;public MeshFilter[] meshes,farMeshes;public float range;}
 public class AirahOwnedMaterials:MonoBehaviour {public readonly List<Material> materials=new List<Material>();void OnDestroy(){foreach(var m in materials)if(m)Destroy(m);}}
 public class AirahForestRenderer:MonoBehaviour {
  [NonSerialized]public List<AirahInstances> batches;public int visibleInstances;readonly Plane[] planes=new Plane[6];Game game;Camera cameraRef;
  void Start(){game=FindAnyObjectByType<Game>();cameraRef=Camera.main;}
  void LateUpdate(){if(!cameraRef||batches==null)return;visibleInstances=0;GeometryUtility.CalculateFrustumPlanes(cameraRef,planes);var pos=cameraRef.transform.position;bool phone=game&&game.phoneMode;
   foreach(var b in batches){float range=b.range*(phone?.63f:1);if(b.bounds.SqrDistance(pos)>range*range||!GeometryUtility.TestPlanesAABB(planes,b.bounds))continue;
    visibleInstances+=b.matrices.Length;var meshes=b.farMeshes!=null&&b.bounds.SqrDistance(pos)>(phone?360*360:750*750)?b.farMeshes:b.meshes;foreach(var mf in meshes){var mat=mf.GetComponent<MeshRenderer>().sharedMaterial;Graphics.DrawMeshInstanced(mf.sharedMesh,0,mat,b.matrices,b.matrices.Length,null,b.bounds.SqrDistance(pos)<10000?ShadowCastingMode.On:ShadowCastingMode.Off,true,0,cameraRef,LightProbeUsage.Off);}
   }
  }
 }
}
