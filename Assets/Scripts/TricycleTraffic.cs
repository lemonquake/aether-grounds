using UnityEngine;
namespace Aether {
 public sealed partial class PhilippineCity {
  Transform MakeTownTricycle(Vector3 position,Quaternion rotation,int seed){
   var o=Tricycle(position,rotation);o.name="Filipino tricycle "+seed;
   var paint=Mat("Tricycle paint "+seed%4,new[]{new Color(.72f,.13f,.09f),new Color(.11f,.36f,.56f),new Color(.17f,.45f,.23f),new Color(.88f,.62f,.12f)}[seed%4],0,.55f);
   foreach(var renderer in o.GetComponentsInChildren<Renderer>())if(renderer.name.Contains("roof")||renderer.name.Contains("Motorcycle body"))renderer.sharedMaterial=paint;
   Box(o,"Sidecar backrest",new Vector3(.5f,1.12f,-.72f),new Vector3(.85f,.65f,.16f),dark);
   Box(o,"Sidecar bench",new Vector3(.5f,.76f,-.32f),new Vector3(.86f,.12f,.65f),dark);
   Box(o,"Front windscreen",new Vector3(.5f,1.24f,.66f),new Vector3(.84f,.66f,.055f),Mat("Tricycle glass",new Color(.18f,.35f,.39f),0,.8f));
   for(int s=-1;s<=1;s+=2)Box(o,"Rear canopy pillar",new Vector3(.5f+s*.46f,1.25f,-1.0f),new Vector3(.07f,.9f,.07f),metal);
   Box(o,"Passenger footstep",new Vector3(1.12f,.35f,0),new Vector3(.34f,.08f,1.6f),metal);
   Box(o,"Motorcycle front fork",new Vector3(-.55f,.66f,.81f),new Vector3(.16f,.7f,.16f),metal,false,new Vector3(-14,0,0));
   Box(o,"Handlebars",new Vector3(-.55f,1.12f,.54f),new Vector3(.72f,.065f,.08f),metal);
   Round(o,"Motorcycle headlight",new Vector3(-.55f,1.03f,.91f),new Vector3(.24f,.04f,.24f),white,PrimitiveType.Cylinder,new Vector3(90,0,0));
   Box(o,"Rear plate",new Vector3(.45f,.66f,-1.13f),new Vector3(.63f,.25f,.03f),yellow);
   Text(o,"DAV "+(100+seed),new Vector3(.45f,.66f,-1.15f),.037f,Color.black,new Vector3(0,180,0));
   Box(o,"Tail lamp",new Vector3(.87f,.8f,-1.13f),new Vector3(.17f,.13f,.045f),red);
   var skin=Mat("Tricycle driver skin",new Color(.49f,.29f,.18f));var shirt=Mat("Tricycle driver shirt "+seed%3,seed%3==0?new Color(.22f,.36f,.59f):seed%3==1?new Color(.76f,.54f,.18f):new Color(.69f,.7f,.67f));
   Round(o,"Seated driver torso",new Vector3(-.55f,1.34f,-.19f),new Vector3(.42f,.35f,.29f),shirt,PrimitiveType.Capsule,new Vector3(12,0,0));
   Round(o,"Driver head",new Vector3(-.55f,1.84f,-.10f),new Vector3(.29f,.33f,.30f),skin,PrimitiveType.Sphere);
   Round(o,"Driver helmet",new Vector3(-.55f,1.94f,-.12f),new Vector3(.34f,.20f,.35f),dark,PrimitiveType.Sphere);
   for(int s=-1;s<=1;s+=2){Box(o,"Driver forearm",new Vector3(-.55f+s*.26f,1.21f,.29f),new Vector3(.11f,.12f,.52f),skin,false,new Vector3(10,0,0));Box(o,"Bent trouser leg",new Vector3(-.55f+s*.22f,.81f,.12f),new Vector3(.17f,.40f,.23f),dark);}
   o.GetComponent<Rigidbody>().centerOfMass=new Vector3(.1f,.4f,-.15f);
   return o;
  }
 }
 public class TricycleTraffic:MonoBehaviour {
  public TrackWorld track;public int point,travelSign=1;public float lane;
  Rigidbody body;CityProp prop;Game game;float hitPause;float wheelAngle;
  void Start(){body=GetComponent<Rigidbody>();var tireMaterial=new PhysicsMaterial("Tricycle rolling contact"){dynamicFriction=.2f,staticFriction=.25f,frictionCombine=PhysicsMaterialCombine.Minimum};foreach(var c in GetComponentsInChildren<Collider>())c.sharedMaterial=tireMaterial;prop=GetComponent<CityProp>();game=FindAnyObjectByType<Game>();body.rotation=Quaternion.LookRotation(track.Forward(point)*travelSign);}
  void FixedUpdate(){
   if(!body||!game||!game.RaceSimulationActive||prop.broken)return;
   hitPause=Mathf.Max(0,hitPause-Time.fixedDeltaTime);
   if(hitPause>0||Vector3.Dot(transform.up,Vector3.up)<.6f)return;
   int near=track.Nearest(body.position);point=near;
   if(track.IsAirah&&near>=26&&near<=43){travelSign=near<TrackWorld.AirahGapStart?-1:1;lane=travelSign>0?8.3f:-8.3f;}
   int aim=(near+travelSign*(track.IsAirah?1:3)+TrackWorld.Count)%TrackWorld.Count;
   Vector3 target=track.points[aim]+track.Right(aim)*lane;Vector3 to=Vector3.ProjectOnPlane(target-body.position,Vector3.up);
   if(to.sqrMagnitude<.1f)return;var forward=to.normalized;
   float bend=Vector3.Angle(track.Forward(near),track.Forward(aim));float desired=Mathf.Lerp(track.IsAirah?14:track.map==0?13:7,track.map==0?5:2.8f,Mathf.Clamp01(bend/50));
   // Slow for another road user, but remain physical obstacles when racers hit them.
   if(Physics.SphereCast(body.position+Vector3.up*.7f+transform.forward*1.35f,.45f,transform.forward,out var hit,4.5f,(1<<8)|(1<<10),QueryTriggerInteraction.Ignore)&&hit.rigidbody!=body)desired=1;
   if(TrackWorld.FlatDistance(body.position,track.points[near])>(track.map==1?8:17))return;
   var velocity=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);
   body.AddForce(Vector3.ClampMagnitude((forward*desired-velocity)*3,12),ForceMode.Acceleration);if(track.IsAirah){var tangent=track.Forward(near)*travelSign;body.AddForce(Vector3.up*(tangent.y*desired-body.linearVelocity.y)*3,ForceMode.Acceleration);}
   float yaw=Vector3.SignedAngle(transform.forward,forward,Vector3.up)*Mathf.Deg2Rad;
   body.AddTorque(Vector3.up*(Mathf.Clamp(yaw*4,-2,2)-body.angularVelocity.y)*3,ForceMode.Acceleration);
   body.AddTorque(Vector3.Cross(transform.up,Vector3.up)*5,ForceMode.Acceleration);
   wheelAngle+=velocity.magnitude/.34f*Mathf.Rad2Deg*Time.fixedDeltaTime;
   foreach(Transform child in transform)if(child.name.Contains("wheel"))child.localRotation=Quaternion.Euler(wheelAngle,0,90);
  }
  void OnCollisionEnter(Collision collision){if(collision.rigidbody&&collision.rigidbody.GetComponent<Vehicle>())hitPause=2.5f;}
 }
}
