using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Aether {
 // A fictional Philippine coastal city. All coordinates are metres.
 // Static visuals are combined in spatial cells; collision and movable props stay separate.
 public sealed partial class PhilippineCity {
  TrackWorld track;Transform root,architecture,props;System.Random random=new System.Random(20909);
  readonly List<Vector3> occupied=new List<Vector3>();
  readonly Dictionary<string,Material> mats=new Dictionary<string,Material>();
  public static readonly Vector3[] CityShape={
   new Vector3(-370,0,-350),new Vector3(-80,0,-350),new Vector3(220,0,-350),new Vector3(460,0,-240),
   new Vector3(485,0,20),new Vector3(485,0,290),new Vector3(275,0,415),new Vector3(30,0,355),
   new Vector3(-25,0,100),new Vector3(-245,0,45),new Vector3(-445,0,170),new Vector3(-500,0,-100)};
  public static void Build(TrackWorld t){new PhilippineCity{track=t}.Generate();}
  float Rand(float a,float b)=>a+(float)random.NextDouble()*(b-a);
  Material Mat(string name,Color color,float surface=0,float shine=.25f,float emission=0){
   if(mats.TryGetValue(name,out var mat))return mat;mat=ModelLibrary.Material("City "+name,color,emission,0);mat.SetFloat("_Surface",surface);mat.SetFloat("_Smoothness",shine);if(name=="Asphalt"||name=="Concrete"||name.Contains("plaster")){var tex=Resources.Load<Texture2D>(name=="Asphalt"?"Textures/DowntownAsphalt":"Textures/DowntownPlaster");if(tex){tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=8;mat.SetTexture("_MainTex",tex);mat.SetFloat("_TextureMix",.8f);mat.SetFloat("_TextureScale",name=="Asphalt"?.22f:.3f);}}mats[name]=mat;return mat;
  }
  Material concrete=>Mat("Concrete",new Color(.59f,.58f,.52f),3);
  Material asphalt=>Mat("Asphalt",new Color(.20f,.22f,.23f),1,.14f);
  Material white=>Mat("White",new Color(.88f,.87f,.78f));
  Material yellow=>Mat("Road yellow",new Color(.96f,.68f,.12f));
  Material metal=>Mat("Metal",new Color(.34f,.39f,.40f),0,.65f);
  Material dark=>Mat("Dark",new Color(.09f,.115f,.12f));
  Material wood=>Mat("Timber",new Color(.42f,.24f,.105f));
  Material red=>Mat("Red",new Color(.77f,.15f,.07f));
  Material green=>Mat("Green",new Color(.08f,.40f,.23f));
  GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,Material mat,bool collision=false,Vector3 rotation=default){
   var o=CarParts.Part(parent,name,PrimitiveType.Cube,p,size,mat,rotation);
   if(collision){var c=o.AddComponent<BoxCollider>();c.sharedMaterial=CityPhysics.Material(CityPropKind.Metal);}return o;
  }
  GameObject Round(Transform parent,string name,Vector3 p,Vector3 size,Material mat,PrimitiveType type=PrimitiveType.Cylinder,Vector3 rotation=default){return CarParts.Part(parent,name,type,p,size,mat,rotation);}
  Transform Node(Transform parent,string name,Vector3 p,Quaternion q=default){var o=new GameObject(name).transform;o.SetParent(parent,false);o.localPosition=p;o.localRotation=q==default?Quaternion.identity:q;return o;}
  void BodyBox(Transform parent,Vector3 center,Vector3 size){var col=parent.gameObject.AddComponent<BoxCollider>();col.center=center;col.size=size;col.sharedMaterial=CityPhysics.Material(CityPropKind.Metal);}
  void Text(Transform parent,string text,Vector3 pos,float size,Color color,Vector3 rotation=default){
   var label=new GameObject(text).AddComponent<TextMesh>();label.transform.SetParent(parent,false);label.transform.localPosition=pos;label.transform.localRotation=Quaternion.Euler(rotation)*Quaternion.Euler(0,180,0);label.text=text;label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.color=color;label.alignment=TextAlignment.Center;label.gameObject.AddComponent<StructureSign>();
  }
  GameObject Asset(string name,Transform parent,Vector3 pos,float height,float yaw=0){
   // Kenney vehicle meshes were mirrored into Unity with their nose along -Z.
   // Align only their visual root to the controller's +Z; route direction stays physical.
   if(name=="sedan"||name=="taxi"||name=="hatchback-sports"||name=="van"||name=="truck")yaw+=180;
   var o=ModelLibrary.Create("K_"+name,parent);o.transform.localPosition=Vector3.zero;
   Bounds bounds=new Bounds();bool first=true;foreach(var f in o.GetComponentsInChildren<MeshFilter>()){if(first){bounds=f.sharedMesh.bounds;first=false;}else bounds.Encapsulate(f.sharedMesh.bounds);}
   float s=height/Mathf.Max(.02f,bounds.size.y);o.transform.localScale=Vector3.one*s;
   // Center imported models, preserve their authored proportions, and put the base on the ground.
   o.transform.localPosition=pos-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*s;o.transform.localRotation=Quaternion.Euler(0,yaw,0);return o;
  }
  void Generate(){
   root=track.transform;architecture=Node(root,"City static geometry",Vector3.zero);props=Node(root,"Interactive city props",Vector3.zero);
   for(int i=0;i<TrackWorld.Count;i++){
    float t=i/(float)TrackWorld.Count*CityShape.Length;int n=(int)t;float f=t-n;var a=CityShape[(n+11)%12];var b=CityShape[n];var c=CityShape[(n+1)%12];var d=CityShape[(n+2)%12];
    var p=.5f*((2*b)+(-a+c)*f+(2*a-5*b+4*c-d)*f*f+(-a+3*b-3*c+d)*f*f*f);
    // Long, smooth ramps with a level elevated deck. The route itself crosses the flyover.
    p.y=3.1f+12*Mathf.SmoothStep(0,1,Mathf.InverseLerp(29,52,i))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(89,114,i)));track.points[i]=p;
   }
   for(int i=0;i<TrackWorld.Count;i++)track.length+=Vector3.Distance(track.points[i],track.points[(i+1)%TrackWorld.Count]);
   GroundAndSea();Road();Districts();StreetDetails();Beach();Landmarks();BuildCircuitTraffic();
   track.MakeCityRaceFeatures();
   for(int i=14;i<TrackWorld.Count;i+=42)for(int lane=-1;lane<=1;lane++)track.pickups.Add(track.points[i]+track.Right(i)*lane*6+Vector3.up*1.25f);
   for(int x=0;x<16;x++)for(int z=0;z<2;z++){var p=track.points[0]+track.Right(0)*(x*1.5f-11.25f)+track.Forward(0)*z*1.3f+Vector3.up*.07f;Box(architecture,"Finish checker",p,new Vector3(1.5f,.025f,1.3f),(x+z)%2==0?white:dark,false,Quaternion.LookRotation(track.Forward(0)).eulerAngles);}
   Gateway(0,"Downtown Minda", "City circuit");Gateway(210,"Baywalk", "City center");Gateway(352,"Bagong Pag-asa", "Public market");
   Batch();root.gameObject.AddComponent<CityLife>().Initialize(track,props);
   Presentation.Lighting(0);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00065f;RenderSettings.fogColor=new Color(.64f,.75f,.77f);
   var sky=new Material(Shader.Find("Aether/Sky"));sky.SetColor("_Top",new Color(.15f,.43f,.68f));sky.SetColor("_Horizon",new Color(.80f,.86f,.81f));sky.SetFloat("_Night",0);RenderSettings.skybox=sky;
   PremiumLighting.Environment(track);Presentation.Probe(root,track.points[0]+Vector3.up*5);
   Debug.Log("CITY GENERATED length="+track.length.ToString("F0")+"m buildings="+occupied.Count+" props="+props.GetComponentsInChildren<CityProp>().Length+" importedModels=48");
  }
  void GroundAndSea(){
   // Level collision slab under the city; the beach slopes down into the water.
   Box(architecture,"City foundation",new Vector3(-105,1.45f,0),new Vector3(1260,2.9f,1350),Mat("Earth",new Color(.39f,.42f,.33f)),true);
   var sand=Mat("Beach sand",new Color(.76f,.66f,.45f),2);
   TrackWorld.MeshObject("Sloping beach",new[]{new Vector3(525,2.9f,-700),new Vector3(525,2.9f,700),new Vector3(615,-2.5f,-700),new Vector3(615,-2.5f,700)},new[]{0,1,2,2,1,3},sand,true,root);
   Box(architecture,"Seabed",new Vector3(980,-4,0),new Vector3(900,2,1800),sand,true);
   var water=Box(root,"Philippine coastal water",new Vector3(1100,1.25f,0),new Vector3(1150,.1f,2400),track.WaterMaterial());
   // Shallow drainage channels are visibly wet without covering the main streets.
   for(int i=0;i<10;i++)Box(architecture,"Roadside drainage",new Vector3(-400+i*80,2.96f,-285),new Vector3(1.5f,.04f,80),Mat("Drain water",new Color(.09f,.22f,.19f),0,.85f));
  }
  void Road(){
   var vs=new List<Vector3>();var ts=new List<int>();
   for(int i=0;i<=TrackWorld.Count;i++){int n=i%TrackWorld.Count;vs.Add(track.points[n]-track.Right(n)*12);vs.Add(track.points[n]+track.Right(n)*12);if(i<TrackWorld.Count&&!track.Broken(i)){int k=i*2;ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
   TrackWorld.MeshObject("City race road",vs.ToArray(),ts.ToArray(),asphalt,true,root);
   for(int i=0;i<TrackWorld.Count;i++){
    int j=(i+1)%TrackWorld.Count;var p=track.points[i];var f=track.Forward(i);var r=track.Right(i);float len=Vector3.Distance(p,track.points[j])+.15f;var angle=Quaternion.LookRotation(track.points[j]-p).eulerAngles;
    if(track.Broken(i))TrackWorld.MeshObject("Dry coastal lane",new[]{p+r*3.5f,p+r*12,track.points[j]+track.Right(j)*3.5f,track.points[j]+track.Right(j)*12},new[]{0,2,1,1,2,3},asphalt,true,root);
    bool elevated=p.y>4;
    for(int s=-1;s<=1;s+=2){
     var q=(p+track.points[j])*.5f+r*s*14.9f;
     Box(architecture,elevated?"Flyover walkway":"Concrete sidewalk",q-Vector3.up*.03f,new Vector3(5.8f,.30f,len),concrete,true,angle);
     // Low curbs can be driven over into the stalls. Elevated edges have proper barriers.
     Box(architecture,"Painted curb",q-r*s*2.7f+Vector3.up*.06f,new Vector3(.35f,.28f,len),i%5<3?white:yellow,true,angle);
     if(elevated){Box(architecture,"Flyover parapet",q+r*s*2.65f+Vector3.up*.6f,new Vector3(.5f,1.3f,len),concrete,true,angle);Box(architecture,"Flyover handrail",q+r*s*2.65f+Vector3.up*1.35f,new Vector3(.1f,.1f,len),metal,false,angle);}
     if(!track.Broken(i)&&i%3==0)Box(architecture,"Edge line",p+r*s*11.2f+Vector3.up*.025f,new Vector3(.16f,.018f,len*.8f),white,false,angle);
    }
    if(i%2==0&&!track.Broken(i))for(int s=-1;s<=1;s+=2)Box(architecture,"Lane dash",p+r*s*.16f+Vector3.up*.03f,new Vector3(.12f,.018f,3),yellow,false,angle);
    if(elevated){Box(architecture,"Flyover underside",(p+track.points[j])*.5f-Vector3.up*.65f,new Vector3(30,1.05f,len),concrete,true,angle);if(i%7==0){float h=p.y-3.6f;for(int s=-1;s<=1;s+=2){Box(architecture,"Flyover pier",new Vector3(p.x,3+h*.5f,p.z)+r*s*8,new Vector3(2.2f,h,2.5f),concrete,true);Box(architecture,"Pier base",new Vector3(p.x,3,p.z)+r*s*8,new Vector3(4,.3f,4),concrete,true);}Box(architecture,"Flyover crossbeam",p-Vector3.up*1.3f,new Vector3(29,1.1f,3),concrete,true,angle);}}
    if(i%38==12&&!elevated){for(int k=-5;k<=5;k++)Box(architecture,"Zebra crossing",p+r*k*1.9f+Vector3.up*.05f,new Vector3(.8f,.025f,5),white,false,angle);}
   }
   // Connected secondary street grid, including a usable road beneath the flyover.
   foreach(float z in new[]{-350f,-190f,-30f,190f,350f}){
    Box(architecture,"City cross street",new Vector3(-80,2.98f,z),new Vector3(1050,.12f,17),asphalt,true);
    for(int x=-580;x<430;x+=18)Box(architecture,"Cross street dash",new Vector3(x,3.055f,z),new Vector3(5,.018f,.15f),yellow);
   }
   foreach(float x in new[]{-390f,-230f,80f,275f}){
    Box(architecture,"City side street",new Vector3(x,2.98f,10),new Vector3(17,.12f,1100),asphalt,true);
    for(int z=-510;z<550;z+=18)Box(architecture,"Side street dash",new Vector3(x,3.055f,z),new Vector3(.15f,.018f,5),white);
   }
  }
  bool Clear(Vector3 p,float radius){
   foreach(var landmark in new[]{new Vector3(-315,0,254),new Vector3(172,0,254),new Vector3(-118,0,242)})if(TrackWorld.FlatDistance(p,landmark)<radius+33)return false;
   int n=track.Nearest(p);if(TrackWorld.FlatDistance(track.points[n],p)<radius+24)return false;
   foreach(float z in new[]{-350f,-190f,-30f,190f,350f})if(Mathf.Abs(p.z-z)<radius+11)return false;
   foreach(float x in new[]{-390f,-230f,80f,275f})if(Mathf.Abs(p.x-x)<radius+11)return false;
   foreach(var q in occupied)if(TrackWorld.FlatDistance(p,q)<radius+19)return false;return true;
  }
  void Districts(){
   for(int z=-540;z<=540;z+=57)for(int x=-615;x<=405;x+=58){
    var p=new Vector3(x+Rand(-4,4),3,z+Rand(-4,4));if(!Clear(p,17))continue;
    occupied.Add(p);bool tower=x>20&&z<160;float h=tower?Rand(38,78):Rand(12,27);
    var building=Node(architecture,tower?"City office tower":"Apartment block",p,Quaternion.Euler(0,random.Next(4)*90,0));
    Asset(tower?"building-skyscraper-"+(char)('a'+random.Next(5)):"building-"+(char)('a'+random.Next(8)),building,Vector3.zero,h);
    // Match static collision to the actual imported building dimensions.
    Bounds b=new Bounds();bool first=true;foreach(var f in building.GetComponentsInChildren<MeshFilter>()){var bb=f.sharedMesh.bounds;var tr=building.worldToLocalMatrix*f.transform.localToWorldMatrix;var min=tr.MultiplyPoint3x4(bb.min);var max=tr.MultiplyPoint3x4(bb.max);if(first){b=new Bounds((min+max)*.5f,Vector3.zero);first=false;}b.Encapsulate(min);b.Encapsulate(max);}BodyBox(building,b.center,b.size);
    Box(building,"Foundation apron",new Vector3(0,.05f,0),new Vector3(b.size.x+3,.2f,b.size.z+3),concrete,true);
    Round(building,"Blue rooftop water tank",new Vector3(0,h+.85f,0),new Vector3(2.4f,.9f,2.4f),Mat("Water tank blue",new Color(.045f,.33f,.65f)));
   }
   // Streetside mixed-use shop houses follow the circuit and frame the driver's view.
   for(int n=0;n<TrackWorld.Count;n+=6){if(track.points[n].y>4||n>=132&&n<206)continue;for(int s=-1;s<=1;s+=2){
    var p=track.points[n]+track.Right(n)*s*31;p.y=3;bool overlap=false;foreach(var q in occupied)if(TrackWorld.FlatDistance(p,q)<21){overlap=true;break;}if(overlap)continue;
    occupied.Add(p);Shop(p,Quaternion.LookRotation(track.Right(n)*s),n+s+1);
   }}
  }
  void Shop(Vector3 p,Quaternion rotation,int seed){
   string[] names={"Aling Nena's Sari-Sari","Panaderia Luntian","Botika ng Bayan","Kape at Pandesal","Lutong Bahay","Vulcanizing Shop","Tubig Refill","Bigasan ni Mang Jun","Laba Express","Gupitan","Tindahan ni Ate","Palengke Fresh"};
   var o=Node(architecture,"Shop house "+names[seed%names.Length],p,rotation);float h=8+(seed%3)*3.2f;float w=13+(seed%3)*2;var paint=Mat("Shop plaster "+seed%6,new[]{new Color(.74f,.62f,.40f),new Color(.27f,.57f,.52f),new Color(.73f,.41f,.32f),new Color(.66f,.68f,.57f),new Color(.40f,.54f,.62f),new Color(.79f,.72f,.53f)}[seed%6]);
   Box(o,"Painted concrete building",new Vector3(0,h*.5f,0),new Vector3(w,h,13),paint,true);
   Box(o,"Roof overhang",new Vector3(0,h+.2f,0),new Vector3(w+1.4f,.35f,14.5f),metal);
   for(int j=0;j<9;j++)Box(o,"Corrugated roof rib",new Vector3(-w*.5f+j*w/8,h+.4f,0),new Vector3(.1f,.12f,14.5f),seed%2==0?red:metal);
   for(int level=0;level<(h-4)/3;level++)for(int j=-1;j<=1;j++){
    Box(o,"Window frame",new Vector3(j*w*.27f,5.5f+level*3,-6.56f),new Vector3(2.4f,1.8f,.14f),white);
    Box(o,"Tinted window",new Vector3(j*w*.27f,5.5f+level*3,-6.66f),new Vector3(2.08f,1.51f,.08f),Mat("Shop glass",new Color(.07f,.23f,.27f),0,.8f));
    Box(o,"Window divider",new Vector3(j*w*.27f,5.5f+level*3,-6.73f),new Vector3(.08f,1.5f,.08f),metal);
   }
   Box(o,"Shop shutter",new Vector3(-w*.19f,1.9f,-6.56f),new Vector3(w*.49f,3.8f,.13f),metal);
   for(int k=0;k<13;k++)Box(o,"Shutter slat",new Vector3(-w*.19f,.24f+k*.27f,-6.66f),new Vector3(w*.49f,.045f,.05f),dark);
   Box(o,"Shop entrance",new Vector3(w*.30f,1.6f,-6.61f),new Vector3(2.5f,3.2f,.18f),dark);
   Box(o,"Shop sign",new Vector3(0,4,-6.86f),new Vector3(w-.4f,1.2f,.22f),seed%2==0?green:red);
   Text(o,names[seed%names.Length],new Vector3(0,4,-7.0f),.20f,Color.white,new Vector3(0,180,0));
   Box(o,"Striped awning",new Vector3(0,3.27f,-8),new Vector3(w,.18f,3),seed%2==0?red:green,false,new Vector3(-8,0,0));
   for(int j=0;j<9;j++)Box(o,"Awning stripe",new Vector3(-w*.46f+j*w*.115f,3.38f,-8),new Vector3(.65f,.025f,3),white,false,new Vector3(-8,0,0));
   Box(o,"Air conditioner",new Vector3(w*.35f,5.1f,-7),new Vector3(1.25f,.7f,.6f),white);Round(o,"AC fan",new Vector3(w*.35f,5.1f,-7.33f),new Vector3(.5f,.045f,.5f),metal,PrimitiveType.Cylinder,new Vector3(90,0,0));
   for(int s=-1;s<=1;s+=2)Box(o,"Downpipe",new Vector3(s*(w*.5f-.25f),h*.5f,-6.72f),new Vector3(.13f,h,.13f),metal);
   Round(o,"Rooftop water storage",new Vector3(-3,h+1,2),new Vector3(2.2f,1,2.2f),Mat("Water tank blue",new Color(.045f,.33f,.65f)));
   if(seed%3==0){for(int j=0;j<5;j++)Box(o,"Drying laundry",new Vector3(-3+j*1.3f,h-1,-7.2f),new Vector3(.85f,.9f,.045f),Mat("Laundry "+j,new[]{Color.white,new Color(.86f,.35f,.23f),new Color(.20f,.45f,.7f),new Color(.9f,.68f,.2f),new Color(.3f,.6f,.4f)}[j]));Cable(o,new Vector3(-4,h-.4f,-7.2f),new Vector3(4,h-.4f,-7.2f),.13f,.025f);}
  }
  void StreetDetails(){
   for(int n=0;n<TrackWorld.Count;n+=8){
    var p=track.points[n];bool elevated=p.y>4;var f=track.Forward(n);var r=track.Right(n);
    for(int s=-1;s<=1;s+=2){
     var q=p+r*s*(elevated?16.5f:19.8f);q.y=elevated?p.y:3;
     StreetLamp(q,Quaternion.LookRotation(r*s),n);
     if(!elevated){if(n%16==0)Palm(p+r*s*22,8+n%3,true);if(n%24==0)UtilityPole(p+r*s*24,track.points[(n+8)%TrackWorld.Count]+track.Right((n+8)%TrackWorld.Count)*s*24);}
    }
    if(!elevated&&!(n>=136&&n<=200)){
     int s=n%16==0?1:-1;var pos=p+r*s*16.5f;pos.y=3.23f;
     if(n%16==0)Stall(pos,Quaternion.LookRotation(r*s),n/8);
     else {LooseProp("K_barrel",p+r*s*16+Vector3.up*.2f,1.15f,25,CityPropKind.Metal);Crate(p-r*s*16+Vector3.up*.22f,Quaternion.LookRotation(f));}
     if(n%24==8){var q=p-r*s*19;q.y=3.22f;Jeepney(q,Quaternion.LookRotation(f),n,false);}
     if(n%32==0){var q=p+r*s*19;q.y=3.22f;Tricycle(q,Quaternion.LookRotation(f));}
    }
    if(n%24==0&&!elevated)Bunting(n);
   }
   // Additional public-market rows sit immediately beside the return leg.
   for(int n=345;n<391;n+=5){int s=1;var p=track.points[n]+track.Right(n)*s*17;p.y=3.23f;Stall(p,Quaternion.LookRotation(track.Right(n)*s),n);}
   // Background traffic travels on cross streets, always outside the race corridor.
   for(int i=0;i<12;i++){
    float z=new[]{-190f,-30f,190f}[i%3];float x=-560+i*69;var p=new Vector3(x,3.07f,z+(i%2==0?-4:4));
    if(TrackWorld.FlatDistance(p,track.points[track.Nearest(p)])<36)continue;
    var o=Node(props,"Neighborhood traffic",p,Quaternion.Euler(0,i%2==0?90:270,0));var model=Asset(i%3==0?"taxi":i%3==1?"van":"sedan",o,Vector3.zero,1.6f);BodyBox(o,new Vector3(0,.85f,0),new Vector3(1.8f,1.4f,4));
    var prop=o.gameObject.AddComponent<CityProp>();prop.Configure(CityPropKind.Metal,240,false,16);var traffic=o.gameObject.AddComponent<CityTraffic>();traffic.track=track;traffic.start=p;traffic.direction=o.forward;
   }
  }
  void StreetLamp(Vector3 p,Quaternion q,int seed){
   var o=Node(props,"Street light",p,q);Round(o,"Lamp pole",new Vector3(0,4.5f,0),new Vector3(.20f,4.5f,.20f),metal);BodyBox(o,new Vector3(0,4,0),new Vector3(.22f,8,.22f));Box(o,"Lamp arm",new Vector3(0,8.9f,-1.1f),new Vector3(.14f,.14f,2.4f),metal);Box(o,"LED streetlight",new Vector3(0,8.83f,-2.2f),new Vector3(.7f,.13f,1.1f),Mat("Lamp",new Color(1,.91f,.66f),0,.4f,.45f));
   o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Metal,100,true,8);o.gameObject.AddComponent<RespawningStreetLamp>();PremiumLighting.Streetlight(o,track.map);
   if(track.map==0&&seed%24==0){Box(o,"Street name plate",new Vector3(0,5,0),new Vector3(3.8f,.7f,.14f),green);Text(o,seed<120?"Rizal Avenue":seed<230?"Baywalk Road":seed<330?"Mabini Street":"Market Street",new Vector3(0,5,-.08f),.13f,Color.white,new Vector3(0,180,0));}
  }
  void UtilityPole(Vector3 p,Vector3 next){
   p.y=3;next.y=3;var o=Node(architecture,"Utility pole and service wires",p);Round(o,"Concrete pole",new Vector3(0,6,0),new Vector3(.34f,6,.34f),concrete);BodyBox(o,new Vector3(0,5,0),new Vector3(.38f,10,.38f));Box(o,"Crossarm",new Vector3(0,11,0),new Vector3(3,.16f,.16f),wood);
   for(int i=-1;i<=1;i++){Round(o,"Insulator",new Vector3(i*1.15f,11.3f,0),new Vector3(.20f,.22f,.2f),white);Cable(architecture,p+new Vector3(i*1.15f,11.5f,0),next+new Vector3(i*1.15f,11.5f,0),1.1f,.035f);}
   Round(o,"Transformer",new Vector3(.5f,8,0),new Vector3(.8f,.8f,.8f),metal);
  }
  void Cable(Transform parent,Vector3 a,Vector3 b,float sag,float width){var o=new GameObject("Overhead cable");o.transform.SetParent(parent,false);var line=o.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=9;line.widthMultiplier=width;line.sharedMaterial=dark;for(int j=0;j<9;j++){float t=j/8f;line.SetPosition(j,Vector3.Lerp(a,b,t)-Vector3.up*Mathf.Sin(t*Mathf.PI)*sag);}line.shadowCastingMode=ShadowCastingMode.Off;}
  void Bunting(int n){var p=track.points[n];var r=track.Right(n);Cable(architecture,p-r*22+Vector3.up*9,p+r*22+Vector3.up*9,2,.025f);for(int j=0;j<20;j++){
    float t=(j+.5f)/20;var pos=p+r*Mathf.Lerp(-22,22,t)+Vector3.up*(9-Mathf.Sin(t*Mathf.PI)*2);var q=Quaternion.LookRotation(track.Forward(n));var a=pos-r*.5f;var b=pos+r*.5f;var c=pos-Vector3.up*.85f;
    TrackWorld.MeshObject("Fiesta pennant",new[]{a,b,c},new[]{0,1,2,2,1,0},j%3==0?red:j%3==1?yellow:Mat("Flag blue",new Color(.06f,.31f,.65f)),false,architecture);
   }}
  void Palm(Vector3 p,float height,bool movable){
   p.y=3.18f;var o=Node(movable?props:architecture,"Coconut palm",p);Asset("tree_palmDetailedTall",o,Vector3.zero,height,Rand(0,360));
   foreach(var renderer in o.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=renderer.sharedMaterial.color.g>.65f?Mat("Palm fronds",new Color(.23f,.48f,.16f)):Mat("Palm trunk",new Color(.44f,.32f,.20f));
   var trunk=o.gameObject.AddComponent<CapsuleCollider>();trunk.center=new Vector3(0,height*.36f,0);trunk.height=height*.72f;trunk.radius=.26f;
   if(movable){var crown=o.gameObject.AddComponent<SphereCollider>();crown.center=Vector3.up*height*.85f;crown.radius=height*.18f;var prop=o.gameObject.AddComponent<CityProp>();prop.Configure(CityPropKind.Tree,180,true,11);}
  }
  GameObject LooseProp(string name,Vector3 p,float height,float mass,CityPropKind type){
   var o=Node(props,name.Substring(2),p);var visual=Asset(name.Substring(2),o,Vector3.zero,height,Rand(0,360));
   if(type==CityPropKind.Loose){var c=o.gameObject.AddComponent<SphereCollider>();c.center=Vector3.up*height*.5f;c.radius=height*.43f;}
   else BodyBox(o,Vector3.up*height*.5f,new Vector3(height*.7f,height,height*.7f));
   var prop=o.gameObject.AddComponent<CityProp>();prop.Configure(type,mass,false,20);return o.gameObject;
  }
  Transform Chunk(Transform parent,string name,Vector3 pos){var o=Node(parent,name,pos);o.gameObject.AddComponent<CityChunk>();return o;}
  void Crate(Vector3 p,Quaternion rotation){
   var o=Node(props,"Breakable wooden produce crate",p,rotation);
   for(int s=-1;s<=1;s+=2){var c=Chunk(o,"Crate wall",new Vector3(s*.64f,.42f,0));for(int k=0;k<3;k++)Box(c,"Timber slat",new Vector3(0,-.28f+k*.28f,0),new Vector3(.1f,.20f,1.2f),wood);BodyBox(c,Vector3.zero,new Vector3(.1f,.8f,1.2f));}
   for(int s=-1;s<=1;s+=2){var c=Chunk(o,"Crate end",new Vector3(0,.42f,s*.58f));Box(c,"Timber panel",Vector3.zero,new Vector3(1.3f,.8f,.1f),wood);BodyBox(c,Vector3.zero,new Vector3(1.3f,.8f,.1f));}
   var b=Chunk(o,"Crate base",new Vector3(0,.08f,0));Box(b,"Base",Vector3.zero,new Vector3(1.3f,.12f,1.2f),wood);BodyBox(b,Vector3.zero,new Vector3(1.3f,.12f,1.2f));o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Timber,18,false,8);
  }
  void Stall(Vector3 p,Quaternion rotation,int seed){
   bool fruit=seed%3==0;string title=fruit?"Prutas • Saging at Mangga":seed%3==1?"Ihaw-Ihaw • BBQ":"Tusok-Tusok • Fishball";
   var o=Node(props,fruit?"Fruit stand":"Street food cart",p,rotation);var paint=seed%3==0?green:seed%3==1?red:Mat("Cart blue",new Color(.07f,.36f,.56f));
   var basePart=Chunk(o,"Cart chassis",Vector3.zero);
   Box(basePart,"Cart body",new Vector3(0,.75f,0),new Vector3(3,1.05f,1.6f),paint);Box(basePart,"Counter",new Vector3(0,1.37f,0),new Vector3(3.35f,.14f,1.85f),fruit?wood:metal);BodyBox(basePart,new Vector3(0,.85f,0),new Vector3(3.35f,1.2f,1.85f));
   for(int s=-1;s<=1;s+=2)for(int j=-1;j<=1;j+=2)Round(basePart,"Cart wheel",new Vector3(s*1.4f,.26f,j*.58f),new Vector3(.48f,.10f,.48f),dark,PrimitiveType.Cylinder,new Vector3(0,0,90));
   var roof=Chunk(o,"Awning and posts",new Vector3(0,2.65f,0));
   for(int s=-1;s<=1;s+=2)for(int j=-1;j<=1;j+=2)Box(roof,"Canopy post",new Vector3(s*1.4f,-.65f,j*.72f),new Vector3(.075f,1.5f,.075f),metal);
   Box(roof,"Canopy",new Vector3(0,.13f,0),new Vector3(3.65f,.16f,2.5f),paint,false,new Vector3(-7,0,0));
   for(int j=0;j<6;j++)Box(roof,"Awning white stripe",new Vector3(-1.5f+j*.6f,.23f,0),new Vector3(.25f,.035f,2.5f),white,false,new Vector3(-7,0,0));BodyBox(roof,new Vector3(0,.15f,0),new Vector3(3.65f,.18f,2.5f));
   var board=Chunk(o,"Menu board",new Vector3(0,2.45f,-1.24f));Box(board,"Printed sign",Vector3.zero,new Vector3(3.55f,.48f,.10f),white);Text(board,title,Vector3.back*.06f,.065f,new Color(.07f,.20f,.13f),new Vector3(0,180,0));BodyBox(board,Vector3.zero,new Vector3(3.55f,.48f,.1f));
   o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Cart,100,false,10);
   for(int i=0;i<8;i++){
    string model=fruit?new[]{"banana","orange","apple","pineapple","watermelon","coconut","banana","orange"}[i]:new[]{"skewer","corn","fish","pot","soda-bottle","bowl","skewer","plate"}[i];
    var pos=o.TransformPoint(new Vector3(-1.12f+i%4*.73f,1.48f,-.42f+i/4*.72f));LooseProp("K_"+model,pos,model=="pot"?.42f:model=="pineapple"?.48f:.30f,model=="pot"?3:.5f,CityPropKind.Loose);
   }
   if(!fruit){var smoke=RaceEffects.Emitter(o,"Grill smoke",new Color(.62f,.65f,.64f,.20f),2,.3f,.6f,22);smoke.transform.localPosition=new Vector3(.7f,1.55f,0);smoke.transform.localRotation=Quaternion.Euler(-90,0,0);var em=smoke.emission;em.rateOverTime=4;}
   if(seed%2==0){Crate(o.TransformPoint(new Vector3(2.6f,0,.2f)),rotation);var stool=Node(props,"Plastic stool",o.TransformPoint(new Vector3(-2.4f,0,-.2f)));Box(stool,"Seat",new Vector3(0,.63f,0),new Vector3(.7f,.14f,.7f),paint);for(int s=-1;s<=1;s+=2)for(int j=-1;j<=1;j+=2)Box(stool,"Leg",new Vector3(s*.27f,.31f,j*.27f),new Vector3(.09f,.62f,.09f),paint);BodyBox(stool,new Vector3(0,.36f,0),new Vector3(.7f,.72f,.7f));stool.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Loose,3);}
  }
  Transform Jeepney(Vector3 p,Quaternion rotation,int seed,bool traffic){
   var o=Node(props,"Philippine jeepney",p,rotation);var paint=seed%2==0?yellow:Mat("Jeepney blue",new Color(.045f,.35f,.70f));
   Box(o,"Chrome chassis",new Vector3(0,.62f,0),new Vector3(2.15f,.35f,5.6f),metal);Box(o,"Passenger body",new Vector3(0,1.13f,-.6f),new Vector3(2.2f,.75f,4),paint);
   Box(o,"Long hood",new Vector3(0,1.1f,1.85f),new Vector3(1.8f,.65f,1.5f),paint);Box(o,"Roof",new Vector3(0,2.34f,-.4f),new Vector3(2.35f,.18f,4.7f),metal);
   Box(o,"Windshield",new Vector3(0,1.88f,1.65f),new Vector3(1.92f,.67f,.07f),Mat("Shop glass",new Color(.07f,.23f,.27f),0,.8f));
   for(int s=-1;s<=1;s+=2){for(int j=0;j<5;j++){Box(o,"Window pillar",new Vector3(s*1.04f,1.86f,1.35f-j*.8f),new Vector3(.07f,.82f,.07f),metal);Box(o,"Window rail",new Vector3(s*1.1f,1.52f,1-j*.8f),new Vector3(.07f,.07f,.7f),metal);}for(float z=-1.7f;z<=1.7f;z+=3.4f)Round(o,"Jeepney tire",new Vector3(s*1.10f,.48f,z),new Vector3(.95f,.17f,.95f),dark,PrimitiveType.Cylinder,new Vector3(0,0,90));Round(o,"Round headlamp",new Vector3(s*.68f,1.10f,2.64f),new Vector3(.3f,.045f,.3f),white,PrimitiveType.Cylinder,new Vector3(90,0,0));}
   Box(o,"Chrome grille",new Vector3(0,.99f,2.65f),new Vector3(1.2f,.59f,.07f),metal);for(int j=-4;j<=4;j++)Box(o,"Grille slit",new Vector3(j*.12f,.99f,2.70f),new Vector3(.055f,.47f,.03f),dark);
   Box(o,"Front bumper",new Vector3(0,.5f,2.83f),new Vector3(2.4f,.18f,.22f),metal);Box(o,"Route sign",new Vector3(0,2.09f,1.72f),new Vector3(1.8f,.24f,.06f),yellow);Text(o,"BAYAN - PALENGKE",new Vector3(0,2.09f,1.76f),.041f,Color.black);
   Text(o,"LUNTIAN",new Vector3(0,.77f,2.73f),.045f,Color.black);
   BodyBox(o,new Vector3(0,1.18f,-.3f),new Vector3(2.25f,1.85f,5.3f));o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Metal,380,false,18);return o;
  }
  Transform Tricycle(Vector3 p,Quaternion rotation){
   var o=Node(props,"Motorized tricycle",p,rotation);Box(o,"Motorcycle body",new Vector3(-.55f,.6f,0),new Vector3(.43f,.62f,1.8f),red);Box(o,"Motorcycle seat",new Vector3(-.55f,.98f,-.2f),new Vector3(.5f,.13f,.85f),dark);
   for(int s=-1;s<=1;s+=2)Round(o,"Motorcycle wheel",new Vector3(-.55f,.35f,s*.88f),new Vector3(.69f,.10f,.69f),dark,PrimitiveType.Cylinder,new Vector3(0,0,90));
   Box(o,"Sidecar",new Vector3(.48f,.62f,-.18f),new Vector3(1.05f,.75f,1.8f),metal);Box(o,"Sidecar roof",new Vector3(.48f,1.70f,-.18f),new Vector3(1.2f,.13f,2),red);for(int s=-1;s<=1;s+=2)Box(o,"Sidecar pillar",new Vector3(.48f+s*.46f,1.28f,.56f),new Vector3(.065f,.8f,.065f),metal);
   Round(o,"Sidecar wheel",new Vector3(1.05f,.35f,-.45f),new Vector3(.65f,.1f,.65f),dark,PrimitiveType.Cylinder,new Vector3(0,0,90));BodyBox(o,new Vector3(.2f,.85f,0),new Vector3(2,1.6f,2));o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Metal,120,false,14);return o;
  }
  void Beach(){
   for(int n=130;n<208;n+=4){var p=track.points[n];var r=track.Right(n);var f=track.Forward(n);
    // The coastline lies on the outside (east / left) of this northbound road.
    var q=p-r*22;q.y=3.1f;Palm(q,8+n%5,true);
    var seawall=p-r*30;seawall.y=2.8f;Box(architecture,"Baywalk sea wall",seawall,new Vector3(1.2f,1.1f,Vector3.Distance(p,track.points[(n+4)%TrackWorld.Count])+.4f),concrete,true,Quaternion.LookRotation(f).eulerAngles);
    if(n%8==2){var rock=Node(props,"Breakable coastal rock",p-r*17+Vector3.up*.1f);Asset("rock_largeA",rock,Vector3.zero,1.3f);BodyBox(rock,new Vector3(0,.55f,0),new Vector3(1.4f,1.1f,1.4f));rock.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Rock,260,true,15);}
   }
   foreach(int start in track.WaterStarts){
    for(int n=start;n<start+8;n++){var p=track.points[n];var r=track.Right(n);var f=track.Forward(n);
     Box(architecture,"Sunken road foundation",p-r*4-Vector3.up*.55f,new Vector3(15,1,Vector3.Distance(p,track.points[n+1])+.3f),concrete,true,Quaternion.LookRotation(f).eulerAngles);
     for(int j=0;j<2;j++){
      var q=p-r*(j==0?9.5f:1.2f)+Vector3.up*.13f;var slab=Node(props,"Loose washout asphalt slab",q,Quaternion.LookRotation(f)*Quaternion.Euler(4,j*31,5));Box(slab,"Broken asphalt",Vector3.zero,new Vector3(1.8f,.22f,2.4f),asphalt);BodyBox(slab,Vector3.zero,new Vector3(1.8f,.22f,2.4f));slab.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Metal,35);
     }
    }
    Gateway(start-8,"Road damage", "Keep right for dry lane");
    for(int j=0;j<5;j++){int n=start-2;var p=track.points[n]-track.Right(n)*(1+j*2)+Vector3.up*.08f;Cone(p);}
   }
   for(int i=0;i<16;i++){
    var p=new Vector3(560+Rand(-5,20),1.2f,-340+i*48);var o=Node(architecture,"Beach shelter",p);for(int s=-1;s<=1;s+=2)for(int j=-1;j<=1;j+=2)Round(o,"Bamboo post",new Vector3(s*2,1.5f,j*1.5f),new Vector3(.15f,1.5f,.15f),wood);
    Box(o,"Nipa roof",new Vector3(0,3.2f,0),new Vector3(5,.35f,4),Mat("Nipa",new Color(.55f,.39f,.17f)),false,new Vector3(0,0,7));
    for(int j=0;j<12;j++)Box(o,"Thatch bundle",new Vector3(-2.4f+j*.44f,3.39f,0),new Vector3(.18f,.13f,4),wood,false,new Vector3(0,0,7));
   }
   for(int i=0;i<5;i++){var boat=Node(root,"Outrigger fishing boat",new Vector3(637+i*30,1.7f,-270+i*132),Quaternion.Euler(0,i*27,0));Round(boat,"Painted hull",Vector3.zero,new Vector3(2.4f,.6f,9),i%2==0?white:Mat("Boat blue",new Color(.06f,.30f,.58f)),PrimitiveType.Sphere);for(int s=-1;s<=1;s+=2){Round(boat,"Bamboo outrigger",new Vector3(s*3.6f,-.15f,0),new Vector3(.24f,3.7f,.24f),wood,PrimitiveType.Cylinder,new Vector3(90,0,0));for(int j=-1;j<=1;j+=2)Box(boat,"Outrigger beam",new Vector3(s*1.8f,.15f,j*2.3f),new Vector3(4,.12f,.12f),wood);}boat.gameObject.AddComponent<CityBoatBob>();}
  }
  void Cone(Vector3 p){var o=Node(props,"Movable traffic cone",p);Box(o,"Rubber base",new Vector3(0,.06f,0),new Vector3(.65f,.12f,.65f),dark);Round(o,"Cone body",new Vector3(0,.39f,0),new Vector3(.30f,.33f,.30f),Mat("Cone orange",new Color(1,.3f,.035f)));Round(o,"Reflective band",new Vector3(0,.47f,0),new Vector3(.32f,.055f,.32f),white);BodyBox(o,new Vector3(0,.36f,0),new Vector3(.55f,.72f,.55f));o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Loose,2.8f);}
  void Gateway(int n,string title,string subtitle){var o=Node(architecture,title+" road gantry",track.points[n],Quaternion.LookRotation(track.Forward(n)));for(int s=-1;s<=1;s+=2)Box(o,"Gantry support",new Vector3(s*15,4.7f,0),new Vector3(.45f,9.4f,.45f),metal,true);Box(o,"Gantry beam",new Vector3(0,9.2f,0),new Vector3(31,.45f,.45f),metal);Box(o,"Direction board",new Vector3(0,8.5f,-.2f),new Vector3(15,2.8f,.2f),green);Text(o,title,new Vector3(0,9,-.32f),.45f,Color.white,new Vector3(0,180,0));Text(o,subtitle,new Vector3(0,7.95f,-.32f),.25f,Color.white,new Vector3(0,180,0));}
  void Landmarks(){
   var church=Node(architecture,"Barangay church",new Vector3(-315,3,254),Quaternion.Euler(0,180,0));Box(church,"Church nave",new Vector3(0,8,0),new Vector3(24,16,35),Mat("Church plaster",new Color(.79f,.73f,.59f)),true);
   for(int s=-1;s<=1;s+=2){Box(church,"Sloped tile roof",new Vector3(s*6,17.7f,0),new Vector3(14,.45f,37),red,false,new Vector3(0,0,-s*23));Round(church,"Church column",new Vector3(s*9,8,-18),new Vector3(1.3f,8,1.3f),white);}
   Box(church,"Church doorway",new Vector3(0,4,-17.6f),new Vector3(5,8,.2f),wood);Round(church,"Rose window",new Vector3(0,12,-17.7f),new Vector3(4,.1f,4),Mat("Church window",new Color(.23f,.40f,.50f)),PrimitiveType.Cylinder,new Vector3(90,0,0));
   Box(church,"Bell tower",new Vector3(17,13,0),new Vector3(8,26,8),white,true);for(int s=-1;s<=1;s+=2)Box(church,"Bell opening",new Vector3(17+s*4.01f,22,0),new Vector3(.1f,4,3),dark);
   Box(church,"Church cross",new Vector3(17,29,0),new Vector3(.45f,5,.45f),metal);Box(church,"Cross arm",new Vector3(17,30,0),new Vector3(2.6f,.4f,.4f),metal);
   var court=Node(architecture,"Barangay covered basketball court",new Vector3(172,3,254));Box(court,"Painted basketball court",Vector3.zero,new Vector3(28,.15f,17),Mat("Court green",new Color(.15f,.43f,.30f)),true);Box(court,"Court center stripe",Vector3.up*.09f,new Vector3(.12f,.02f,17),white);
   for(int s=-1;s<=1;s+=2){Box(court,"Backboard pole",new Vector3(s*12.5f,2.2f,0),new Vector3(.18f,4.4f,.18f),metal,true);Box(court,"Backboard",new Vector3(s*12.3f,4.4f,0),new Vector3(.12f,1.2f,1.9f),white);Box(court,"Court key",new Vector3(s*9,.10f,0),new Vector3(6,.025f,5),red);for(int j=-1;j<=1;j+=2)Box(court,"Covered court pillar",new Vector3(s*14,5.5f,j*9),new Vector3(.4f,11,.4f),metal,true);}
   Box(court,"Covered court roof",new Vector3(0,11.1f,0),new Vector3(31,.25f,21),Mat("Court roof",new Color(.13f,.33f,.45f)),false,new Vector3(0,0,3));
   var hall=Node(architecture,"Barangay hall",new Vector3(-118,3,242));Box(hall,"Barangay building",new Vector3(0,5,0),new Vector3(21,10,16),white,true);Box(hall,"Barangay roof",new Vector3(0,10.2f,0),new Vector3(23,.4f,18),green);Text(hall,"Barangay Luntian",new Vector3(0,7,-8.1f),.40f,new Color(.06f,.22f,.18f),new Vector3(0,180,0));
   Box(hall,"Philippine flagpole",new Vector3(14,6,-10),new Vector3(.1f,12,.1f),metal);Box(hall,"Flag blue",new Vector3(15.5f,11.2f,-10),new Vector3(3,.8f,.025f),Mat("Flag blue",new Color(.06f,.31f,.65f)));Box(hall,"Flag red",new Vector3(15.5f,10.4f,-10),new Vector3(3,.8f,.025f),red);
   TrackWorld.MeshObject("Flag white triangle",new[]{new Vector3(14,11.6f,-10.025f),new Vector3(14,10,-10.025f),new Vector3(15.4f,10.8f,-10.025f)},new[]{0,1,2,2,1,0},white,false,hall);
   // Small sun disc sits in the white hoist triangle.
   Round(hall,"Flag sun",new Vector3(14.45f,10.8f,-10.05f),new Vector3(.26f,.015f,.26f),yellow,PrimitiveType.Cylinder,new Vector3(90,0,0));
   for(int i=0;i<18;i++){var p=track.points[(i*23+7)%TrackWorld.Count]+track.Right((i*23+7)%TrackWorld.Count)*25;p.y=3.2f;if(track.points[(i*23+7)%TrackWorld.Count].y>4)continue;var stop=Node(architecture,"Jeepney waiting shed",p,Quaternion.LookRotation(track.Right((i*23+7)%TrackWorld.Count)));for(int s=-1;s<=1;s+=2)Box(stop,"Waiting shed post",new Vector3(s*2,1.6f,0),new Vector3(.12f,3.2f,.12f),metal,true);Box(stop,"Waiting shed roof",new Vector3(0,3.2f,0),new Vector3(5,.15f,2.8f),green);Box(stop,"Bench",new Vector3(0,.7f,0),new Vector3(4,.2f,.55f),wood,true);Text(stop,"Sakayan",new Vector3(0,2.7f,-.12f),.16f,Color.white,new Vector3(0,180,0));}
  }
  void Batch(){
   var batches=new Dictionary<(Material,int,int),List<CombineInstance>>();var old=new List<MeshRenderer>();
   foreach(var mf in architecture.GetComponentsInChildren<MeshFilter>()){
    var renderer=mf.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled)continue;
    var pos=renderer.bounds.center;var key=(renderer.sharedMaterial,Mathf.FloorToInt(pos.x/90),Mathf.FloorToInt(pos.z/90));
    if(!batches.ContainsKey(key))batches[key]=new List<CombineInstance>();batches[key].Add(new CombineInstance{mesh=mf.sharedMesh,transform=mf.transform.localToWorldMatrix});old.Add(renderer);
   }
   foreach(var pair in batches){var mesh=new Mesh{name="City spatial batch",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray());var o=new GameObject("City block "+pair.Key.Item2+","+pair.Key.Item3+" "+pair.Key.Item1.name);o.transform.SetParent(root,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=pair.Key.Item1;o.AddComponent<OwnedMesh>();}
   foreach(var renderer in old)renderer.enabled=false;
  }
 }
}
