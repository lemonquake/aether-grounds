using UnityEngine;
namespace Aether {
 public partial class Vehicle {
  public float rolloverLock;
  public float BaseMass=>spec.mass;
  public void RefreshShieldMass(){if(body)body.mass=BaseMass*(shield>0?2:1);}
  public static float PulseStrength(float distance)=>Mathf.Lerp(12,46,Mathf.Pow(Mathf.Clamp01(1-distance/22),.75f));
  public void PushHit(Vector3 impulse,float seconds=1.5f){bool blocked=shield>0;Hit(impulse);if(!blocked){rolloverLock=Mathf.Max(rolloverLock,seconds);rollGrace=Mathf.Max(rollGrace,seconds);righting=0;stun=Mathf.Max(stun,seconds);}}
  void ImpactCollision(Collision c){
   var other=c.rigidbody;if(!other||other==body)return;
   float speed=c.relativeVelocity.magnitude;if(speed<3&&shield<=0)return;
   var civilian=other.GetComponent<CityProp>();var person=other.GetComponent<CityPedestrian>();
   if(person){person.Strike(transform.forward*Mathf.Max(speed,12)+Vector3.up*8,this);return;}
   bool traffic=other.GetComponent<TricycleTraffic>()||other.GetComponent<CityTraffic>()||other.GetComponent<RushTrafficBody>();
   if(traffic)RecordTraffic(other);if(!traffic&&shield<=0)return;
   Vector3 direction=Vector3.ProjectOnPlane(other.worldCenterOfMass-body.worldCenterOfMass,Vector3.up).normalized;
   if(direction.sqrMagnitude<.1f)direction=transform.forward;
   float strength=Mathf.Clamp(speed*(shield>0?1.8f:.95f),shield>0?22:9,shield>0?72:40);
   if(civilian){if(!civilian.broken)civilian.Break(c.GetContact(0).point,direction*strength,this);}
   var racer=other.GetComponent<Vehicle>();
   if(racer){racer.PushHit(direction*strength+Vector3.up*8);}
   else if(!other.isKinematic){other.AddForce(direction*strength+Vector3.up*Mathf.Clamp(strength*.42f,6,23),ForceMode.VelocityChange);other.AddTorque(transform.forward*8+transform.right*4,ForceMode.VelocityChange);var rush=other.GetComponent<RushTrafficBody>();if(rush)rush.DisableDrive();}
   ImpactEffects.Crash(c.GetContact(0).point,strength,shield>0?Color.cyan:new Color(1,.55f,.16f));
   if(shield>0){CityPhysics.Blast(c.GetContact(0).point,7,45,this);RaceEffects.Ring(transform.position,Color.cyan,6);}
   if(player)game.cameraShake=Mathf.Max(game.cameraShake,Mathf.Clamp(strength*.018f,.3f,1.1f));
  }
 }
 public class RushTrafficBody:MonoBehaviour {
  public float disabledUntil;public bool disabled=>Time.time<disabledUntil;
  public void DisableDrive(){disabledUntil=Time.time+20;}
  void OnCollisionEnter(Collision c){if(c.relativeVelocity.magnitude>5&&c.rigidbody&&c.rigidbody.GetComponent<Vehicle>())DisableDrive();}
 }
 public static class ImpactEffects {
  static float nextCrash;
  public static void Crash(Vector3 p,float strength,Color color){if(!CityPhysics.NearCamera(p,150)||Time.time<nextCrash)return;nextCrash=Time.time+.08f;
   RaceEffects.Burst(p,color,64,Mathf.Min(24,strength*.7f),.15f,.65f,1);
   RaceEffects.Burst(p,new Color(.22f,.25f,.28f,.75f),30,9,1.2f,1.8f);
   RaceEffects.Debris(p,new Color(.32f,.39f,.45f),Mathf.Clamp((int)strength,16,32),Mathf.Min(22,strength*.5f));CityPhysics.Impact(p,CityPropKind.Metal,strength);
  }
 }
 public class CityPedestrian:MonoBehaviour {
  public bool struck;public Rigidbody body;Vector3 home;Quaternion heading;float restoreAt;
  public void Initialize(){home=transform.position;heading=transform.rotation;gameObject.layer=10;var col=gameObject.AddComponent<CapsuleCollider>();col.center=Vector3.up*.95f;col.height=1.85f;col.radius=.29f;body=gameObject.AddComponent<Rigidbody>();body.mass=45;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.interpolation=RigidbodyInterpolation.Interpolate;body.constraints=RigidbodyConstraints.FreezeRotation;body.linearDamping=.3f;}
  public void Walk(Vector3 p,Quaternion q){if(struck)return;body.MovePosition(p);body.MoveRotation(q);}
  public void Strike(Vector3 velocity,Vehicle source=null){if(struck)return;struck=true;if(source&&source.CountingStats)source.stats.civilians++;restoreAt=Time.time+24;body.constraints=RigidbodyConstraints.None;body.linearVelocity=Vector3.ClampMagnitude(velocity,46);body.angularVelocity=new Vector3(5,3,8);Vector3 p=body.worldCenterOfMass;
   RaceEffects.Burst(p,new Color(.72f,.015f,.025f,.95f),70,14,.13f,.9f,1.6f);RaceEffects.Burst(p,new Color(.54f,.015f,.035f,.65f),30,6,.85f,1.4f);CityPhysics.Impact(p,CityPropKind.Loose,16);
  }
  void OnCollisionEnter(Collision c){if(c.rigidbody&&c.relativeVelocity.magnitude>3&&(c.rigidbody.GetComponent<Vehicle>()||c.rigidbody.GetComponent<CityProp>()))Strike(-c.GetContact(0).normal*c.relativeVelocity.magnitude+Vector3.up*7,c.rigidbody.GetComponent<Vehicle>()??c.rigidbody.GetComponent<CityProp>()?.lastHitBy);}
  void Update(){if(!struck||Time.time<restoreAt)return;if(Physics.CheckCapsule(home+Vector3.up*.4f,home+Vector3.up*1.5f,.6f,1<<8))return;body.position=home;body.rotation=heading;body.linearVelocity=body.angularVelocity=Vector3.zero;body.constraints=RigidbodyConstraints.FreezeRotation;struck=false;}
 }
}
