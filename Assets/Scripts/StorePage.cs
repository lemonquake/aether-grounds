using UnityEngine;
namespace Aether {
 public static partial class SupportStore {
  public static Vector4 Bonus(SavedCar car,bool preview=false){
   Vector4 bonus=Vector4.zero;var p=Product(car.supportEdition);
   if(p!=null&&p.body==car.body&&(preview||Owned(p.id))){switch(p.id){
    case "pip-city":bonus=new Vector4(3,1.5f,2,.5f);break;
    case "comet-chrome":bonus=new Vector4(6,2,3,.6f);break;
    case "nomad-night":bonus=new Vector4(5,2.5f,4,1.4f);break;
    case "manta-gold":bonus=new Vector4(10,3,4,1.2f);break;
    case "vanta-gold":bonus=new Vector4(12,3.5f,4,1.1f);break;
    case "vanta-chrome":bonus=new Vector4(9,2.8f,6,1.7f);break;
   }}
   if(car.neonWheels&&(preview||Owned("neon-wheels")))bonus.w+=.5f;
   if(car.starTrail&&(preview||Owned("star-trail")))bonus.y+=1;
   if(car.glassWheels&&(preview||Owned("glass-wheels"))){bonus.w+=1.1f;bonus.z+=2;}
   if(car.glassChassis&&(preview||Owned("glass-chassis"))){bonus.x+=4;bonus.y+=2;}
   return bonus;
  }
  public static CarSpec Tuned(CarSpec s,SavedCar car){var b=Bonus(car);return new CarSpec(s.name,s.description,s.speed+b.x,s.accel+b.y,s.brake+b.z,s.grip+b.w,s.mass);}
  public static SavedCar PreviewCar(SupportProduct p){return new SavedCar{body=Mathf.Max(0,p.body),paint=p.id.Contains("gold")?9:24,accent=26,engine=p.id=="glass-chassis"?8:0,supportEdition=p.body>=0?p.id:"",neonWheels=p.id=="neon-wheels",starTrail=p.id=="star-trail",glassWheels=p.id=="glass-wheels",glassChassis=p.id=="glass-chassis"};}
  static Material glassMaterial;
  public static Material GlassMaterial(){if(glassMaterial)return glassMaterial;glassMaterial=new Material(Shader.Find("Aether/StoreGlass")){name="Clear cyan glass"};return glassMaterial;}
  static void ApplyFinishes(GameObject model,SavedCar car,bool preview){
   bool glass=car.glassChassis&&(preview||Owned("glass-chassis"));
   bool edition=(preview||Owned(car.supportEdition))&&(car.supportEdition=="vanta-gold"||car.supportEdition=="vanta-chrome")&&car.body==0;
   if(edition||glass)foreach(var r in model.GetComponentsInChildren<MeshRenderer>())if(r.name=="Paint"||r.name=="Accent"){
    if(glass)r.sharedMaterial=GlassMaterial();else {var m=ModelLibrary.Material("Store Metal "+car.supportEdition,car.supportEdition=="vanta-gold"?new Color(1,.64f,.16f):new Color(.78f,.88f,.98f));m.SetFloat("_Metallic",1);m.SetFloat("_Smoothness",.98f);r.sharedMaterial=m;}
   }
   if(car.glassWheels&&(preview||Owned("glass-wheels")))foreach(string n in new[]{"WheelFL","WheelFR","WheelRL","WheelRR"}){var hub=model.transform.Find(n);if(hub)foreach(var r in hub.GetComponentsInChildren<MeshRenderer>())if(r.name.Contains("recess"))r.enabled=false;else if(!r.name.Contains("Tire"))r.sharedMaterial=GlassMaterial();}
   if(glass){var frame=ModelLibrary.Material("Glass chassis frame",new Color(.08f,.15f,.19f));for(int s=-1;s<=1;s+=2)CarParts.Part(model.transform,"Visible chassis rail",PrimitiveType.Cube,new Vector3(s*.65f,.5f,0),new Vector3(.1f,.12f,3.4f),frame);CarParts.Part(model.transform,"Visible battery",PrimitiveType.Cube,new Vector3(0,.45f,0),new Vector3(1.1f,.24f,1.8f),ModelLibrary.Material("Glass battery",new Color(.1f,.7f,.72f),.25f));}
  }
 }
 public class StorePreview:MonoBehaviour {
  public RenderTexture[] images;GameObject[] models;Camera cameraView;Game game;float yaw=35,zoom=7.2f;
  public void Initialize(Game g){game=g;int count=SupportStore.Products.Length;images=new RenderTexture[count];models=new GameObject[count];
   var cameraObject=new GameObject("Store preview camera");cameraObject.transform.SetParent(transform,false);cameraView=cameraObject.AddComponent<Camera>();cameraView.enabled=false;cameraView.cullingMask=1<<11;cameraView.clearFlags=CameraClearFlags.SolidColor;cameraView.backgroundColor=new Color(.025f,.045f,.065f);cameraView.fieldOfView=30;cameraView.nearClipPlane=.1f;cameraView.farClipPlane=30;
   for(int i=0;i<count;i++){var p=SupportStore.Products[i];var car=SupportStore.PreviewCar(p);var model=ModelLibrary.Create(CarSpec.All[car.body].name,transform);models[i]=model;model.transform.position=new Vector3(10000+i*40,100,10000);CarParts.Apply(model,new SavedCar{body=car.body,paint=car.paint,accent=car.accent,engine=car.engine});SupportStore.Apply(model,car,true);
    if(car.starTrail){var stars=RaceEffects.Emitter(model.transform,"Starlight preview",new Color(.7f,.4f,1),1.4f,.18f,3,70);stars.transform.localPosition=Vector3.back*2;stars.transform.localRotation=Quaternion.Euler(0,180,0);var emission=stars.emission;emission.rateOverTime=32;}
    foreach(var t in model.GetComponentsInChildren<Transform>())t.gameObject.layer=11;
    var lamp=new GameObject("Preview soft light").AddComponent<Light>();lamp.transform.SetParent(model.transform,false);lamp.transform.localPosition=new Vector3(-3,5,4);lamp.type=LightType.Point;lamp.range=20;lamp.intensity=3;lamp.cullingMask=1<<11;
    images[i]=new RenderTexture(720,400,24){name=p.name+" preview",antiAliasing=2};images[i].Create();Render(i,30);
   }
  }
  void Render(int i,float angle){var model=models[i];var p=model.transform.position;cameraView.transform.position=p+Quaternion.Euler(0,angle,0)*new Vector3(0,2.4f,zoom);cameraView.transform.LookAt(p+Vector3.up*.65f);cameraView.targetTexture=images[i];cameraView.Render();cameraView.targetTexture=null;}
  void Update(){if(game.state==Game.State.Menu&&(game.menu=="Store"||game.menu=="Support")){var m=game.ScreenToUI(Input.mousePosition);bool hover=new Rect(729,232,834,292).Contains(m);if(hover&&Input.GetMouseButton(0)){yaw-=Input.GetAxis("Mouse X")*4;if(game.phoneMode&&Input.touchCount>0)yaw-=Input.GetTouch(0).deltaPosition.x*.2f;}else yaw+=Time.unscaledDeltaTime*9;if(hover)zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*.4f,5.8f,9.5f);Render(game.StoreSelection,yaw);}}
  void OnDestroy(){if(images!=null)foreach(var image in images){image.Release();Destroy(image);}}
 }
 public partial class Game {
  public int StoreSelection=>supportChoice;StorePreview storePreview;int storePage;
  public void SelectStoreProduct(int index){supportChoice=Mathf.Clamp(index,0,SupportStore.Products.Length-1);storePage=supportChoice/4;}
  void EnsureStorePreview(){if(storePreview)return;storePreview=new GameObject("Store product previews").AddComponent<StorePreview>();storePreview.transform.SetParent(showroom.transform);storePreview.Initialize(this);}
  void StoreGUI(){EnsureStorePreview();Panel(0,98,1600,802,.98f);Label("Store",35,116,700,60,43,paper,true);Label("Cars, wheels and chassis · inspect every item before buying",38,178,1110,35,22,muted);
   int pages=(SupportStore.Products.Length+3)/4;if(Button("‹",38,232,56,42))storePage=(storePage+pages-1)%pages;Label("Collection "+(storePage+1)+" / "+pages,110,239,305,32,21,muted);if(Button("›",639,232,56,42))storePage=(storePage+1)%pages;
   for(int j=0;j<4;j++){int i=storePage*4+j;if(i>=SupportStore.Products.Length)break;var p=SupportStore.Products[i];float x=38+j%2*335,y=292+j/2*213;Card(x,y,320,200);GUI.DrawTexture(new Rect(x+2,y+2,316,116),storePreview.images[i],ScaleMode.ScaleAndCrop);Label(p.name,x+12,y+115,298,42,20,p.color,true);if(Button((SupportStore.Owned(p.id)?"Owned":"$"+p.price)+" · Preview",x+10,y+156,300,36,supportChoice==i))supportChoice=i;}
   var selected=SupportStore.Products[supportChoice];var demo=SupportStore.PreviewCar(selected);var b=SupportStore.Bonus(demo,true);Card(727,230,838,487);GUI.DrawTexture(new Rect(729,232,834,292),storePreview.images[supportChoice],ScaleMode.ScaleAndCrop);Label("Drag to rotate · scroll to zoom",752,239,780,28,18,muted);Label(selected.name,753,531,790,43,30,Color.Lerp(selected.color,Color.white,.35f),true);Label(selected.description,754,579,777,58,21,muted);
   Label("Speed +"+(b.x*3.6f).ToString("0.0")+" km/h   Accel +"+b.y.ToString("0.0")+"   Brake +"+b.z.ToString("0.0")+"   Grip +"+b.w.ToString("0.0"),754,648,790,40,21,cyan,true);
   Label("$"+selected.price+" USD · one-time",38,737,330,34,23,paper,true);
   if(SupportStore.Owned(selected.id)){if(Button(StoreEquipped(selected)?"Remove item":"Equip on saved car",38,782,320,47,true)){EquipStore(selected);}}
   else if(Button("Buy with PayPal",38,782,320,47,true))Application.OpenURL(SupportStore.Checkout(selected,SupportStore.PlayerId));
   Label("Manual delivery: email your receipt + Player ID to\nAljay Leodones · lemonquake@gmail.com",379,735,1147,55,20,muted);Label("Player ID: "+SupportStore.PlayerId,379,801,385,28,18,cyan);if(Button("Copy ID",765,794,128,37))GUIUtility.systemCopyBuffer=SupportStore.PlayerId;
   Label("Unlock code",38,850,181,30,21);unlockCode=GUI.TextField(new Rect(207,844,959,39),unlockCode,1500,new GUIStyle(GUI.skin.textField){fontSize=20,padding=new RectOffset(8,8,7,7)});if(Button("Paste",1180,844,130,39))unlockCode=GUIUtility.systemCopyBuffer;if(Button("Unlock",1325,844,240,39,true))Toast(SupportStore.Redeem(unlockCode)?"Item unlocked · ready to equip":"Code not valid for this Player ID");
  }
  bool StoreEquipped(SupportProduct p)=>p.body>=0?CurrentCar.body==p.body&&CurrentCar.supportEdition==p.id:p.id=="neon-wheels"?CurrentCar.neonWheels:p.id=="star-trail"?CurrentCar.starTrail:p.id=="glass-wheels"?CurrentCar.glassWheels:CurrentCar.glassChassis;
  void EquipStore(SupportProduct p){if(!SupportStore.Owned(p.id))return;bool equipped=StoreEquipped(p);if(p.body>=0){CurrentCar.body=p.body;CurrentCar.supportEdition=equipped?"":p.id;}else if(p.id=="neon-wheels")CurrentCar.neonWheels=!equipped;else if(p.id=="star-trail")CurrentCar.starTrail=!equipped;else if(p.id=="glass-wheels")CurrentCar.glassWheels=!equipped;else if(p.id=="glass-chassis")CurrentCar.glassChassis=!equipped;Changed();Toast(equipped?"Item removed":"Item equipped · stats updated");}
 }
}
