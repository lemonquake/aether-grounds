using System.Collections.Generic;
using UnityEngine;

namespace Aether {
 public class CityLife:MonoBehaviour {
  public TrackWorld track;
  readonly List<(Transform root,Renderer[] renderers)> propVisuals=new List<(Transform,Renderer[])>();
  readonly List<(Transform root,Transform left,Transform right,Vector3 home,Vector3 direction,float phase)> people=new List<(Transform,Transform,Transform,Vector3,Vector3,float)>();
  float refresh;
  public void Initialize(TrackWorld world,Transform props){
   track=world;
   foreach(var prop in props.GetComponentsInChildren<CityProp>())if(!prop.GetComponent<RespawningStreetLamp>())propVisuals.Add((prop.transform,prop.GetComponentsInChildren<Renderer>()));
   var skin=ModelLibrary.Material("City citizen skin",new Color(.52f,.31f,.19f));var pants=ModelLibrary.Material("City citizen trousers",new Color(.08f,.15f,.2f));
   for(int i=0;i<36;i++){
    int n=(i*13+10)%TrackWorld.Count;if(track.points[n].y>4||(track.map==0&&n>138&&n<202))continue;
    var home=track.points[n]+track.Right(n)*(i%2==0?1:-1)*(track.map==1?5.4f:20.7f);home.y=3.2f;var root=new GameObject("Sidewalk resident").transform;root.SetParent(transform,false);root.position=home;root.rotation=Quaternion.LookRotation(track.Forward(n));
    var shirt=ModelLibrary.Material("City citizen shirt "+i%5,new[]{new Color(.80f,.29f,.15f),new Color(.16f,.43f,.58f),new Color(.94f,.75f,.27f),new Color(.22f,.46f,.28f),new Color(.83f,.78f,.66f)}[i%5]);
    CarParts.Part(root,"Torso",PrimitiveType.Capsule,new Vector3(0,1.12f,0),new Vector3(.46f,.36f,.3f),shirt);
    CarParts.Part(root,"Head",PrimitiveType.Sphere,new Vector3(0,1.68f,0),new Vector3(.34f,.38f,.33f),skin);
    CarParts.Part(root,"Hair",PrimitiveType.Sphere,new Vector3(0,1.83f,-.015f),new Vector3(.35f,.15f,.33f),pants);
    var left=new GameObject("Left leg").transform;left.SetParent(root,false);left.localPosition=new Vector3(-.13f,.8f,0);
    var right=new GameObject("Right leg").transform;right.SetParent(root,false);right.localPosition=new Vector3(.13f,.8f,0);
    foreach(var leg in new[]{left,right}){CarParts.Part(leg,"Trouser leg",PrimitiveType.Capsule,new Vector3(0,-.33f,0),new Vector3(.18f,.34f,.19f),pants);CarParts.Part(leg,"Shoe",PrimitiveType.Cube,new Vector3(0,-.71f,.05f),new Vector3(.2f,.12f,.33f),pants);}
    for(int s=-1;s<=1;s+=2)CarParts.Part(root,"Arm",PrimitiveType.Capsule,new Vector3(s*.3f,1.05f,0),new Vector3(.13f,.3f,.13f),skin,new Vector3(0,0,s*8));
    root.gameObject.AddComponent<CityPedestrian>().Initialize();people.Add((root,left,right,home,track.Forward(n),i*.7f));
   }
  }
  void Update(){
   refresh-=Time.deltaTime;
   if(refresh<=0){refresh=.45f;var cam=Camera.main;if(cam)foreach(var p in propVisuals)if(p.root){bool visible=(p.root.position-cam.transform.position).sqrMagnitude<190*190;foreach(var r in p.renderers)if(r)r.enabled=visible;}}
   foreach(var person in people){
    var physics=person.root.GetComponent<CityPedestrian>();if(physics.struck)continue;
    float t=Time.time*.22f+person.phase;Vector3 pos=person.home+person.direction*Mathf.Sin(t)*3.2f;
    var car=Camera.main?Camera.main.transform:null;bool nearby=car&&(pos-car.position).sqrMagnitude<130*130;if(!nearby){person.root.gameObject.SetActive(false);continue;}person.root.gameObject.SetActive(true);
    physics.Walk(pos,Quaternion.LookRotation(person.direction*(Mathf.Cos(t)>0?1:-1)));float step=Mathf.Sin(Time.time*5+person.phase)*18;person.left.localRotation=Quaternion.Euler(step,0,0);person.right.localRotation=Quaternion.Euler(-step,0,0);
   }
  }
 }
 public class CityTraffic:MonoBehaviour {
  public TrackWorld track;public Vector3 start,direction;Rigidbody body;CityProp prop;
  void Start(){body=GetComponent<Rigidbody>();prop=GetComponent<CityProp>();}
  void FixedUpdate(){
   if(!body||!track||prop.broken)return;
   var next=body.position+direction*8;
   // Yield well before any race crossing. Traffic never teleports through racers.
   bool crossing=TrackWorld.FlatDistance(next,track.points[track.Nearest(next)])<35;
   bool obstacle=Physics.Raycast(body.position+Vector3.up*.7f,direction,10,(1<<8)|(1<<10),QueryTriggerInteraction.Ignore);
   float travel=Vector3.Dot(body.position-start,direction);
   float speed=crossing||obstacle||travel>65?0:5;
   var planar=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);body.AddForce((direction*speed-planar)*2,ForceMode.Acceleration);
   if(travel>65&&!crossing&&body.linearVelocity.magnitude<.3f){direction=-direction;start=body.position;body.MoveRotation(Quaternion.LookRotation(direction));}
  }
  void OnCollisionEnter(Collision c){if(c.relativeVelocity.magnitude>7)prop.broken=true;}
 }
 public class CityBoatBob:MonoBehaviour {Vector3 home;Quaternion rotation;void Start(){home=transform.position;rotation=transform.rotation;}void Update(){transform.position=home+Vector3.up*Mathf.Sin(Time.time*.8f+home.z)*.14f;transform.rotation=rotation*Quaternion.Euler(Mathf.Sin(Time.time*.6f+home.x)*2,0,Mathf.Sin(Time.time+home.z)*2);}}
 public partial class TrackWorld {
  public bool CityStreet(Vector3 p){
   if(map!=0||p.y<.5f||p.y>10||p.x < -598||p.x>445||p.z< -540||p.z>560)return false;
   foreach(float z in new[]{-350f,-190f,-30f,190f,350f})if(Mathf.Abs(p.z-z)<10)return true;
   foreach(float x in new[]{-390f,-230f,80f,275f})if(Mathf.Abs(p.x-x)<10)return true;
   return false;
  }
 }
}
