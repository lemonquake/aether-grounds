using System.Collections.Generic;
using UnityEngine;

namespace Aether {
 public enum CityPropKind { Loose, Cart, Timber, Tree, Rock, Metal }

 // Full-size props use their own collision layer. They collide with cars, scenery,
 // each other and the ground; purely visual sparks remain on the existing FX layer.
 public class CityProp : MonoBehaviour {
  public CityPropKind kind;
  public Rigidbody body;
  public float breakSpeed=12;
  public bool broken;public Vehicle lastHitBy;
  public float submergedAt=-100;
  float nextSound;
  public void Configure(CityPropKind type,float mass,bool anchored=false,float threshold=12) {
   kind=type;breakSpeed=threshold;
   foreach(var t in GetComponentsInChildren<Transform>())t.gameObject.layer=10;
   body=gameObject.AddComponent<Rigidbody>();body.mass=mass;body.isKinematic=anchored;
   body.interpolation=RigidbodyInterpolation.Interpolate;
   body.collisionDetectionMode=anchored?CollisionDetectionMode.ContinuousSpeculative:CollisionDetectionMode.ContinuousDynamic;
   body.linearDamping=.12f;body.angularDamping=.32f;body.maxAngularVelocity=15;
   body.maxDepenetrationVelocity=8;body.solverIterations=10;body.solverVelocityIterations=4;
   foreach(var c in GetComponentsInChildren<Collider>())c.sharedMaterial=CityPhysics.Material(type);
   if(!anchored)body.Sleep();
  }
  void OnCollisionEnter(Collision c) {
   if(c.rigidbody){var source=c.rigidbody.GetComponent<Vehicle>()??c.rigidbody.GetComponent<CityProp>()?.lastHitBy;if(source)lastHitBy=source;}float mass=c.rigidbody?c.rigidbody.mass:body.mass;
   // A falling piece of fruit must never uproot a tree.
   float strength=c.relativeVelocity.magnitude*Mathf.Clamp(Mathf.Sqrt(mass/120),.05f,1.4f);
   if(!broken && kind!=CityPropKind.Loose && strength>breakSpeed)Break(c.GetContact(0).point,c.relativeVelocity.normalized*Mathf.Min(strength,24));
   if(c.relativeVelocity.magnitude>3 && Time.time>nextSound && CityPhysics.NearCamera(transform.position,85)){
    nextSound=Time.time+.3f;CityPhysics.Impact(c.GetContact(0).point,kind,c.relativeVelocity.magnitude);
   }
  }
  public void Push(Vector3 center,float radius,float speed,Vehicle source=null){if(source)lastHitBy=source;
   if(!body)return;
   float distance=Vector3.Distance(body.worldCenterOfMass,center);
   float falloff=Mathf.Clamp01(1-distance/radius);if(falloff<=0)return;
   Vector3 force=(body.worldCenterOfMass-center).normalized*speed*falloff+Vector3.up*speed*.3f*falloff;
   if(!broken&&kind!=CityPropKind.Loose&&speed*falloff>breakSpeed*.55f)Break(center,force);
   if(body&&!body.isKinematic){body.WakeUp();body.AddForce(force,ForceMode.VelocityChange);body.AddTorque(Vector3.Cross(Vector3.up,force)*.35f,ForceMode.VelocityChange);}
  }
  public void Break(Vector3 point,Vector3 force,Vehicle source=null){if(source)lastHitBy=source;
   if(broken)return;broken=true;CityPhysics.TotalBroken++;if(lastHitBy&&lastHitBy.CountingStats){if(GetComponent<CityTraffic>()||GetComponent<TricycleTraffic>()||GetComponent<RushTrafficBody>())lastHitBy.RecordTraffic(this);else lastHitBy.stats.propsDestroyed++;}
   if(kind==CityPropKind.Tree){
    body.isKinematic=false;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
    body.AddForceAtPosition((Vector3.ProjectOnPlane(force,Vector3.up).normalized+Vector3.up*.3f)*body.mass*10,transform.position+Vector3.up*4,ForceMode.Impulse);
    body.AddTorque(Vector3.Cross(Vector3.up,force.normalized)*5,ForceMode.VelocityChange);
    if(CityPhysics.NearCamera(point,120))RaceEffects.Burst(point,new Color(.48f,.36f,.16f),24,5,.3f,1,1);
   } else if(kind==CityPropKind.Rock){
    for(int i=0;i<7;i++)CityPhysics.Shard(transform.parent,transform.position+Vector3.up*.7f+Random.insideUnitSphere*.6f,new Vector3(.6f,.5f,.65f)*Random.Range(.7f,1.8f),CityPropKind.Rock,force+Random.onUnitSphere*5);
    gameObject.SetActive(false);Destroy(gameObject);
   } else if(kind==CityPropKind.Cart||kind==CityPropKind.Timber){
    var chunks=new List<Transform>();foreach(Transform child in transform)if(child.GetComponent<CityChunk>())chunks.Add(child);
    if(chunks.Count>0){foreach(var chunk in chunks){chunk.SetParent(transform.parent,true);CityPhysics.ReleaseChunk(chunk.gameObject,force+Random.insideUnitSphere*3,kind);}gameObject.SetActive(false);Destroy(gameObject);}
    else {body.isKinematic=false;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.AddForce(force,ForceMode.VelocityChange);}
   } else {body.isKinematic=false;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;}
   if(GetComponent<RespawningStreetLamp>()){body.AddForce(force,ForceMode.VelocityChange);body.AddTorque(Vector3.Cross(Vector3.up,force)*.6f,ForceMode.VelocityChange);}
   if(CityPhysics.NearCamera(point,120)){CityPhysics.Impact(point,kind,18);RaceEffects.Burst(point,new Color(.56f,.49f,.36f,.7f),22,5,.6f,1.3f);}
  }
  void FixedUpdate(){
   if(!body||body.isKinematic||body.IsSleeping())return;
   if(body.position.y < -12&&!GetComponent<RespawningStreetLamp>()){gameObject.SetActive(false);Destroy(gameObject);return;}
   if(body.position.x>535 && body.position.y<2){
    // Buoyancy and water drag for beach debris, with denser rocks sinking.
    body.AddForce(Vector3.up*(kind==CityPropKind.Rock?5:13)-body.linearVelocity*2.2f,ForceMode.Acceleration);
    body.angularVelocity*=.98f;
    if(submergedAt<0){submergedAt=Time.time;if(CityPhysics.NearCamera(body.position,100))RaceEffects.Splash(body.position,12);}
   }
  }
 }
 public class CityChunk:MonoBehaviour {}
 public class CityDebrisLifetime:MonoBehaviour {void OnDestroy(){CityPhysics.DebrisCount=Mathf.Max(0,CityPhysics.DebrisCount-1);}}

 public static class CityPhysics {
  public static int TotalBroken,DebrisCount;
  static readonly Collider[] overlaps=new Collider[1024];
  static readonly HashSet<CityProp> affected=new HashSet<CityProp>();
  static readonly Dictionary<CityPropKind,PhysicsMaterial> materials=new Dictionary<CityPropKind,PhysicsMaterial>();
  static readonly Dictionary<CityPropKind,AudioClip> sounds=new Dictionary<CityPropKind,AudioClip>();
  static float nextAudio;
  public static bool NearCamera(Vector3 p,float range)=>Camera.main&&(Camera.main.transform.position-p).sqrMagnitude<range*range;
  public static PhysicsMaterial Material(CityPropKind type){
   if(materials.TryGetValue(type,out var m))return m;
   m=new PhysicsMaterial("City "+type){dynamicFriction=type==CityPropKind.Loose?.38f:.6f,staticFriction=.65f,bounciness=type==CityPropKind.Loose?.25f:.08f,frictionCombine=PhysicsMaterialCombine.Average,bounceCombine=PhysicsMaterialCombine.Minimum};materials[type]=m;return m;
  }
  public static void Blast(Vector3 p,float radius,float speed,Vehicle source=null){
   int count=Physics.OverlapSphereNonAlloc(p,radius,overlaps,1<<10,QueryTriggerInteraction.Ignore);affected.Clear();
   for(int i=0;i<count;i++){var prop=overlaps[i].GetComponentInParent<CityProp>();if(prop)affected.Add(prop);}
   foreach(var prop in affected)if(prop)prop.Push(p,radius,speed,source);
   var bodies=new HashSet<Rigidbody>();for(int i=0;i<count;i++){var body=overlaps[i].attachedRigidbody;if(!body||!bodies.Add(body))continue;float d=Vector3.Distance(body.worldCenterOfMass,p),f=Mathf.Clamp01(1-d/radius);var pedestrian=body.GetComponent<CityPedestrian>();if(pedestrian)pedestrian.Strike((body.worldCenterOfMass-p).normalized*speed*f+Vector3.up*8,source);var traffic=body.GetComponent<RushTrafficBody>();if(traffic){if(source)source.RecordTraffic(traffic);traffic.DisableDrive();body.AddForce((body.worldCenterOfMass-p).normalized*speed*f+Vector3.up*speed*.35f*f,ForceMode.VelocityChange);body.AddTorque(Random.insideUnitSphere*9,ForceMode.VelocityChange);}}
  }
  public static void ReleaseChunk(GameObject obj,Vector3 velocity,CityPropKind type){
   if(DebrisCount>=160){Object.Destroy(obj);return;}DebrisCount++;
   var prop=obj.AddComponent<CityProp>();prop.Configure(CityPropKind.Loose,type==CityPropKind.Metal?24:12);prop.broken=true;prop.body.linearVelocity=Vector3.ClampMagnitude(velocity,28);prop.body.angularVelocity=Random.insideUnitSphere*7;
   obj.AddComponent<CityDebrisLifetime>();Object.Destroy(obj,28);
  }
  public static void Shard(Transform parent,Vector3 p,Vector3 size,CityPropKind type,Vector3 velocity){
   if(DebrisCount>=160)return;
   var o=ModelLibrary.Create("K_rock_smallA",parent);o.name="Colliding rock fragment";o.transform.position=p;o.transform.localScale=size*2;
   var c=o.AddComponent<BoxCollider>();c.center=Vector3.up*.16f;c.size=new Vector3(.45f,.32f,.45f);ReleaseChunk(o,velocity,type);
  }
  public static void Impact(Vector3 p,CityPropKind kind,float strength){
   if(Time.time<nextAudio)return;nextAudio=Time.time+.065f;
   if(!sounds.TryGetValue(kind,out var clip)){
    const int count=10000;float[] samples=new float[count];var random=new System.Random(130+(int)kind);
    for(int i=0;i<count;i++){float t=i/22050f;float noise=(float)random.NextDouble()*2-1;float tone=Mathf.Sin(t*(kind==CityPropKind.Metal?810:kind==CityPropKind.Rock?130:240)*Mathf.PI*2);samples[i]=(noise*.6f+tone*.4f)*Mathf.Exp(-t*(kind==CityPropKind.Metal?14:24))*.55f;}
    clip=AudioClip.Create("City impact "+kind,count,1,22050,false);clip.SetData(samples,0);sounds[kind]=clip;
   }
   var o=new GameObject("City impact sound");o.transform.position=p;var source=o.AddComponent<AudioSource>();source.clip=clip;source.spatialBlend=1;source.minDistance=6;source.maxDistance=70;source.rolloffMode=AudioRolloffMode.Linear;var game=Object.FindAnyObjectByType<Game>();source.volume=(game?game.volume:.5f)*Mathf.Clamp01(strength/15);source.pitch=Random.Range(.85f,1.15f);source.Play();Object.Destroy(o,1);
  }
 }
}
