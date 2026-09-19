using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Aether {
 public class OverhaulTest:MonoBehaviour {
  int failures;Game game;string evidence;float watchdog;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Run(){if(Environment.GetCommandLineArgs().Contains("-aetherOverhaulTest"))new GameObject("Overhaul verification").AddComponent<OverhaulTest>();}
  void Check(bool pass,string detail){if(!pass)failures++;Debug.Log("OVERHAUL "+(pass?"PASS ":"FAIL ")+detail);}
  void Update(){watchdog+=Time.unscaledDeltaTime;if(watchdog>650){Debug.LogError("OVERHAUL TIMEOUT");Application.Quit(2);}}
  IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(evidence,name+".png"));yield return new WaitForSecondsRealtime(.5f);}
  void Place(Vehicle v,int n,float lane=0,float height=.4f){v.lastPoint=n;v.progress=n;v.body.position=game.track.points[n]+game.track.Right(n)*lane+Vector3.up*height;v.body.rotation=Quaternion.LookRotation(game.track.Forward(n));v.body.linearVelocity=Vector3.zero;v.body.angularVelocity=Vector3.zero;Physics.SyncTransforms();if(v==game.player){game.cam.transform.position=v.body.position-game.track.Forward(n)*9+Vector3.up*4;game.cam.transform.LookAt(v.body.position+game.track.Forward(n)*4+Vector3.up);}}
  IEnumerator Start(){
   yield return new WaitForSeconds(2);game=FindAnyObjectByType<Game>();evidence=Path.GetFullPath(Application.dataPath+"/../../Evidence/Overhaul");Directory.CreateDirectory(evidence);
   Check(CarSpec.All.Length==10,"Ten base cars");Check(Game.Paints.Length==32&&CarParts.Wheels.Length==10&&CarParts.Engines.Length==11,"Paint, wheel and engine catalogs");
   game.CurrentCar.paint=1;game.SendMessage("Changed");yield return Capture("menu");game.menu="Garage";
   while(game.garage.cars.Count<8){game.AddCar();yield return null;}Check(!game.AddCar()&&game.garage.cars.Count==8,"Eight independent garage slots and add limit");
   game.CurrentCar.engineLevel=20;game.CurrentCar.tireLevel=20;game.CurrentCar.brakeLevel=20;game.CurrentCar.engine=9;game.CurrentCar.wheel=8;game.CurrentCar.accent=3;
   var saved=JsonUtility.FromJson<GarageSave>(JsonUtility.ToJson(game.garage));Check(saved.cars.Count==8&&saved.cars[7].engineLevel==20&&saved.cars[7].engine==9&&saved.cars[0].engineLevel!=20,"Garage round-trip preserves per-car parts and upgrades");
   for(int i=0;i<10;i++){game.CurrentCar.body=i;game.CurrentCar.name="Test "+CarSpec.All[i].name;game.CurrentCar.paint=(i*3+1)%32;game.CurrentCar.wheel=i;game.CurrentCar.engine=i+1;game.SendMessage("Changed");yield return null;
    var preview=GameObject.Find("Garage display").transform.Find(CarSpec.All[i].name);Check(preview&&new[]{"WheelFL","WheelFR","WheelRL","WheelRR"}.All(n=>preview.Find(n)!=null),CarSpec.All[i].name+" has four wheel hubs");yield return Capture("car-"+i+"-"+CarSpec.All[i].name);
   }
   game.CurrentCar.body=0;game.CurrentCar.paint=1;game.CurrentCar.engine=3;game.CurrentCar.engineLevel=0;game.CurrentCar.tireLevel=0;game.CurrentCar.brakeLevel=0;game.SendMessage("Changed");
   game.aiCount=19;game.laps=1;game.difficulty=1;game.mapIndex=0;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);var car=game.player;car.manualTest=true;
   Check(game.racers.Count==20,"20-car race grid");Check(game.track.tiles.Count==8&&game.track.coins.Count>45,"Track tiles and lap coins generated");
   foreach(var v in game.racers){if(v!=car)v.manualTest=true;}
   var coin=game.track.coins[0];int before=car.coins;Check(coin.Collect(car)&&!coin.Collect(car)&&car.coins==before+1,"Coin cannot be farmed within a lap");car.progress=TrackWorld.Count;Check(coin.Collect(car)&&car.coins==before+2,"Coin regenerates next lap");car.progress=0;
   Place(car,36);yield return new WaitForSeconds(.15f);Check(car.boost>1,"Booster tile activates on contact");yield return Capture("booster");
   Place(car,101);yield return new WaitForSeconds(.15f);Check(car.body.linearVelocity.y>5,"Jump tile launches car");yield return Capture("jump");
   var ramp=game.track.tiles.First(t=>t.kind==TileKind.Ramp);car.body.position=ramp.transform.position+Vector3.up*.4f;car.body.rotation=ramp.transform.rotation;car.body.linearVelocity=game.track.Forward(ramp.point)*25;Physics.SyncTransforms();yield return new WaitForSeconds(.12f);Check(car.body.linearVelocity.y>5,"Ramp lip launches car");yield return Capture("ramp");
   Place(car,148,-4,.2f);car.boost=0;car.throttle=0;car.body.linearVelocity=game.track.Forward(148)*24;yield return new WaitForSeconds(.4f);Check(car.wet&&car.speed<75,"Flooded broken road slows car");yield return Capture("water");
   Place(car,148,7);yield return new WaitForSeconds(.1f);Check(!car.wet,"Dry bypass avoids water drag");
   Place(car,62,0,3);car.body.rotation=Quaternion.LookRotation(game.track.Forward(62))*Quaternion.Euler(0,0,180);Physics.SyncTransforms();yield return new WaitForSeconds(.65f);Check(Vector3.Dot(car.transform.up,Vector3.up)>.85f,"Automatic upright recovery within 0.65 seconds");
   Place(car,65,0,1);car.Hit(car.transform.right*18);yield return new WaitForFixedUpdate();Check(car.body.angularVelocity.magnitude>4,"Heavy hit causes physical roll");
   Place(car,70);car.item="Shield";car.UseItem();car.Hit(Vector3.right*20);Check(car.shield==0,"Shield absorbs heavy impact");
   var crate=FindObjectsByType<Pickup>().Where(p=>p.Available).OrderByDescending(p=>Vector3.Distance(p.home,car.transform.position)).First();car.item="";car.body.position=crate.home-Vector3.up*.6f;car.body.linearVelocity=Vector3.zero;car.body.angularVelocity=Vector3.zero;car.body.rotation=Quaternion.LookRotation(game.track.Forward(game.track.Nearest(crate.home)));car.lastPoint=game.track.Nearest(crate.home);Physics.SyncTransforms();yield return new WaitForSeconds(.12f);Check(car.item!=""&&!crate.Available&&FindObjectsByType<Fragment>().Length>0,"Crate gives item and breaks into fragments item="+car.item+" fragments="+FindObjectsByType<Fragment>().Length);yield return Capture("crate-break");
   Place(car,60);var target=game.racers[1];Place(target,64);int impacts=Rocket.ImpactCount;car.item="Rocket";car.UseItem();Check(FindAnyObjectByType<Rocket>()!=null,"Modeled missile spawns");yield return new WaitForSeconds(1.4f);Check(Rocket.ImpactCount>impacts,"Missile impact produces explosion or hits target");yield return Capture("missile");
   foreach(var v in game.racers)if(v!=car){Place(v,180+v.carIndex*5,6);v.manualTest=true;}
   target=game.racers[1];Place(target,14,0);target.manualTest=false;target.item="Boost";yield return new WaitForSeconds(3);Check(target.item=="","AI uses held Boost on a clear straight");
   car.manualTest=true;car.throttle=0;car.steer=0;car.boost=0;int returns=car.automaticReturns;
   Place(car,190,32);yield return new WaitForSeconds(1.1f);Check(car.automaticReturns==returns&&car.offTrackTime>1&&car.returnRemaining<1,"Off-track countdown runs before teleport");yield return Capture("track-return-countdown");
   yield return new WaitForSeconds(.3f);Check(car.automaticReturns==returns+1&&game.track.OnRoad(car.transform.position),"Off-track player returns to valid road after two seconds");yield return Capture("track-return-arrival");
   returns=car.automaticReturns;Place(car,194,30);yield return new WaitForSeconds(.6f);Place(car,194,7);yield return new WaitForSeconds(1.6f);Check(car.automaticReturns==returns&&car.offTrackTime==0,"Driving back cancels the return countdown");
   target=game.racers[2];target.manualTest=true;target.throttle=0;target.steer=0;returns=target.automaticReturns;Place(target,270,-32);yield return new WaitForSeconds(2.3f);Check(target.automaticReturns==returns+1&&game.track.OnRoad(target.transform.position),"AI also returns after two seconds outside the track");
   for(int map=0;map<3;map++){
    game.mapIndex=map;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);car=game.player;car.player=false;
    yield return new WaitForSeconds(12);yield return Capture("race-"+map);float start=Time.realtimeSinceStartup;int frames=0;float seconds=0;
    while(game.state==Game.State.Race&&Time.realtimeSinceStartup-start<180){frames++;seconds+=Time.unscaledDeltaTime;yield return null;}
    Check(car.finished,"AI completes "+TrackWorld.Names[map]+" progress="+car.progress);Check(game.racers.All(v=>float.IsFinite(v.body.position.x)&&float.IsFinite(v.speed)),"All 20 cars remain finite on map "+map);
    Debug.Log("OVERHAUL PERFORMANCE map="+map+" fps="+(frames/Mathf.Max(.01f,seconds)).ToString("F1")+" finishers="+game.racers.Count(v=>v.finished)+" positions="+string.Join(",",game.racers.Select(v=>v.progress.ToString("F0"))));yield return Capture("results-"+map);
   }
   Debug.Log("OVERHAUL COMPLETE failures="+failures);File.WriteAllText(Path.Combine(evidence,"test-result.txt"),"failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
