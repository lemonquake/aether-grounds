using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Aether {
 [DefaultExecutionOrder(10000)]
 public class CityTest:MonoBehaviour {
  public static bool HideHUD;
  Game game;int failures;float watchdog;string evidence;bool photo;Vector3 eye,look;float fov=60;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherCityTest"))new GameObject("City verification").AddComponent<CityTest>();}
  void Check(bool value,string name){Debug.Log("CITY "+(value?"PASS ":"FAIL ")+name);if(!value)failures++;}
  void Update(){watchdog+=Time.unscaledDeltaTime;if(watchdog>440){Debug.LogError("CITY TIMEOUT");Application.Quit(2);}}
  void LateUpdate(){if(photo&&game){game.cam.transform.position=eye;game.cam.transform.LookAt(look);game.cam.fieldOfView=fov;}}
  IEnumerator Shot(string name,Vector3 position,Vector3 target,float lens=60){photo=true;HideHUD=true;eye=position;look=target;fov=lens;yield return new WaitForSecondsRealtime(.6f);ScreenCapture.CaptureScreenshot(Path.Combine(evidence,name+".png"));yield return new WaitForSecondsRealtime(.35f);photo=false;HideHUD=false;}
  void Place(int n,float lane=0,float height=.55f){var v=game.player;v.body.position=game.track.points[n]+game.track.Right(n)*lane+Vector3.up*height;v.body.rotation=Quaternion.LookRotation(game.track.Forward(n));v.body.linearVelocity=Vector3.zero;v.body.angularVelocity=Vector3.zero;v.lastPoint=n;v.progress=n;Physics.SyncTransforms();}
  IEnumerator Start(){
   yield return new WaitForSeconds(1.5f);game=FindAnyObjectByType<Game>();evidence=Path.GetFullPath(Application.dataPath+"/../../Evidence/City");Directory.CreateDirectory(evidence);game.aiCount=19;game.laps=1;game.mapIndex=0;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);
   var track=game.track;var car=game.player;foreach(var v in game.racers){v.manualTest=true;v.throttle=0;}
   Check(track.length>2800,"City race route exceeds 2.8 km: "+track.length.ToString("F0")+" m");
   Check(track.points.Max(p=>p.y)>15,"Flyover is elevated twelve metres above street level");
   var all=FindObjectsByType<CityProp>();Check(all.Length>300,"More than 300 interactive props: "+all.Length);
   Check(all.Count(p=>p.kind==CityPropKind.Cart)>20,"Market and street food carts: "+all.Count(p=>p.kind==CityPropKind.Cart));
   Check(all.Count(p=>p.kind==CityPropKind.Tree)>55,"Colliding breakable palms: "+all.Count(p=>p.kind==CityPropKind.Tree));
   Check(all.All(p=>p.body&&p.GetComponentsInChildren<Collider>().Any(c=>c.enabled)),"Every interactive prop has a rigidbody and enabled collider");
   Check(track.tiles.Count==8&&track.coins.Count>45&&game.racers.Count==20,"Tiles, coins, powerups and 20-car grid preserved");
   // Check the central driving lanes against scenery, rather than merely counting objects.
   int obstacles=0;var hits=new Collider[128];
   for(int n=0;n<TrackWorld.Count;n+=2){if(track.Broken(n)||track.tiles.Any(t=>Mathf.Abs(t.point-n)<5))continue;int count=Physics.OverlapBoxNonAlloc(track.points[n]+Vector3.up*1.35f,new Vector3(6,.5f,1),hits,Quaternion.LookRotation(track.Forward(n)),1,QueryTriggerInteraction.Ignore);for(int j=0;j<count;j++){if(hits[j]&&hits[j].bounds.size.y>2){obstacles++;Debug.Log("CITY ROAD OBSTACLE point="+n+" name="+hits[j].name);}}}
   Check(obstacles==0,"Central race lanes clear of static buildings and poles; obstacles="+obstacles);
   Check(Physics.Raycast(track.points[70]+Vector3.up*5,Vector3.down,out var deck,10,1)&&deck.point.y>14,"Flyover has solid drivable deck");
   Check(Physics.Raycast(new Vector3(track.points[70].x,7,-350),Vector3.down,out var under,6,1)&&under.point.y<3.5f,"Lower street has separate ground beneath flyover");
   Check(track.CityStreet(new Vector3(80,3.6f,100)),"Side street is recognized as drivable");
   Place(148,-5,.2f);car.body.linearVelocity=track.Forward(148)*20;yield return new WaitForSeconds(.2f);Check(car.wet,"Beach washout applies water drag");
   Place(148,7);yield return new WaitForSeconds(.2f);Check(!car.wet,"Dry lane bypasses washout");
   Place(177,-4,.2f);yield return new WaitForSeconds(.12f);Check(car.wet,"Second beach washout applies water drag");
   Place(36);yield return new WaitForSeconds(.12f);Check(car.boost>1,"Booster works on flyover approach");Place(101);yield return new WaitForSeconds(.12f);Check(car.body.linearVelocity.y>4,"Jump works on flyover descent");
   Place(2);car.enforceTrackBounds=false;
   var cart=all.First(p=>p.kind==CityPropKind.Cart);Vector3 cartPos=cart.transform.position;
   yield return Shot("01-market-before",cartPos+cart.transform.forward*-12+Vector3.up*6,cartPos+Vector3.up*1.1f,54);
   // Drive the real car into the stand. This validates collision callbacks and fracture.
   car.body.position=cartPos-cart.transform.forward*8+Vector3.up*.35f;car.body.rotation=cart.transform.rotation;car.body.linearVelocity=cart.transform.forward*26;car.throttle=1;Physics.SyncTransforms();int before=CityPhysics.TotalBroken;
   yield return new WaitForSeconds(.65f);car.throttle=0;Check(CityPhysics.TotalBroken>before,"Vehicle impact fractures cart or nearby crate");
   yield return Shot("02-market-impact",cartPos+new Vector3(-10,7,-10),cartPos+Vector3.up,58);
   var tree=all.First(p=>p&&p.kind==CityPropKind.Tree);var treePos=tree.transform.position;float treeUp=Vector3.Dot(tree.transform.up,Vector3.up);RaceEffects.Explosion(treePos+Vector3.right*3);yield return new WaitForSeconds(.6f);
   Check(tree.broken&&!tree.body.isKinematic&&tree.body.angularVelocity.magnitude>.1f,"Explosion releases anchored tree with physical angular motion");
   yield return Shot("03-tree-explosion",treePos+new Vector3(-12,7,-12),treePos+Vector3.up*3);
   var rock=all.First(p=>p&&p.kind==CityPropKind.Rock);before=CityPhysics.TotalBroken;var rockPos=rock.transform.position;RaceEffects.Explosion(rockPos+Vector3.right);yield return new WaitForSeconds(.2f);Check(CityPhysics.TotalBroken>before&&FindObjectsByType<CityDebrisLifetime>().Length>0,"Rock blast produces bounded colliding fragments");
   var loose=FindObjectsByType<CityProp>().First(p=>p.kind==CityPropKind.Loose&&!p.broken);var loosePos=loose.transform.position;CityPhysics.Blast(loosePos-Vector3.right,8,14);yield return new WaitForSeconds(.15f);Check(loose.body.linearVelocity.magnitude>1,"Explosion impulse moves loose objects");
   var pulseProp=FindObjectsByType<CityProp>().First(p=>p.kind==CityPropKind.Loose&&!p.broken);car.body.position=pulseProp.transform.position+Vector3.right*3+Vector3.up;car.body.linearVelocity=Vector3.zero;Physics.SyncTransforms();car.item="Pulse";car.UseItem();yield return new WaitForSeconds(.12f);Check(pulseProp.body.linearVelocity.magnitude>1,"Pulse power-up pushes city props");
   // Check the real missile path, including scenery impact and prop blast propagation.
   cart=FindObjectsByType<CityProp>().First(p=>p.kind==CityPropKind.Cart);var missile=RaceEffects.Missile();missile.transform.position=cart.transform.position-cart.transform.forward*4+Vector3.up*1.2f;missile.transform.rotation=cart.transform.rotation;var rocket=missile.AddComponent<Rocket>();rocket.owner=car;rocket.game=game;rocket.direction=cart.transform.forward;int impacts=Rocket.ImpactCount;yield return new WaitForSeconds(.4f);Check(Rocket.ImpactCount>impacts,"Missile collides with city geometry and explodes");
   Check(CityPhysics.DebrisCount<=160,"Physics debris budget is bounded");
   foreach(var v in game.racers){v.body.linearVelocity=Vector3.zero;v.manualTest=true;v.throttle=0;}
   yield return Shot("04-city-overview",new Vector3(-710,390,-700),new Vector3(-30,0,20),64);
   yield return Shot("05-flyover",track.points[67]+new Vector3(-80,47,-65),track.points[67]+new Vector3(20,-4,20),60);
   yield return Shot("06-under-flyover",new Vector3(110,6,-351),new Vector3(240,8,-349),68);
   yield return Shot("07-beach-washout",track.points[149]+new Vector3(-35,19,-32),track.points[152]+Vector3.right*12,62);
   yield return Shot("08-market-street",track.points[364]-track.Forward(364)*14+Vector3.up*6,track.points[367]+Vector3.up*2,68);
   yield return Shot("09-neighborhood",new Vector3(-360,32,310),new Vector3(-170,6,245),70);
   Check(FindObjectsByType<CityProp>().All(p=>float.IsFinite(p.transform.position.x)&&p.body&&p.body.linearVelocity.magnitude<250),"All prop positions and speeds remain finite after impacts");
   game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);car=game.player;car.player=false;float start=Time.realtimeSinceStartup;int frames=0;float seconds=0;bool captured=false;
   while(!car.finished&&Time.realtimeSinceStartup-start<200){frames++;seconds+=Time.unscaledDeltaTime;if(!captured&&car.progress>58){captured=true;ScreenCapture.CaptureScreenshot(Path.Combine(evidence,"10-racing-flyover.png"));}yield return null;}
   Check(car.finished,"AI completes full city circuit: progress="+car.progress+" returns="+car.automaticReturns);
   Check(game.racers.All(v=>float.IsFinite(v.body.position.x)&&float.IsFinite(v.speed)),"20 racers remain finite");
   Debug.Log("CITY PERFORMANCE fps="+(frames/Mathf.Max(.001f,seconds)).ToString("F1")+" finishers="+game.racers.Count(v=>v.finished));
   // Restart must rebuild destroyed props and remove debris from the previous race.
   game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);Check(FindObjectsByType<CityProp>().Count(p=>p.kind==CityPropKind.Cart)>20,"Restart restores all market carts");Check(FindObjectsByType<CityDebrisLifetime>().Length==0,"Restart removes previous city debris");
   Debug.Log("CITY COMPLETE failures="+failures);File.WriteAllText(Path.Combine(evidence,"test-result.txt"),"failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
