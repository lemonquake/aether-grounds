using UnityEngine;
namespace Aether {
 public class RespawningStreetLamp:MonoBehaviour {
  public const float RespawnDelay=10;public float remaining;public int respawns;Vector3 home;Quaternion rotation;CityProp prop;Game game;Light bulb;bool waiting;Renderer[] visuals;
  void Start(){home=transform.position;rotation=transform.rotation;prop=GetComponent<CityProp>();game=FindAnyObjectByType<Game>();bulb=GetComponentInChildren<Light>();visuals=GetComponentsInChildren<Renderer>();}
  void Update(){if(!game||game.state!=Game.State.Race)return;
   if(prop.broken&&!waiting){waiting=true;remaining=RespawnDelay;if(bulb)bulb.enabled=false;RaceEffects.Burst(transform.position+Vector3.up*4,new Color(1,.8f,.3f),28,8,.15f,.7f,1);}
   if(!waiting)return;remaining=Mathf.Max(0,remaining-Time.deltaTime);
   if(remaining>0){if(remaining<8){foreach(var r in visuals)if(r)r.enabled=false;foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;}return;}
   // Post collision stays disabled if a car is crossing its home when the visual respawns.
   prop.body.linearVelocity=prop.body.angularVelocity=Vector3.zero;prop.body.isKinematic=true;prop.body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;prop.body.position=home;prop.body.rotation=rotation;transform.SetPositionAndRotation(home,rotation);prop.broken=false;waiting=false;respawns++;
   foreach(var r in visuals)if(r)r.enabled=true;Physics.SyncTransforms();StartCoroutine(RestoreCollision());if(bulb)bulb.enabled=true;RaceEffects.Ring(home,new Color(1,.8f,.45f),2);
  }
  System.Collections.IEnumerator RestoreCollision(){while(Physics.CheckSphere(home+Vector3.up,1.4f,1<<8))yield return new WaitForSeconds(.2f);foreach(var c in GetComponentsInChildren<Collider>())c.enabled=true;}
 }
 public sealed partial class PhilippineCity {
  void BuildCircuitTraffic(){
   var system=root.gameObject.AddComponent<TownTrafficSystem>();system.Initialize(track,MakeCityTraffic);
   for(int i=0;i<60;i++){int n=20+i*(TrackWorld.Count-40)/60;float lane=i%3==0?7.5f:i%3==1?-7.5f:0;
    if(track.Broken(n))lane=8;int sign=i%5==0?1:-1;var p=track.points[n]+track.Right(n)*lane+Vector3.up*.18f;
    var car=MakeCityTraffic(p,Quaternion.LookRotation(track.Forward(n)*sign),i);var driver=car.gameObject.AddComponent<TricycleTraffic>();driver.track=track;driver.point=n;driver.travelSign=sign;driver.lane=lane;system.Register(driver,i);
   }
  }
  Transform MakeCityTraffic(Vector3 p,Quaternion q,int seed){
   if(seed%3==0)return Jeepney(p,q,seed,true);
   var o=Node(props,"Circuit civilian "+seed,p,q);Asset(new[]{"sedan","taxi","hatchback-sports","van"}[seed%4],o,Vector3.zero,seed%4==3?2:1.5f);
   BodyBox(o,new Vector3(0,.8f,0),new Vector3(1.9f,1.45f,4.1f));o.gameObject.AddComponent<CityProp>().Configure(CityPropKind.Metal,seed%4==3?310:220,false,26);return o;
  }
 }
 public partial class Game {
  void MapQuirks(float x,float y,float width){
   Card(x,y,width,223);Label(TrackWorld.Names[mapIndex],x+20,y+15,width-40,36,25,cyan,true);
   string[] details={"Hard · wide city circuit\n60 moving vehicles · 80% oncoming\n20 jeepneys + 40 civilian cars\nLate afternoon · flyover · washouts","Very challenging · narrow village\n26 tricycles · mixed directions\nNight · low visibility · sharp turns\nShort 1.87 km neighborhood circuit","Moderate · flowing garden circuit\nNo civilian traffic\nMoonlight · winding stone roads\nLanterns · jumps · water crossings","Challenging · long mountain circuit\n18 civilian cars · mixed directions\nSunny afternoon · climbing roads\nForest · bridges · boosts · jumps"};
   Label(details[mapIndex],x+20,y+61,width-36,147,20,muted);
  }
 }
}
