using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class Vehicle:MonoBehaviour {
  Renderer[] shadowRenderers;public Renderer[] ShadowRenderers=>shadowRenderers??(shadowRenderers=model.GetComponentsInChildren<Renderer>());public Rigidbody body;public CarSpec spec;public bool player,finished;public int place,carIndex,lastPoint;public float progress,finishTime,shield,boost,stun,energy=100;public string driver,item="";public float throttle,steer;public bool drift;
  public int rushSafePoint;public int coins;public bool wet;float launchGrace,rollGrace,splashTimer,aiLane,righting;ParticleSystem[] exhaustFire;public GameObject model;public float speed=>body?body.linearVelocity.magnitude*3.6f:0;public int grounded;public bool manualTest;float watchdog,watchProgress,overturned;
  Transform[] wheels=new Transform[4];Vector3[] hubs=new Vector3[4];float radius;float wheelSpin;float steerSmooth,stuck,fxTimer,aiItemTimer;TrailRenderer[] trails=new TrailRenderer[2];ParticleSystem smoke;AudioSource engine;float phase;
  public float engineTune,brakeTune,gripTune;TrackWorld track;Game game;Vector3 lastSafe;Quaternion safeRot;GameObject shieldVisual;TrailRenderer[] flames=new TrailRenderer[2];
  public void Initialize(Game g,int index,bool isPlayer,string name,Vector3 p,Quaternion q,int tune=0,int tires=0,int brakes=0){
   game=g;track=g.track;carIndex=index;spec=isPlayer?SupportStore.Tuned(CarSpec.All[index],g.CurrentCar):CarSpec.All[index];player=isPlayer;driver=name;transform.SetPositionAndRotation(p,q);gameObject.layer=8;
   engineTune=tune*.4f;brakeTune=brakes*.6f;gripTune=tires*.13f;if(isPlayer&&g.inventory!=null){if(g.inventory.engine=="sun-turbo")engineTune+=1.5f;if(g.inventory.chassis=="grip-kit")gripTune+=.6f;}
   model=ModelLibrary.Create(spec.name,transform);var col=gameObject.AddComponent<BoxCollider>();col.center=new Vector3(0,.68f,0);col.size=new Vector3(index==3?1.8f:1.9f,.67f,index==2||index==6?3.4f:index==5?4.8f:4.25f);
   body=gameObject.AddComponent<Rigidbody>();body.mass=spec.mass;body.centerOfMass=new Vector3(0,.3f,0);body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.linearDamping=.045f;body.angularDamping=2.1f;
   radius=new[]{.44f,.46f,.47f,.59f,.45f,.55f,.43f,.49f,.49f,.46f}[index];
   string[] names={"WheelFL","WheelFR","WheelRL","WheelRR"};for(int i=0;i<4;i++){wheels[i]=model.transform.Find(names[i]);if(wheels[i])hubs[i]=wheels[i].localPosition;}
   for(int i=0;i<2;i++){var o=new GameObject("Tire marks");o.transform.SetParent(transform,false);trails[i]=o.AddComponent<TrailRenderer>();trails[i].time=18;trails[i].minVertexDistance=.22f;trails[i].widthMultiplier=.25f;trails[i].sharedMaterial=new Material(Shader.Find("Sprites/Default"));trails[i].startColor=new Color(.025f,.03f,.05f,.8f);trails[i].endColor=new Color(.025f,.03f,.05f,0);trails[i].emitting=false;}
   var sm=new GameObject("Tire smoke");sm.transform.SetParent(transform,false);sm.transform.localPosition=new Vector3(0,.22f,-1.6f);smoke=sm.AddComponent<ParticleSystem>();var main=smoke.main;main.startLifetime=.8f;main.startSpeed=1.7f;main.startSize=.6f;main.startColor=new Color(.72f,.76f,.85f,.4f);main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=100;var emission=smoke.emission;emission.rateOverTime=0;var shape=smoke.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(1.9f,.2f,.2f);var size=smoke.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.3f,1,2.8f));smoke.GetComponent<ParticleSystemRenderer>().material=new Material(Shader.Find("Particles/Standard Unlit"));
   if(player){engine=gameObject.AddComponent<AudioSource>();engine.loop=true;engine.volume=.12f;engine.spatialBlend=0;engine.clip=MakeEngine();engine.Play();}
   smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial=Game.FXMaterial();
   shieldVisual=GameObject.CreatePrimitive(PrimitiveType.Sphere);shieldVisual.name="Shield bubble";Destroy(shieldVisual.GetComponent<Collider>());shieldVisual.transform.SetParent(transform,false);shieldVisual.transform.localPosition=Vector3.up*.65f;shieldVisual.transform.localScale=new Vector3(3.1f,2.6f,5.8f);shieldVisual.GetComponent<Renderer>().sharedMaterial=new Material(Shader.Find("Aether/Shield"));shieldVisual.SetActive(false);
   for(int i=0;i<2;i++){var o=new GameObject("Boost exhaust");o.transform.SetParent(transform,false);o.transform.localPosition=new Vector3(i==0?-.51f:.51f,.42f,-2.4f);var t=o.AddComponent<TrailRenderer>();t.time=.09f;t.minVertexDistance=.08f;t.widthMultiplier=.26f;t.sharedMaterial=Game.FXMaterial();t.startColor=new Color(.25f,1,1,.9f);t.endColor=new Color(.3f,.7f,1,0);t.emitting=false;flames[i]=t;}
   exhaustFire=new ParticleSystem[2];for(int i=0;i<2;i++){var fire=RaceEffects.Emitter(transform,"Boost fire",new Color(.2f,.72f,1,.85f),.24f,.44f,5,50);fire.transform.localPosition=new Vector3(i==0?-.51f:.51f,.45f,-2.3f);fire.transform.localRotation=Quaternion.Euler(0,180,0);exhaustFire[i]=fire;}
   PremiumLighting.Headlights(transform,player,track.map);body.maxAngularVelocity=16;lastPoint=track.Nearest(p);progress=track.isRush?0:lastPoint>TrackWorld.Count/2?lastPoint-TrackWorld.Count:lastPoint;lastSafe=p;safeRot=q;
  }
  AudioClip MakeEngine(){int count=44100;var clip=AudioClip.Create("Engine harmonics",count,1,44100,false);var samples=new float[count];for(int i=0;i<count;i++){float t=i/44100f;samples[i]=.22f*(Mathf.Sin(t*80*Mathf.PI*2)+.35f*Mathf.Sin(t*160*Mathf.PI*2)+.18f*Mathf.Sin(t*240*Mathf.PI*2));}clip.SetData(samples,0);return clip;}
  void Update(){
   if(game==null)return;if(!game.RaceSimulationActive||finished){throttle=0;steer=0;drift=false;if(engine)engine.volume=0;return;}
   UpdateWeaponStatus();UpdateTrackBounds();
   if(!manualTest){if(player&&!finished){throttle=game.phoneMode?game.touchThrottle:Input.GetAxisRaw("Vertical");steer=game.phoneMode?game.touchSteer:Input.GetAxisRaw("Horizontal");drift=game.phoneMode?game.touchDrift:Input.GetKey(KeyCode.Space);if(Input.GetKeyDown(KeyCode.E))UseItem();if(Input.GetKeyDown(KeyCode.R))Recover();if((game.phoneMode?game.touchBoost:Input.GetKey(KeyCode.LeftShift))&&energy>0&&throttle>0){boost=Mathf.Max(boost,.12f);energy=Mathf.Max(0,energy-Time.deltaTime*27*(player&&game.inventory.engine=="storm-coil"?.8f:1));}else energy=Mathf.Min(100,energy+Time.deltaTime*9);}else DriveAI();}
   shield=Mathf.Max(0,shield-Time.deltaTime);boost=Mathf.Max(0,boost-Time.deltaTime);stun=Mathf.Max(0,stun-Time.deltaTime);
   shieldVisual.SetActive(shield>0);foreach(var flame in flames)flame.emitting=boost>0;foreach(var fire in exhaustFire){var em=fire.emission;em.rateOverTime=boost>0?42:0;}
   if(engine){engine.pitch=.65f+Mathf.Clamp(speed/180,0,1.7f)+Mathf.Abs(throttle)*.15f;engine.volume=game.volume*.25f;}
   if(body.linearVelocity.magnitude<2&&throttle>.5f)stuck+=Time.deltaTime;else stuck=0;if(!player&&!manualTest&&grounded>0&&stuck>3&&offTrackTime<=0&&wrongWayTime<=0)Recover();
   int point=track.Nearest(transform.position);int delta=point-lastPoint;if(!track.isRush){if(delta>TrackWorld.Count/2)delta-=TrackWorld.Count;if(delta<-TrackWorld.Count/2)delta+=TrackWorld.Count;}
   if(!track.isStunts&&Mathf.Abs(delta)<20&&track.InRaceCorridor(transform.position)){if(track.isRush){progress=point;rushSafePoint=point;}else progress+=delta;}lastPoint=point;
   if(!player&&!finished&&!manualTest){if(progress>watchProgress+(track.isStunts?.45f:3)){watchProgress=progress;watchdog=0;}else watchdog+=Time.deltaTime;if(watchdog>5&&grounded>0&&offTrackTime<=0&&wrongWayTime<=0){Recover();watchdog=0;watchProgress=progress;}}
   if(Vector3.Dot(transform.up,Vector3.up)<.3f||transform.position.y<-20)overturned+=Time.deltaTime;else overturned=0;
   if(track.isStunts)StuntProgress();

  }
  void DriveAI(){
   if(track.isStunts){DriveStuntAI();return;}
   int near=lastPoint;int ahead=track.IsAirah?Mathf.Clamp(Mathf.RoundToInt((18+speed*.18f)/(track.length/TrackWorld.Count)),1,4):Mathf.RoundToInt(5+speed/25);int aim=track.isRush?Mathf.Min(near+ahead,TrackWorld.Count-1):(near+ahead)%TrackWorld.Count;
   float bend=Vector3.Angle(track.Forward(near),track.Forward((near+ahead*2)%TrackWorld.Count));
   float preferred=Mathf.Sin(carIndex*2.4f+driver.Length)*(track.map==1?1.2f:4);
   bool waterAhead=false;for(int k=0;k<=ahead+8;k++)if(track.Broken((near+k)%TrackWorld.Count))waterAhead=true;
   if(waterAhead)preferred=7;
   else foreach(var tile in track.tiles){int d=(tile.point-near+TrackWorld.Count)%TrackWorld.Count;if(d<14&&d>0&&bend<32){preferred=0;break;}}
   bool blocked=false,nearThreat=false;Vehicle front=null;
   foreach(var v in game.racers){if(v==this)continue;Vector3 local=transform.InverseTransformPoint(v.transform.position);if(local.sqrMagnitude<225)nearThreat=true;if(local.z>0&&local.z<24&&Mathf.Abs(local.x)<3.2f){blocked=true;front=v;if(!waterAhead)preferred+=local.x>0?-3.5f:3.5f;}}
   aiLane=Mathf.MoveTowards(aiLane,Mathf.Clamp(preferred,track.map==1?-1.8f:-8,track.map==1?1.8f:8),Time.deltaTime*6);
   if(track.isRush){preferred=Mathf.Sin(carIndex*2.4f+driver.Length)*7;foreach(var hazard in track.rushHazards){var local=transform.InverseTransformPoint(hazard.transform.position);if(local.z>0&&local.z<100&&Mathf.Abs(hazard.transform.position.x-preferred)<4)preferred=hazard.transform.position.x>0?-6:6;}if(Physics.SphereCast(transform.position+Vector3.up,1.2f,transform.forward,out var obstacle,45,(1<<10)|1,QueryTriggerInteraction.Ignore)&&obstacle.normal.y<.5f)preferred=transform.position.x>0?-6:6;aiLane=Mathf.MoveTowards(aiLane,Mathf.Clamp(preferred,-8,8),Time.deltaTime*5);}Vector3 target=track.points[aim]+track.Right(aim)*aiLane;if(track.isRush&&near>TrackWorld.Count-8)target.z=track.RushEnd+120;Vector3 localTarget=transform.InverseTransformPoint(target);
   float angle=Mathf.Atan2(localTarget.x,localTarget.z)*Mathf.Rad2Deg;steer=Mathf.Clamp(angle/27,-1,1);
   float desired=Mathf.Lerp(205,83,Mathf.Clamp01(bend/65))*game.difficultyFactor;
   if(track.IsAirah){float farBend=Vector3.Angle(track.Forward(near),track.Forward((near+7)%TrackWorld.Count));desired=Mathf.Min(desired,Mathf.Lerp(185,66,Mathf.Clamp01(farBend/65)));}
   if(track.map==1)desired=Mathf.Min(desired,Mathf.Lerp(90,32,Mathf.Clamp01(bend/60)));
   if(Mathf.Abs(angle)>45)desired=Mathf.Min(desired,65);if(wet)desired=80;if(blocked&&front&&front.speed<speed&&Mathf.Abs(aiLane-preferred)>2)desired=Mathf.Min(desired,front.speed+8);
   throttle=speed>desired?-Mathf.Clamp((speed-desired)/28,0,.8f):1;drift=bend>32&&bend<58&&speed>110&&game.difficulty>0;
   bool safeBoost=track.map!=1&&bend<20&&Mathf.Abs(angle)<14&&!blocked&&!waterAhead&&grounded>=2;
   energy=Mathf.Min(100,energy+Time.deltaTime*(drift?16:7));if(safeBoost&&energy>25&&speed<desired+12){boost=Mathf.Max(boost,.12f);energy=Mathf.Max(0,energy-Time.deltaTime*27*(player&&game.inventory.engine=="storm-coil"?.8f:1));}
   aiItemTimer+=Time.deltaTime;
   bool use=item=="Boost"?safeBoost:item=="Shield"?nearThreat:item=="Pulse"?nearThreat:item=="Rocket"?game.racers.Exists(v=>v!=this&&Vector3.Dot(transform.forward,v.transform.position-transform.position)>3&&Vector3.Distance(v.transform.position,transform.position)<100):item=="Oil Slick"||item=="Gravity Mine"?nearThreat:WeaponCatalog.IsExtra(item)&&nearThreat;
   if(aiItemTimer>1&&use){UseItem();aiItemTimer=0;}
  }
  void FixedUpdate(){
   if(game==null||body==null)return;
   bool active=game.RaceSimulationActive&&!finished;if(active){RaceTelemetry();CheckFinishCrossing();active=!finished;}else ResetFinishSample();RefreshShieldMass();if(active)rolloverLock=Mathf.Max(0,rolloverLock-Time.fixedDeltaTime);if(empFor>0)boost=0;
   launchGrace=Mathf.Max(0,launchGrace-Time.fixedDeltaTime);rollGrace=Mathf.Max(0,rollGrace-Time.fixedDeltaTime);
   if(active){foreach(var tile in track.tiles)tile.Apply(this);wet=track.InWater(transform.position,lastPoint);if(wet){Vector3 planar=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);body.AddForce(-planar*1.2f,ForceMode.Acceleration);splashTimer-=Time.fixedDeltaTime;if(splashTimer<=0&&speed>12&&Vector3.Distance(transform.position,game.cam.transform.position)<110){RaceEffects.Splash(transform.position+transform.forward*.9f,speed/3.6f);splashTimer=.16f;}}}
   UpdateFlipRecovery(active);
   if(frozenFor>0){body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;return;}
   float accel=active?throttle:0;steerSmooth=Mathf.MoveTowards(steerSmooth,steer,Time.fixedDeltaTime*3);float forwardSpeed=Vector3.Dot(body.linearVelocity,transform.forward);
   grounded=0;wheelSpin+=forwardSpeed/radius*Mathf.Rad2Deg*Time.fixedDeltaTime;float slip=Mathf.Abs(Vector3.Dot(body.linearVelocity,transform.right));
   for(int i=0;i<4;i++){
    Vector3 origin=transform.TransformPoint(hubs[i]+Vector3.up*.32f);bool contact=Physics.Raycast(origin,-transform.up,out RaycastHit hit,radius+.65f,~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore);
    Quaternion turn=Quaternion.Euler(0,i<2?steerSmooth*Mathf.Lerp(30,12,Mathf.Clamp01(speed/230)):0,0);Vector3 fwd=transform.rotation*turn*Vector3.forward;Vector3 right=transform.rotation*turn*Vector3.right;
    if(contact&&launchGrace<=0){grounded++;float compression=.32f-(hit.distance-radius);Vector3 velocity=body.GetPointVelocity(origin);float spring=Mathf.Clamp((compression*(carIndex==3?30000:36000)*(track.isStunts?2.5f:1)-Vector3.Dot(velocity,transform.up)*4200*(track.isStunts?1.6f:1))*(body.mass/spec.mass),0,26000*(body.mass/spec.mass)*(track.isStunts?2.5f:1));body.AddForceAtPosition(hit.normal*spring,origin);
     float lateral=Vector3.Dot(velocity,right);float grip=(spec.grip+gripTune)*(oilFor>0?.20f:1)*(drift&&i>=2?.36f:1);body.AddForceAtPosition(-right*Mathf.Clamp(lateral*grip,-14,14)*body.mass/4,hit.point);
     float a=(spec.accel+engineTune)*(boost>0?1.8f:1)*(stun>0?.15f:1);float cap=spec.speed+engineTune*1.875f+(boost>0?22:0);
     if(accel>0&&forwardSpeed<cap)body.AddForceAtPosition(fwd*accel*a*body.mass/4,hit.point);
     if(accel<0){if(forwardSpeed>1.2f)body.AddForceAtPosition(-fwd*(-accel)*(spec.brake+brakeTune)*body.mass/4,hit.point);else if(forwardSpeed>-13)body.AddForceAtPosition(fwd*accel*a*.55f*body.mass/4,hit.point);}
     if(!active)body.AddForceAtPosition(-fwd*Vector3.Dot(velocity,fwd)*body.mass*2,hit.point);
     if(wheels[i])wheels[i].localPosition=hubs[i]+Vector3.up*Mathf.Clamp(compression,-.3f,.3f);
     if(i>=2){trails[i-2].transform.position=hit.point+hit.normal*.028f;trails[i-2].emitting=active&&speed>28&&(drift||slip>4);}
    }else{if(wheels[i])wheels[i].localPosition=hubs[i]-Vector3.up*.15f;if(i>=2)trails[i-2].emitting=false;}
    if(wheels[i])wheels[i].localRotation=turn*Quaternion.Euler(wheelSpin,0,0);
   }
   if(grounded>1){body.AddForce(-transform.up*body.linearVelocity.sqrMagnitude*1.6f);float targetYaw=steerSmooth*Mathf.Clamp(forwardSpeed/18,-.7f,1.5f)*(drift?1.35f:1);body.AddTorque(Vector3.up*(targetYaw-body.angularVelocity.y)*body.mass*1.7f);if(rollGrace<=0&&rolloverLock<=0&&Vector3.Dot(transform.up,Vector3.up)>.65f)body.AddTorque(Vector3.Cross(transform.up,Vector3.up)*body.mass*3);}
   if(active)StuntPhysics();
   var emission=smoke.emission;emission.rateOverTime=0;UpdateDrivingEffects(active);
   if(drift&&speed>55&&grounded>1&&player)energy=Mathf.Min(100,energy+Time.fixedDeltaTime*16);
  }
  public void Recover(){TeleportToRoad(false);}
  public void UseItem(){if(item==""||!game.RaceSimulationActive||finished||frozenFor>0)return;string used=item;stats.itemsUsed++;item="";if(used=="Boost"){boost=3.2f;}else if(used=="Shield"){shield=8;RefreshShieldMass();}else if(used=="Pulse"){CityPhysics.Blast(transform.position,22,58,this);RaceEffects.Ring(transform.position,Color.cyan,22);game.Burst(transform.position,new Color(.4f,1,1),3);foreach(var v in game.racers)if(v!=this&&Vector3.Distance(v.transform.position,transform.position)<22)v.PushHit((v.transform.position-transform.position).normalized*PulseStrength(Vector3.Distance(v.transform.position,transform.position))+Vector3.up*7);}else if(used=="Rocket"){var o=RaceEffects.Missile();o.transform.SetPositionAndRotation(transform.position+transform.forward*3+Vector3.up*.8f,transform.rotation);var r=o.AddComponent<Rocket>();r.owner=this;r.game=game;r.direction=transform.forward;}
   UseExtraWeapon(used);if(player)game.Toast(used+" activated");}
  public void Hit(Vector3 impulse){if(shield>0){shield=0;RefreshShieldMass();RaceEffects.Ring(transform.position,Color.cyan,4);game.Burst(transform.position,Color.cyan,1);return;}if(player&&game.inventory.chassis=="rock-plate")impulse*=.75f;body.AddForce(impulse+Vector3.up*4,ForceMode.VelocityChange);float side=Vector3.Dot(impulse,transform.right);body.AddTorque(transform.forward*(-Mathf.Sign(side==0?1:side)*7)+transform.right*2,ForceMode.VelocityChange);rollGrace=1.5f;rolloverLock=Mathf.Max(rolloverLock,1.5f);righting=0;stun=Mathf.Max(stun,1.5f);if(player)game.cameraShake=.8f;}
  void OnCollisionEnter(Collision c){RecordFlipContact(c);if(CountingStats&&c.relativeVelocity.magnitude>4)stats.collisions++;if(game!=null)ImpactCollision(c);if(game==null||c.relativeVelocity.magnitude<4||Time.time-fxTimer<.18f)return;fxTimer=Time.time;float impact=c.relativeVelocity.magnitude;Vector3 point=c.GetContact(0).point;game.Burst(point,new Color(1,.65f,.25f),Mathf.Clamp(impact/10,.4f,2.5f));
   if(impact>11){RaceEffects.Burst(point,new Color(.48f,.45f,.41f,.6f),12,4,.7f,.8f);RaceEffects.Debris(point,new Color(.3f,.34f,.39f),Mathf.Min(28,(int)(impact)),Mathf.Min(18,impact*.55f));}
   Vector3 normal=c.GetContact(0).normal;if(shield<=0&&impact>17&&Mathf.Abs(normal.y)<.5f&&rollGrace<=0&&(c.rigidbody==null||c.rigidbody.mass>body.mass*.65f)){float side=Vector3.Dot(normal,transform.right);body.AddForce(Vector3.up*Mathf.Clamp(impact*.16f,2.5f,6),ForceMode.VelocityChange);body.AddTorque(transform.forward*(-Mathf.Sign(side==0?1:side)*Mathf.Clamp(impact*.24f,4,9)),ForceMode.VelocityChange);rollGrace=.65f;}
   if(player)game.cameraShake=Mathf.Min(.9f,impact*.022f);
  }
  public void Launch(float force){if(launchGrace>0)return;var velocity=body.linearVelocity;velocity.y=Mathf.Max(velocity.y,force);body.linearVelocity=velocity;launchGrace=.23f;}
  public void NotifyTile(string message){game.Toast(message);}
 }
 public class Rocket:MonoBehaviour {
  public static int ImpactCount;public Vehicle owner;public Game game;public Vector3 direction;float age;Vehicle target;bool exploded;
  void Update(){if(!game.RaceSimulationActive)return;age+=Time.deltaTime;
   if(!target){float d=130;foreach(var v in game.racers){float dist=Vector3.Distance(v.transform.position,transform.position);if(v!=owner&&Vector3.Dot(direction,(v.transform.position-transform.position).normalized)>.3f&&dist<d){target=v;d=dist;}}}
   if(target)direction=Vector3.RotateTowards(direction,(target.transform.position+Vector3.up*.7f-transform.position).normalized,Time.deltaTime*1.6f,0);
   float step=95*Time.deltaTime;Vector3 from=transform.position;if(Physics.SphereCast(from,.2f,direction,out RaycastHit hit,step,~(1<<9),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<Vehicle>()!=owner){transform.position=hit.point;Explode();return;}
   transform.position+=direction*step;transform.rotation=Quaternion.LookRotation(direction);if(age>5)Explode();
  }
  public void Explode(){if(exploded)return;exploded=true;ImpactCount++;RaceEffects.Explosion(transform.position,1.65f,owner);foreach(var v in game.racers){float dist=Vector3.Distance(v.transform.position,transform.position);if(v!=owner&&dist<16){bool protectedByShield=v.shield>0;v.Hit((v.transform.position-transform.position).normalized*Mathf.Lerp(38,10,dist/16)+Vector3.up*Mathf.Lerp(9,2,dist/16));if(!protectedByShield)v.stun=Mathf.Max(v.stun,1.8f);}}Destroy(gameObject);}
 }
}

