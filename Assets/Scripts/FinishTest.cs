using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Aether {
 public class FinishTest:MonoBehaviour {
  Game game;int failures;float watchdog;string evidence;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherFinishTest"))new GameObject("Finish and results verification").AddComponent<FinishTest>();}
  void Check(bool ok,string detail){if(!ok)failures++;Debug.Log("FINISH_TEST "+(ok?"PASS ":"FAIL ")+detail);}
  void Update(){watchdog+=Time.unscaledDeltaTime;if(watchdog>300){Debug.LogError("FINISH_TEST TIMEOUT");Application.Quit(2);}}
  IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(evidence,name+".png"));yield return new WaitForSecondsRealtime(.15f);}
  void Move(Vehicle v,Vector3 nose,Vector3 heading){v.body.rotation=Quaternion.LookRotation(heading);v.body.position=nose-heading*v.GetComponent<BoxCollider>().size.z*.5f;v.body.linearVelocity=Vector3.zero;v.body.angularVelocity=Vector3.zero;Physics.SyncTransforms();}
  void Sweep(Vehicle v,Vector3 origin,Vector3 heading,float side=0,bool reverse=false){var right=Vector3.Cross(Vector3.up,heading);Move(v,origin+right*side+Vector3.up*.6f+heading*(reverse?3:-3),heading);v.ResetFinishSample();Move(v,origin+right*side+Vector3.up*.6f+heading*(reverse?-3:.03f),heading);v.CheckFinishCrossing();}
  void CircuitGates(Vehicle v){for(int i=1;i<=3;i++){int n=i*TrackWorld.Count/4;Sweep(v,game.track.points[n],game.track.Forward(n));}}
  IEnumerator Start(){
   yield return new WaitForSeconds(1);game=FindAnyObjectByType<Game>();evidence=Path.GetFullPath(Application.dataPath+"/../../Evidence/Finish");Directory.CreateDirectory(evidence);
   Check(Vehicle.CrossedFinish(new Vector3(0,1,-20),new Vector3(0,1,20),Vector3.zero,Vector3.forward,12,15,out float t)&&Mathf.Abs(t-.5f)<.001f,"Swept high-speed crossing interpolates time");
   Check(!Vehicle.CrossedFinish(new Vector3(0,1,2),new Vector3(0,1,-2),Vector3.zero,Vector3.forward,12,15,out _),"Reverse crossing rejected");
   Check(!Vehicle.CrossedFinish(new Vector3(20,1,-2),new Vector3(20,1,2),Vector3.zero,Vector3.forward,12,15,out _),"Outside gate crossing rejected");
   for(int mode=0;mode<7;mode++){
    game.aiCount=mode==0?19:4;game.laps=2;game.stuntAiCount=5;
    if(mode<3){game.rushMode=game.stuntMode=false;game.mapIndex=mode;game.StartRace();}else if(mode<6)game.StartRush(mode-3);else game.StartStunts();
    yield return new WaitUntil(()=>game.state==Game.State.Race);foreach(var racer in game.racers){racer.manualTest=true;racer.throttle=racer.steer=0;}
    Vehicle v=game.player;game.raceTime=85.3f;
    Vector3 origin=game.stuntMode?game.track.StuntPosition(TrackWorld.StuntGates[5]):game.rushMode?new Vector3(0,5,game.track.RushEnd):game.track.points[0];Vector3 heading=game.rushMode||game.stuntMode?Vector3.forward:game.track.Forward(0);
    v.progress=0;v.stuntCheckpoint=0;Sweep(v,origin,heading);Check(!v.finished&&v.completedLaps==0,"Mode "+mode+" cannot finish without course progress");
    if(mode==0){for(int section=1;section<=3;section++){int n=section*TrackWorld.Count/4+12;Sweep(v,game.track.points[n],game.track.Forward(n));Check(v.circuitCheckpoint==section,"Returning beyond course gate "+section+" preserves lap eligibility");}
     Move(v,origin-heading*4,heading);v.ResetFinishSample();Move(v,origin+heading*4,heading);v.ResetFinishSample();v.CheckFinishCrossing();Check(v.completedLaps==0&&!v.finished,"Recovery teleport across finish cannot grant a lap");}
    if(mode<3)CircuitGates(v);v.progress=game.rushMode?TrackWorld.Count-3:TrackWorld.Count-3;v.stuntCheckpoint=game.stuntMode?5:0;
    Sweep(v,origin,heading,0,true);Check(!v.finished&&v.completedLaps==0,"Mode "+mode+" reverse crossing cannot finish");Sweep(v,origin,heading,30);Check(!v.finished&&v.completedLaps==0,"Mode "+mode+" side-road crossing cannot finish");
    if(mode<3){Sweep(v,origin,heading);Check(v.completedLaps==1&&!v.finished,"Circuit "+mode+" lap registers at painted line");Sweep(v,origin,heading);Check(v.completedLaps==1&&!v.finished,"Circuit "+mode+" repeated crossing cannot farm lap");CircuitGates(v);v.progress=TrackWorld.Count*2-3;game.raceTime=172.6f;}
    // Real physics sampling of speed, distance, out-of-bounds time and item use.
    Move(v,origin+Vector3.up*8-heading*20,heading);v.ResetFinishSample();v.body.linearVelocity=heading*50;v.enforceTrackBounds=false;v.item="Boost";v.UseItem();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
    Check(v.stats.topSpeed>=175&&v.stats.distance>0&&v.stats.itemsUsed==1,"Mode "+mode+" telemetry measures physical travel and item use");
    var traffic=new GameObject("Telemetry traffic");v.RecordTraffic(traffic.transform);v.RecordTraffic(traffic.AddComponent<RushTrafficBody>());Check(v.stats.trafficCars==1,"Mode "+mode+" traffic hits deduplicate across components");Destroy(traffic);
    if(mode==0){
     Move(v,origin+Vector3.Cross(Vector3.up,heading)*35+Vector3.up,heading);v.ResetFinishSample();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Check(v.stats.outOfBounds>0,"Out-of-bounds telemetry accumulates separately from recovery timer");
     var propObject=GameObject.CreatePrimitive(PrimitiveType.Cube);propObject.transform.position=origin+Vector3.up*12;var prop=propObject.AddComponent<CityProp>();prop.Configure(CityPropKind.Metal,40);prop.Break(origin,Vector3.forward,v);prop.Break(origin,Vector3.forward,v);Check(v.stats.propsDestroyed==1,"Destroyed prop credits its attacker only once");Destroy(propObject);
     var personObject=new GameObject("Finish telemetry resident");personObject.transform.position=origin+Vector3.up*12;var person=personObject.AddComponent<CityPedestrian>();person.Initialize();person.Strike(Vector3.up,v);person.Strike(Vector3.up,v);Check(v.stats.civilians==1,"Civilian hit credits its attacker only once");Destroy(personObject);
     Check(v.circuitCheckpoint==3,"Ordered circuit gates survive a return from outside the road");
    }
    int credits=game.credits;v.progress=game.rushMode?TrackWorld.Count-3:TrackWorld.Count*2-3;v.stuntCheckpoint=game.stuntMode?5:0;v.grounded=0;Sweep(v,origin+(game.stuntMode?Vector3.up*4:Vector3.zero),heading);
    Check(v.finished&&v.finishOrder==1&&game.finishCelebrating,"Mode "+mode+" finishes immediately with front bumper, including airborne stunt finish");Check(game.credits>credits,"Mode "+mode+" finish grants reward");
    float top=v.stats.topSpeed;credits=game.credits;v.FinishRace(999);game.OnFinish(v);Check(credits==game.credits,"Mode "+mode+" duplicate finish cannot grant another reward");
    if(mode==0){yield return Shot("finish-animation");yield return new WaitForSecondsRealtime(1.55f);yield return Shot("placement-reveal");}
    // Cross two opponents after the player to populate genuine silver and bronze finishers.
    for(int i=1;i<=2;i++){var bot=game.racers[i];bot.progress=game.rushMode?TrackWorld.Count-3:TrackWorld.Count*2-3;bot.completedLaps=mode<3?1:0;if(mode<3)CircuitGates(bot);bot.stuntCheckpoint=game.stuntMode?5:0;game.raceTime+=.15f;Sweep(bot,origin,heading);Check(bot.finished&&bot.finishOrder==i+1,"Mode "+mode+" podium place "+(i+1)+" uses actual finisher");}
    yield return new WaitUntil(()=>game.state==Game.State.Results);yield return new WaitForSecondsRealtime(.45f);
    Check(game.ResultStage&&game.ResultStage.PortraitCount==game.racers.Count,"Mode "+mode+" all racers have rendered model portraits");Check(game.ResultStage.Winners.All(w=>w&&w.finished),"Mode "+mode+" all podium cars are finishers");Check(v.stats.topSpeed==top,"Mode "+mode+" player stats freeze at finish");
    if(mode==0)yield return Shot("podium-car-show");game.RevealResultStats();yield return new WaitForSecondsRealtime(1);yield return Shot("results-mode-"+mode);
    int portraitCount=game.ResultStage.PortraitCount;Check(portraitCount==game.racers.Count&&game.ResultStage.Output.IsCreated(),"Mode "+mode+" result render targets remain valid");
   }
   game.StartRush(0);yield return new WaitUntil(()=>game.state==Game.State.Race);game.raceTime=game.Challenge.seconds;yield return null;yield return new WaitForSecondsRealtime(.3f);Check(game.rushFailed&&game.state==Game.State.Results&&!game.player.finished&&game.player.finishOrder==0,"Rush timeout shows failure without false finish or medal");yield return Shot("rush-time-limit");
   game.MainMenu();yield return new WaitForSecondsRealtime(.2f);Check(game.state==Game.State.Menu&&Time.timeScale==1&&!FindAnyObjectByType<ResultsStage>(),"Returning to menu cleans stage and restores normal time");
   File.WriteAllText(Path.Combine(evidence,"test-result.txt"),"failures="+failures);Debug.Log("FINISH_TEST COMPLETE failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
