using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Aether {
 [DefaultExecutionOrder(11000)]
 public class NeighborhoodTest:MonoBehaviour {
  Game game;int failures;string output;bool photo;Vector3 eye,look;float watchdog;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherNeighborhoodTest"))new GameObject("Neighborhood verification").AddComponent<NeighborhoodTest>();}
  void Check(bool pass,string name){Debug.Log("NEIGHBORHOOD "+(pass?"PASS ":"FAIL ")+name);if(!pass)failures++;}
  void Update(){watchdog+=Time.unscaledDeltaTime;if(watchdog>350){Debug.LogError("NEIGHBORHOOD timeout");Application.Quit(2);}}
  void LateUpdate(){if(photo){game.cam.transform.position=eye;game.cam.transform.LookAt(look);game.cam.fieldOfView=65;}}
  IEnumerator Shot(string name,Vector3 p,Vector3 target){photo=true;CityTest.HideHUD=true;eye=p;look=target;yield return new WaitForSecondsRealtime(.8f);var rt=new RenderTexture(1600,900,24);game.cam.targetTexture=rt;game.cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());game.cam.targetTexture=null;RenderTexture.active=old;Destroy(image);rt.Release();Destroy(rt);yield return new WaitForSecondsRealtime(.3f);photo=false;CityTest.HideHUD=false;}
  void Place(Vehicle v,int n,bool reverse=false){v.body.position=game.track.points[n]+Vector3.up*.22f;v.body.rotation=Quaternion.LookRotation(game.track.Forward(n)*(reverse?-1:1));v.body.linearVelocity=v.body.angularVelocity=Vector3.zero;v.lastPoint=n;v.offTrackTime=v.wrongWayTime=0;Physics.SyncTransforms();}
  IEnumerator Start(){
   yield return new WaitForSeconds(1);output=Path.GetFullPath(Application.dataPath+"/../../Evidence/Malasugue");Directory.CreateDirectory(output);game=FindAnyObjectByType<Game>();game.mapIndex=1;game.aiCount=19;game.laps=1;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);
   var track=game.track;var car=game.player;
   Check(!track.GetComponentsInChildren<TextMesh>().Any(t=>t.text.Contains("Race route"))&&track.GetComponentInChildren<RoadGuideLights>().ArrowCount>=240,"Direction comes from pulsing road lights, with no route text signs");
   Check(TrackWorld.Names[0]=="Downtown Minda"&&TrackWorld.Names[1]=="Malasugue Town","Map names");
   Check(track.neighborhood.buildings.Length>500&&track.length>1800&&track.length<2000,"Mapped footprints and measured 1.87 km circuit");
   Check(track.NeighborhoodStreet(track.neighborhood.destination)&&TrackWorld.FlatDistance(track.neighborhood.residence,track.points[track.Nearest(track.neighborhood.residence)])<25,"Both supplied endpoints included");
   Check(GameObject.Find("Leodones-Olivar Residence")!=null,"Residence landmark exists");
   Check(game.racers.Count==20&&track.coins.Count>45&&track.pickups.Count>5,"20-car grid, coins and pickups");
   var traffic=FindObjectsByType<TricycleTraffic>();Check(traffic.Length>=24&&traffic.All(t=>t.GetComponent<Rigidbody>()&&t.GetComponent<Collider>()),"Tricycle traffic has colliding rigidbodies");
   Check(!track.Broken(150)&&!track.InWater(track.points[150],150),"No inherited canyon water on mapped roads");
   int obstacles=0;for(int n=0;n<TrackWorld.Count;n+=2){if(Physics.CheckBox(track.points[n]+Vector3.up*1.3f,new Vector3(.8f,.45f,.65f),Quaternion.LookRotation(track.Forward(n)),1,QueryTriggerInteraction.Ignore)){obstacles++;Debug.Log("NEIGHBORHOOD lane obstruction "+n);}}
   Check(obstacles==0,"Mapped centerline clear of static obstacles: "+obstacles);
   var before=traffic.Select(t=>t.transform.position).ToArray();yield return new WaitForSeconds(3);Check(traffic.Where((t,i)=>Vector3.Distance(t.transform.position,before[i])>2).Count()>15,"Traffic drives along streets");
   foreach(var v in game.racers){v.manualTest=true;v.throttle=v.steer=0;v.body.constraints=RigidbodyConstraints.FreezeAll;}
   Place(car,63,true);yield return new WaitForSeconds(2);Check(car.wrongWayTime>1.5f&&car.wrongWayTime<2.5f,"Wrong-way timer starts on backwards heading");
   float timer=car.wrongWayTime;game.state=Game.State.Pause;Time.timeScale=0;yield return new WaitForSecondsRealtime(.7f);Check(Mathf.Abs(car.wrongWayTime-timer)<.02f,"Pause freezes wrong-way timer");Time.timeScale=1;game.state=Game.State.Race;
   car.body.rotation=Quaternion.LookRotation(track.Forward(63));Physics.SyncTransforms();yield return new WaitForSeconds(.15f);Check(car.wrongWayTime==0,"Correcting direction cancels return");
   Place(car,63,true);int returns=car.automaticReturns;yield return new WaitForSeconds(4.65f);Check(car.automaticReturns==returns&&car.wrongWayTime>4.3f,"No teleport before five seconds");
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"wrong-way-countdown.png"));yield return new WaitForSeconds(.75f);
   Check(car.automaticReturns==returns+1&&car.wrongWayReturns==1,"Five-second automatic wrong-way return");Check(Vector3.Dot(car.transform.forward,track.Forward(track.Nearest(car.transform.position)))>.98f&&car.body.linearVelocity.magnitude<.1f,"Return faces correct direction and stops velocity");
   Check(Vehicle.IsWrongWay(Vector3.forward,Vector3.back*5,Vector3.forward)&&!Vehicle.IsWrongWay(Vector3.forward,Vector3.right*8,Vector3.forward),"Reverse travel detected; sideways drift is not wrong-way");
   car.body.position=new Vector3(680,3.6f,680);Physics.SyncTransforms();returns=car.automaticReturns;yield return new WaitForSeconds(5.3f);Check(car.automaticReturns==returns+1&&car.offTrackTime==0,"Five-second out-of-bounds return");
   var trike=traffic.First(t=>t&&!t.GetComponent<CityProp>().broken);var tb=trike.GetComponent<Rigidbody>();trike.enabled=false;tb.position=new Vector3(600,15,0);tb.linearVelocity=Vector3.zero;Physics.SyncTransforms();var trikeStart=tb.position;tb.AddForce(Vector3.right*7,ForceMode.VelocityChange);yield return new WaitForSeconds(.4f);Check(tb.position.x-trikeStart.x>1,"Tricycle rigidbody responds to an isolated impact impulse");tb.position=track.points[90]+track.Right(90)*1.65f+Vector3.up*.2f;tb.linearVelocity=Vector3.zero;trike.enabled=true;Physics.SyncTransforms();
   var crate=FindObjectsByType<CityProp>().First(p=>p.kind==CityPropKind.Timber);int broken=CityPhysics.TotalBroken;crate.Break(crate.transform.position,Vector3.right*8);yield return new WaitForSeconds(.2f);Check(CityPhysics.TotalBroken>broken&&CityPhysics.DebrisCount>0,"Wooden crates fracture into colliding pieces");
   foreach(var v in game.racers){v.enforceTrackBounds=false;v.gameObject.SetActive(false);}foreach(var indicator in FindObjectsByType<LineRenderer>())if(indicator.name=="Return to track countdown")indicator.gameObject.SetActive(false);
   var home=track.neighborhood.residence;
   yield return Shot("01-town-overview",new Vector3(-570,620,-620),new Vector3(0,0,0));
   yield return Shot("02-residence",home+new Vector3(-13,5,-18),home+Vector3.up*2);
   yield return Shot("03-maya-maya",track.points[38]-track.Forward(38)*15+Vector3.up*5,track.points[43]+Vector3.up*2);
   yield return Shot("04-tricycle",tb.position-tb.transform.forward*6+tb.transform.right*5+Vector3.up*2.8f,tb.position+Vector3.up);
   yield return Shot("05-bolcan",track.points[180]-track.Forward(180)*14+Vector3.up*5,track.points[185]+Vector3.up*2);
   game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);car=game.player;car.player=false;float started=Time.realtimeSinceStartup;int frames=0;
   while(!car.finished&&Time.realtimeSinceStartup-started<190){frames++;yield return null;}
   Check(car.finished,"AI completes neighborhood circuit: progress="+car.progress+" returns="+car.automaticReturns);Check(game.racers.All(v=>float.IsFinite(v.speed)&&float.IsFinite(v.transform.position.x)),"All racers remain finite");Debug.Log("NEIGHBORHOOD average fps="+(frames/(Time.realtimeSinceStartup-started)));
   game.mapIndex=0;game.aiCount=0;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);Check(game.track.name=="Downtown Minda","First city still loads");
   var label=game.track.GetComponentsInChildren<TextMesh>().First(t=>t.text=="Downtown Minda");Check(Vector3.Dot(label.transform.forward,label.transform.parent.forward)>.99f,"City structure text faces approaching camera");
   yield return Shot("06-downtown-sign",game.track.points[0]-game.track.Forward(0)*26+Vector3.up*7,game.track.points[0]+Vector3.up*7);
   File.WriteAllText(Path.Combine(output,"test-result.txt"),"failures="+failures);Debug.Log("NEIGHBORHOOD COMPLETE failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
