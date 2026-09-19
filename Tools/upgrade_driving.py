from pathlib import Path
p=Path(__file__).resolve().parents[1]/'Assets/Scripts/Vehicle.cs'
s=p.read_text(encoding='utf-8')
def method(signature,new):
 global s
 start=s.index(signature);brace=s.index('{',start);depth=1;i=brace+1
 while depth:
  if s[i]=='{':depth+=1
  if s[i]=='}':depth-=1
  i+=1
 s=s[:start]+new+s[i:]
s=s.replace('public GameObject model;','public int coins;public bool wet;float launchGrace,rollGrace,splashTimer,aiLane;ParticleSystem[] exhaustFire;public GameObject model;')
s=s.replace('engineTune=tune*1.5f;brakeTune=brakes*2;gripTune=tires*.8f;','engineTune=tune*.4f;brakeTune=brakes*.6f;gripTune=tires*.13f;')
s=s.replace('index==2?3.55f:4.25f','index==2||index==6?3.4f:index==5?4.8f:4.25f')
s=s.replace('radius=index==3?.59f:index==2?.47f:index==1?.46f:index==4?.45f:.44f;','radius=new[]{.44f,.46f,.47f,.59f,.45f,.55f,.43f,.49f,.49f,.46f}[index];')
s=s.replace('lastPoint=track.Nearest(p);','''exhaustFire=new ParticleSystem[2];for(int i=0;i<2;i++){var fire=RaceEffects.Emitter(transform,"Boost fire",new Color(.2f,.72f,1,.85f),.24f,.44f,5,50);fire.transform.localPosition=new Vector3(i==0?-.51f:.51f,.45f,-2.3f);fire.transform.localRotation=Quaternion.Euler(0,180,0);exhaustFire[i]=fire;}
   body.maxAngularVelocity=16;lastPoint=track.Nearest(p);''')
s=s.replace('boost=.12f;','boost=Mathf.Max(boost,.12f);')
s=s.replace('shieldVisual.SetActive(shield>0);foreach(var flame in flames)flame.emitting=boost>0;','shieldVisual.SetActive(shield>0);foreach(var flame in flames)flame.emitting=boost>0;foreach(var fire in exhaustFire){var em=fire.emission;em.rateOverTime=boost>0?42:0;}')
s=s.replace('if(overturned>2.5f){Recover();overturned=0;}','if(transform.position.y<-20&&overturned>.7f){Recover();overturned=0;}')
method('void DriveAI(){','''void DriveAI(){
   int near=lastPoint;int ahead=Mathf.RoundToInt(5+speed/25);int aim=(near+ahead)%TrackWorld.Count;
   float bend=Vector3.Angle(track.Forward(near),track.Forward((near+ahead*2)%TrackWorld.Count));
   float preferred=Mathf.Sin(carIndex*2.4f+driver.Length)*4;
   bool waterAhead=false;for(int k=0;k<=ahead+8;k++)if(track.Broken((near+k)%TrackWorld.Count))waterAhead=true;
   if(waterAhead)preferred=7;
   else foreach(var tile in track.tiles){int d=(tile.point-near+TrackWorld.Count)%TrackWorld.Count;if(d<14&&d>0&&bend<32){preferred=0;break;}}
   bool blocked=false,nearThreat=false;Vehicle front=null;
   foreach(var v in game.racers){if(v==this)continue;Vector3 local=transform.InverseTransformPoint(v.transform.position);if(local.sqrMagnitude<225)nearThreat=true;if(local.z>0&&local.z<24&&Mathf.Abs(local.x)<3.2f){blocked=true;front=v;if(!waterAhead)preferred+=local.x>0?-3.5f:3.5f;}}
   aiLane=Mathf.MoveTowards(aiLane,Mathf.Clamp(preferred,-8,8),Time.deltaTime*6);
   Vector3 target=track.points[aim]+track.Right(aim)*aiLane;Vector3 localTarget=transform.InverseTransformPoint(target);
   float angle=Mathf.Atan2(localTarget.x,localTarget.z)*Mathf.Rad2Deg;steer=Mathf.Clamp(angle/27,-1,1);
   float desired=Mathf.Lerp(205,83,Mathf.Clamp01(bend/65))*game.difficultyFactor;
   if(Mathf.Abs(angle)>45)desired=Mathf.Min(desired,65);if(wet)desired=80;if(blocked&&front&&front.speed<speed&&Mathf.Abs(aiLane-preferred)>2)desired=Mathf.Min(desired,front.speed+8);
   throttle=speed>desired?-Mathf.Clamp((speed-desired)/28,0,.8f):1;drift=bend>32&&bend<58&&speed>110&&game.difficulty>0;
   bool safeBoost=bend<20&&Mathf.Abs(angle)<14&&!blocked&&!waterAhead&&grounded>=2;
   energy=Mathf.Min(100,energy+Time.deltaTime*(drift?16:7));if(safeBoost&&energy>25&&speed<desired+12){boost=Mathf.Max(boost,.12f);energy=Mathf.Max(0,energy-Time.deltaTime*27);}
   aiItemTimer+=Time.deltaTime;
   bool use=item=="Boost"?safeBoost:item=="Shield"?nearThreat:item=="Pulse"?nearThreat:item=="Rocket"?game.racers.Exists(v=>v!=this&&Vector3.Dot(transform.forward,v.transform.position-transform.position)>3&&Vector3.Distance(v.transform.position,transform.position)<100):false;
   if(aiItemTimer>1&&use){UseItem();aiItemTimer=0;}
  }''')
s=s.replace('bool active=game.state==Game.State.Race;','''bool active=game.state==Game.State.Race;
   launchGrace=Mathf.Max(0,launchGrace-Time.fixedDeltaTime);rollGrace=Mathf.Max(0,rollGrace-Time.fixedDeltaTime);
   if(active){foreach(var tile in track.tiles)tile.Apply(this);wet=track.InWater(transform.position,lastPoint);if(wet){Vector3 planar=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);body.AddForce(-planar*1.2f,ForceMode.Acceleration);splashTimer-=Time.fixedDeltaTime;if(splashTimer<=0&&speed>12&&Vector3.Distance(transform.position,game.cam.transform.position)<110){RaceEffects.Splash(transform.position+transform.forward*.9f,speed/3.6f);splashTimer=.16f;}}}
   float upright=Vector3.Dot(transform.up,Vector3.up);if(active&&upright<.2f&&overturned>.10f){Vector3 heading=Vector3.ProjectOnPlane(transform.forward,Vector3.up);if(heading.sqrMagnitude<.1f)heading=track.Forward(lastPoint);body.MoveRotation(Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(heading),900*Time.fixedDeltaTime));body.angularVelocity*=.65f;rollGrace=0;}
   ''')
s=s.replace('bool contact=Physics.Raycast(','bool contact=Physics.Raycast(') # preserve definite assignment for hit below
s=s.replace('if(contact){grounded++;','if(contact&&launchGrace<=0){grounded++;')
s=s.replace('float cap=spec.speed+engineTune*2','float cap=spec.speed+engineTune*1.875f')
s=s.replace('body.AddTorque(Vector3.Cross(transform.up,Vector3.up)*body.mass*3);','if(rollGrace<=0)body.AddTorque(Vector3.Cross(transform.up,Vector3.up)*body.mass*3);')
method('public void Hit(Vector3 impulse){','''public void Hit(Vector3 impulse){if(shield>0){shield=0;RaceEffects.Ring(transform.position,Color.cyan,4);game.Burst(transform.position,Color.cyan,1);return;}body.AddForce(impulse+Vector3.up*4,ForceMode.VelocityChange);float side=Vector3.Dot(impulse,transform.right);body.AddTorque(transform.forward*(-Mathf.Sign(side==0?1:side)*7)+transform.right*2,ForceMode.VelocityChange);rollGrace=.75f;stun=1.0f;if(player)game.cameraShake=.8f;}''')
method('void OnCollisionEnter(Collision c){','''void OnCollisionEnter(Collision c){if(game==null||c.relativeVelocity.magnitude<4||Time.time-fxTimer<.18f)return;fxTimer=Time.time;float impact=c.relativeVelocity.magnitude;Vector3 point=c.GetContact(0).point;game.Burst(point,new Color(1,.65f,.25f),Mathf.Clamp(impact/10,.4f,2.5f));
   if(impact>11){RaceEffects.Burst(point,new Color(.48f,.45f,.41f,.6f),12,4,.7f,.8f);RaceEffects.Debris(point,new Color(.3f,.34f,.39f),Mathf.Min(8,(int)(impact/3)),5);}
   Vector3 normal=c.GetContact(0).normal;if(impact>17&&Mathf.Abs(normal.y)<.5f&&rollGrace<=0){float side=Vector3.Dot(normal,transform.right);body.AddForce(Vector3.up*Mathf.Clamp(impact*.16f,2.5f,6),ForceMode.VelocityChange);body.AddTorque(transform.forward*(-Mathf.Sign(side==0?1:side)*Mathf.Clamp(impact*.24f,4,9)),ForceMode.VelocityChange);rollGrace=.65f;}
   if(player)game.cameraShake=Mathf.Min(.9f,impact*.022f);
  }
  public void Launch(float force){if(launchGrace>0)return;var velocity=body.linearVelocity;velocity.y=Mathf.Max(velocity.y,force);body.linearVelocity=velocity;launchGrace=.23f;}
  public void NotifyTile(string message){game.Toast(message);}''')
start=s.index('var o=GameObject.CreatePrimitive(PrimitiveType.Sphere);o.name="Rocket";');end=s.index('var r=o.AddComponent<Rocket>();',start)
s=s[:start]+'''var o=RaceEffects.Missile();o.transform.SetPositionAndRotation(transform.position+transform.forward*3+Vector3.up*.8f,transform.rotation);'''+s[end:]
start=s.index(' public class Rocket:');s=s[:start]+''' public class Rocket:MonoBehaviour {
  public Vehicle owner;public Game game;public Vector3 direction;float age;Vehicle target;bool exploded;
  void Update(){if(game.state!=Game.State.Race)return;age+=Time.deltaTime;
   if(!target){float d=130;foreach(var v in game.racers){float dist=Vector3.Distance(v.transform.position,transform.position);if(v!=owner&&Vector3.Dot(direction,(v.transform.position-transform.position).normalized)>.3f&&dist<d){target=v;d=dist;}}}
   if(target)direction=Vector3.RotateTowards(direction,(target.transform.position+Vector3.up*.7f-transform.position).normalized,Time.deltaTime*1.6f,0);
   float step=95*Time.deltaTime;Vector3 from=transform.position;if(Physics.SphereCast(from,.2f,direction,out RaycastHit hit,step,~(1<<9),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<Vehicle>()!=owner){transform.position=hit.point;Explode();return;}
   transform.position+=direction*step;transform.rotation=Quaternion.LookRotation(direction);if(age>5)Explode();
  }
  public void Explode(){if(exploded)return;exploded=true;RaceEffects.Explosion(transform.position);foreach(var v in game.racers){float dist=Vector3.Distance(v.transform.position,transform.position);if(v!=owner&&dist<8)v.Hit((v.transform.position-transform.position).normalized*Mathf.Lerp(15,5,dist/8));}Destroy(gameObject);}
 }
}
'''
p.write_text(s,encoding='utf-8')
print('Driving upgrade integrated')
