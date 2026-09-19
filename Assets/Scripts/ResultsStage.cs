using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Aether {
 public class ResultsStage:MonoBehaviour {
  public static readonly Color[] Medals={new Color(1,.77f,.28f),new Color(.78f,.88f,1),new Color(1,.74f,.52f)};
  public readonly Vehicle[] Winners=new Vehicle[3];
  public RenderTexture Output {get;private set;}
  public int PortraitCount=>portraits.Count;
  const int Layer=30;Camera cameraStage,portraitCamera;Game game;Color accent;
  readonly Dictionary<Vehicle,RenderTexture> portraits=new Dictionary<Vehicle,RenderTexture>();
  readonly Transform[] turntables=new Transform[3];readonly List<Transform> lights=new List<Transform>();
  readonly List<Material> owned=new List<Material>();readonly List<Transform> chasingLights=new List<Transform>();
  readonly Vector3[] podiums={new Vector3(0,2.5f,1.5f),new Vector3(-6.6f,1.5f,0),new Vector3(6.6f,1.05f,0)};
  Vector3 origin=new Vector3(0,-10000,0);float started;ReflectionProbe probe;
  public void Build(Game owner,Color color){
   game=owner;accent=color;started=Time.unscaledTime;transform.position=origin;
   var dark=Mat("Stage charcoal",new Color(.018f,.027f,.048f),.7f,.78f);
   var black=Mat("Stage black",new Color(.008f,.014f,.026f),.6f,.65f);
   var strip=Glow("Stage accent",accent,.8f);var warm=Glow("Gold light",new Color(1,.68f,.24f),.85f);var white=Glow("Softboxes",Color.white,.7f);
   Part("Reflective stage floor",PrimitiveType.Cube,new Vector3(0,-.35f,0),new Vector3(42,.6f,32),dark);
   Part("Stage rear wall",PrimitiveType.Cube,new Vector3(0,6,9),new Vector3(38,13,.4f),black);
   for(int i=0;i<23;i++){
    float x=(i-11)*1.45f;Part("Wall ribs",PrimitiveType.Cube,new Vector3(x,6,8.5f),new Vector3(.15f,12,.5f),dark);
    Part("Wall light columns",PrimitiveType.Cube,new Vector3(x,4.7f+Mathf.Sin(i*.7f),8.15f),new Vector3(.055f,7+Mathf.Sin(i*.7f)*2,.04f),i%3==0?warm:strip);
   }
   for(int i=0;i<5;i++){float z=-8+i*4;Part("Floor inlay",PrimitiveType.Cube,new Vector3(0,-.027f,z),new Vector3(33,.015f,.035f),strip);}
   for(int side=-1;side<=1;side+=2){
    Part("Light gantry",PrimitiveType.Cube,new Vector3(side*14,6,2),new Vector3(.35f,12,.4f),dark);
    Part("Vertical softbox",PrimitiveType.Cube,new Vector3(side*13,6,5),new Vector3(1.1f,8,.12f),white,new Vector3(0,-side*25,0));
   }
   Part("Overhead gantry",PrimitiveType.Cube,new Vector3(0,11.8f,2),new Vector3(29,.35f,.5f),dark);
   Part("Overhead reflection panel",PrimitiveType.Cube,new Vector3(0,10,0),new Vector3(15,.05f,4),white);
   for(int i=0;i<3;i++){
    Vector3 p=podiums[i];float height=p.y;
    var metal=Mat(i==0?"Brushed gold":i==1?"Polished silver":"Brushed bronze",i==0?new Color(1,.70f,.22f):i==1?new Color(.79f,.85f,.93f):new Color(.74f,.37f,.16f),1,i==0?.88f:.81f);
    metal.mainTexture=Resources.Load<Texture2D>("PaintStyles/BrushedMetal");metal.SetFloat("_Scale",.65f);
    Part("Podium "+(i+1),PrimitiveType.Cylinder,new Vector3(p.x,height*.5f,p.z),new Vector3(6.1f,height*.5f,6.1f),metal);
    Part("Podium display surface",PrimitiveType.Cylinder,new Vector3(p.x,height+.025f,p.z),new Vector3(5.85f,.045f,5.85f),black);
    Ring(new Vector3(p.x,height+.08f,p.z),3.03f,Glow("Medal ring "+i,Medals[i],.9f),.045f);
    Ring(new Vector3(p.x,.14f,p.z),3.13f,strip,.035f);
    for(int k=0;k<36;k++){float a=k*Mathf.PI*2/36;Part("Pedestal metal fluting",PrimitiveType.Cube,new Vector3(p.x+Mathf.Sin(a)*3.04f,height*.5f,p.z+Mathf.Cos(a)*3.04f),new Vector3(.035f,height-.23f,.07f),metal,new Vector3(0,a*Mathf.Rad2Deg,0));}
    var number=new GameObject("Podium place "+(i+1)).AddComponent<TextMesh>();number.transform.SetParent(transform,false);number.transform.localPosition=new Vector3(p.x,height*.52f,p.z-3.065f);number.text=(i+1).ToString();number.fontSize=100;number.characterSize=.33f;number.anchor=TextAnchor.MiddleCenter;number.color=i==0?new Color(.20f,.095f,.01f):new Color(.025f,.035f,.05f);number.gameObject.layer=Layer;
    var tt=new GameObject("Rotating car "+(i+1)).transform;tt.SetParent(transform,false);tt.localPosition=p+Vector3.up*.11f;turntables[i]=tt;
    for(int side=-1;side<=1;side+=2){var spot=Light("Podium spotlight",new Vector3(p.x+side*2,9,p.z-3),Color.Lerp(Medals[i],Color.white,.7f),1.7f,LightType.Spot,22);spot.spotAngle=48;spot.innerSpotAngle=27;spot.transform.LookAt(origin+p);spot.shadows=LightShadows.Soft;}
   }
   for(int i=0;i<8;i++){
    var light=Light("Moving show light",new Vector3((i-3.5f)*3.6f,10.9f,5.5f),i%2==0?accent:new Color(1,.8f,.4f),1.1f,LightType.Spot,30);light.spotAngle=28;light.innerSpotAngle=13;
    PremiumLighting.Beam(light.transform,23,3.2f,new Color(light.color.r,light.color.g,light.color.b,.012f));lights.Add(light.transform);
    Part("Moving light housing",PrimitiveType.Cylinder,light.transform.localPosition,new Vector3(.45f,.45f,.45f),dark);
   }
   for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;var lamp=Part("Animated stage perimeter",PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*13,.04f,Mathf.Cos(a)*7),new Vector3(.2f,.04f,.2f),strip);chasingLights.Add(lamp.transform);}
   Light("Car show key",new Vector3(-7,7,-9),new Color(.8f,.88f,1),1.5f,LightType.Point,35);
   Light("Car show warm rim",new Vector3(8,6,4),new Color(1,.71f,.4f),.9f,LightType.Point,30);
   // A real reflection capture gives the metal its softbox highlights and stage reflections.
   var probeObject=new GameObject("Car show reflection capture");probeObject.transform.SetParent(transform,false);probeObject.transform.localPosition=new Vector3(0,4,-2);probe=probeObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;probe.resolution=128;probe.cullingMask=1<<Layer;probe.size=new Vector3(80,50,70);probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=new Color(.07f,.09f,.14f);probe.farClipPlane=60;probe.intensity=1.5f;
   cameraStage=NewCamera("Podium camera");cameraStage.transform.localPosition=new Vector3(0,7.7f,-23);cameraStage.transform.LookAt(origin+new Vector3(0,2.5f,1));cameraStage.fieldOfView=47;
   Output=new RenderTexture(1600,760,24,RenderTextureFormat.ARGB32){name="Car show view",antiAliasing=2};Output.Create();cameraStage.targetTexture=Output;cameraStage.gameObject.AddComponent<Presentation>();cameraStage.enabled=true;
   portraitCamera=NewCamera("Car portrait camera");portraitCamera.clearFlags=CameraClearFlags.SolidColor;portraitCamera.backgroundColor=new Color(.035f,.055f,.085f,0);portraitCamera.fieldOfView=34;portraitCamera.farClipPlane=18;portraitCamera.enabled=false;
   Light("Portrait softbox",new Vector3(33,6,-4),Color.white,3,LightType.Point,17);
   Light("Portrait rim",new Vector3(40,4,3),accent,3,LightType.Point,16);
   SetLayer(gameObject);Refresh(game.racers);foreach(var racer in game.racers)MakePortrait(racer);probe.RenderProbe();
   if(!game.rushFailed){FinishCelebration.Confetti(origin+new Vector3(-7,4,0),transform);FinishCelebration.Confetti(origin+new Vector3(7,4,0),transform);SetLayer(gameObject);}
  }
  Camera NewCamera(string name){var go=new GameObject(name);go.transform.SetParent(transform,false);var c=go.AddComponent<Camera>();c.cullingMask=1<<Layer;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.008f,.015f,.028f);c.nearClipPlane=.1f;c.farClipPlane=90;c.allowHDR=true;return c;}
  Material Mat(string name,Color color,float metallic,float smoothness){var m=new Material(Shader.Find("Aether/Paint")){name=name,color=color};m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Style",6);m.SetFloat("_Scale",1);owned.Add(m);return m;}
  Material Glow(string name,Color color,float emission){var m=new Material(Shader.Find("Aether/Toon")){name=name,color=color};m.SetFloat("_Emission",emission);m.SetFloat("_Outline",0);owned.Add(m);return m;}
  GameObject Part(string name,PrimitiveType type,Vector3 p,Vector3 size,Material mat,Vector3 rotation=default)=>CarParts.Part(transform,name,type,p,size,mat,rotation);
  Light Light(string name,Vector3 p,Color color,float intensity,LightType type,float range){var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.localPosition=p;var l=go.AddComponent<Light>();l.type=type;l.color=color;l.intensity=intensity;l.range=range;l.cullingMask=1<<Layer;return l;}
  void Ring(Vector3 p,float radius,Material mat,float width){var go=new GameObject("Pedestal light ring");go.transform.SetParent(transform,false);go.transform.localPosition=p;var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=100;line.widthMultiplier=width;line.sharedMaterial=mat;for(int j=0;j<100;j++){float a=j*Mathf.PI*2/100;line.SetPosition(j,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}}
  static void SetLayer(GameObject go){foreach(var t in go.GetComponentsInChildren<Transform>(true))t.gameObject.layer=Layer;}
  GameObject CopyCar(Vehicle racer,Transform parent){var copy=Instantiate(racer.model,parent);copy.name=racer.spec.name+" · "+racer.driver;copy.transform.localPosition=Vector3.zero;copy.transform.localRotation=Quaternion.identity;copy.transform.localScale=Vector3.one;foreach(var script in copy.GetComponentsInChildren<MonoBehaviour>())script.enabled=false;foreach(var ps in copy.GetComponentsInChildren<ParticleSystem>())ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);foreach(var collider in copy.GetComponentsInChildren<Collider>())collider.enabled=false;foreach(var renderer in copy.GetComponentsInChildren<Renderer>()){renderer.enabled=true;renderer.shadowCastingMode=ShadowCastingMode.On;}foreach(var wheel in copy.GetComponentsInChildren<Transform>())if(wheel.name.StartsWith("WheelF")||wheel.name.StartsWith("WheelR"))wheel.localRotation=Quaternion.identity;SetLayer(copy);return copy;}
  void MakePortrait(Vehicle v){var model=CopyCar(v,transform);model.transform.localPosition=new Vector3(37,0,0);model.transform.localRotation=Quaternion.Euler(0,155,0);portraitCamera.transform.localPosition=new Vector3(41.5f,2.9f,-5.8f);portraitCamera.transform.LookAt(origin+new Vector3(37,.7f,0));var target=new RenderTexture(256,160,24,RenderTextureFormat.ARGB32){name=v.driver+" · "+v.spec.name+" portrait",antiAliasing=2};target.Create();portraitCamera.targetTexture=target;portraitCamera.Render();portraitCamera.targetTexture=null;portraits.Add(v,target);model.SetActive(false);Destroy(model);}
  public RenderTexture Portrait(Vehicle v)=>v&&portraits.TryGetValue(v,out var t)?t:null;
  public void Refresh(List<Vehicle> racers){for(int i=0;i<3;i++){if(Winners[i])continue;var car=racers.FirstOrDefault(v=>v.finishOrder==i+1);if(!car)continue;Winners[i]=car;CopyCar(car,turntables[i]).transform.localScale=Vector3.one*1.12f;}}
  public Vector2 PodiumLabel(int i,Rect rect){var uv=cameraStage.WorldToViewportPoint(origin+new Vector3(podiums[i].x,.02f,podiums[i].z-3.6f));return new Vector2(rect.x+uv.x*rect.width,rect.y+(1-uv.y)*rect.height);}
  void Update(){float t=Time.unscaledTime-started;for(int i=0;i<3;i++)if(turntables[i])turntables[i].localRotation=Quaternion.Euler(0,155+t*(i==0?15:12),0);for(int i=0;i<lights.Count;i++){Vector3 aim=origin+new Vector3(Mathf.Sin(t*.6f+i*1.2f)*11,.2f,-3+Mathf.Cos(t*.47f+i)*4);lights[i].LookAt(aim);}for(int i=0;i<chasingLights.Count;i++)chasingLights[i].localScale=new Vector3(.2f,.045f,Mathf.Lerp(.12f,.65f,.5f+.5f*Mathf.Sin(i*.3f-t*2)));}
  void OnDestroy(){if(Output){Output.Release();Destroy(Output);}foreach(var t in portraits.Values){t.Release();Destroy(t);}foreach(var mat in owned)if(mat)Destroy(mat);}
 }
}
