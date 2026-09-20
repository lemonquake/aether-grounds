using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Aether {
 public sealed class GameplayAuditTest:MonoBehaviour {
  Game game;int failures,checks;float elapsed;string evidence;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherGameplayAuditTest"))new GameObject("Gameplay audit verification").AddComponent<GameplayAuditTest>();}
  void Check(bool condition,string label){checks++;if(!condition)failures++;Debug.Log("GAMEPLAY_AUDIT "+(condition?"PASS ":"FAIL ")+label);}
  void Update(){elapsed+=Time.unscaledDeltaTime;if(elapsed>240){Debug.LogError("GAMEPLAY_AUDIT TIMEOUT");Application.Quit(2);}}
  void Move(Vehicle car,Vector3 position,Quaternion rotation,Vector3 velocity){car.transform.SetPositionAndRotation(position,rotation);car.body.position=position;car.body.rotation=rotation;car.body.linearVelocity=velocity;car.body.angularVelocity=Vector3.zero;Physics.SyncTransforms();car.ResetFinishSample();}
  IEnumerator Shot(string label){
   if(Environment.GetCommandLineArgs().Contains("-batchmode"))yield break;
   yield return new WaitForEndOfFrame();
   // Hidden Windows players do not render the swapchain. Explicit camera renders
   // verify the 3D view only; they intentionally do not claim to capture IMGUI.
   var target=new RenderTexture(1280,720,24);target.Create();var prior=game.cam.targetTexture;var active=RenderTexture.active;
   game.cam.targetTexture=target;game.cam.Render();RenderTexture.active=target;
   var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
   File.WriteAllBytes(Path.Combine(evidence,label+"-world.png"),pixels.EncodeToPNG());
   game.cam.targetTexture=prior;RenderTexture.active=active;target.Release();Destroy(target);Destroy(pixels);
  }
  IEnumerator Start(){
   evidence=Path.GetFullPath(Application.dataPath+"/../../Evidence/GameplayAudit");Directory.CreateDirectory(evidence);
   var run=new DrivingSkillRun();run.Add("Drift",100);run.Tick(3,true);
   Check(run.score==0&&run.pending==100&&run.remaining==1,"Chain waits four active seconds before banking");
   run.Tick(0,false);Check(run.pending==100&&run.cleanTime==3,"Zero elapsed time preserves chain and clean progress");
   run.Tick(1,true);Check(run.score==100&&run.pending==0&&run.bestChain==100,"Timeout banks once and records best chain");
   run.Tick(10,true);Check(run.score==100,"Idle timeout cannot duplicate banked points");
   for(int i=0;i<10;i++)run.Add("Drift",100);
   Check(run.Multiplier==3&&run.pending==2100,"Multiplier ramps to three and stays capped");
   run.Break();Check(run.score==100&&run.pending==0&&run.cleanTime==0,"Crash loses pending chain but preserves banked score");
   for(int i=0;i<200;i++)run.Add("Landing",400);run.Bank();
   Check(run.score==3100&&run.bestChain==3000,"Chain score cap prevents runaway multipliers");
   run.drifts=3;run.passes=3;run.landings=2;run.Tick(20,true);
   Check(run.Goals(false,false,false)==3&&run.Goals(false,true,false)==3&&run.Goals(true,false,false)==3,"Each mode has achievable three-goal completion");
   Check(run.FinishBonus(false,false,false)==600,"Driving reward capped at 600 credits");
   run.Break();Check(run.bestCleanTime>=20&&run.Goals(false,false,false)==3,"Completed goals survive later mistakes");

   yield return new WaitForSecondsRealtime(.5f);game=FindAnyObjectByType<Game>();game.ChooseDevice(false);
   game.mapIndex=2;game.aiCount=1;game.laps=1;game.rushMode=game.stuntMode=false;game.StartRace();
   yield return new WaitUntil(()=>game.state==Game.State.Race);
   foreach(var racer in game.racers){racer.manualTest=true;racer.enforceTrackBounds=false;racer.throttle=0;}
   var car=game.player;var bot=game.racers[1];
   car.empFor=2;car.energy=10;car.throttle=1;car.UpdateBoostEnergy(true,1);
   Check(car.energy==10&&car.boost==0,"EMP blocks propulsion without wasting remaining energy");
   car.item="Boost";int uses=car.stats.itemsUsed;car.UseItem();
   Check(car.item=="Boost"&&car.stats.itemsUsed==uses,"EMP retains unusable Boost item");
   car.empFor=0;car.frozenFor=1;car.energy=40;car.UpdateBoostEnergy(true,1);
   Check(car.energy==40&&car.boost==0,"Freeze does not drain held boost");
   car.frozenFor=0;car.UseItem();Check(car.item==""&&car.boost>3,"Retained item works after disable ends");
   car.boost=0;car.energy=50;car.UpdateBoostEnergy(true,1);
   Check(Mathf.Abs(car.energy-23)<.01f&&car.boost>0,"Normal held boost still drains expected energy");
   string engine=game.inventory.engine;game.inventory.engine="storm-coil";car.energy=50;car.UpdateBoostEnergy(true,1);
   Check(Mathf.Abs(car.energy-28.4f)<.01f,"Storm Coil retains 20 percent energy savings");game.inventory.engine=engine;

   bool soloPool=true;game.racers.Remove(bot);
   for(int i=0;i<100;i++){string item=Pickup.ChooseItem(game,car,i/99f);soloPool&=item=="Boost"||item=="Shield";}
   Check(soloPool,"Solo pickup pool contains useful Boost and Shield only");game.racers.Add(bot);
   Check(Pickup.ChooseItem(game,car,.25f)=="Rocket","Rival present retains circuit offensive items");
   bot.finished=true;Check(Pickup.ChooseItem(game,car,.25f)=="Boost","Finished rivals do not force offensive solo items");bot.finished=false;
   game.rushMode=true;int offensive=0;for(int i=0;i<10;i++){string item=Pickup.ChooseItem(game,car,(i+.1f)/10);if(item!="Boost"&&item!="Shield")offensive++;}
   Check(offensive==8,"Rush keeps its 80 percent offensive pool with rivals");game.rushMode=false;

   var coin=game.track.coins[0];car.progress=TrackWorld.Count-1;car.completedLaps=0;
   Check(coin.Collect(car),"Coin available on first lap");car.progress=TrackWorld.Count+1;
   Check(!coin.Collect(car),"Crossing progress boundary without validated lap cannot refill coins");car.completedLaps=1;
   Check(coin.Collect(car),"A validated lap refills its coins");car.completedLaps=0;car.progress=0;

   Quaternion heading=Quaternion.LookRotation(game.track.Forward(20));Vector3 origin=game.track.points[20]+Vector3.up*.6f;
   car.boost=0;car.shield=0;car.stun=car.oilFor=0;car.grounded=4;car.drift=true;car.steer=.5f;
   Move(car,origin,heading,heading*Vector3.forward*25);
   Check(!car.ControlledDrift,"Holding drift on a straight is not a controlled slide");
   car.body.linearVelocity=heading*new Vector3(5,0,25);Check(car.ControlledDrift,"Forward lateral slide with steering qualifies");
   car.body.linearVelocity=heading*new Vector3(20,0,-10);Check(!car.ControlledDrift,"Reverse donut does not qualify");
   car.skills.Add("Pending",200);car.boost=3;int resets=car.stats.resets;car.Recover();
   Check(car.skills.pending==0&&car.boost==0&&car.stats.resets==resets+1,"Recovery clears chain and propulsion, counting every reset");

   // End-to-end FixedUpdate sample: a real road-supported controlled slide.
   car.skills=new DrivingSkillRun();car.drift=true;car.steer=.45f;car.throttle=0;
   for(int i=0;i<250;i++){
    Move(car,origin,heading,heading*new Vector3(5,0,25));
    yield return new WaitForFixedUpdate();
   }
   Check(car.skills.drifts>=1&&car.skills.pending>0,"Physics driving sample feeds controlled-drift chain; drifts="+car.skills.drifts+" grounded="+car.grounded);
   int pending=car.skills.pending;float remaining=car.skills.remaining;game.state=Game.State.Pause;Time.timeScale=0;
   yield return new WaitForSecondsRealtime(.25f);
   Check(car.skills.pending==pending&&car.skills.remaining==remaining,"Pause freezes actual vehicle chain timers");
   yield return Shot("driving-goals-pause");Time.timeScale=1;game.state=Game.State.Race;
   car.InterruptDrivingSkills();car.drift=false;car.steer=0;
   for(int i=0;i<110;i++){Move(car,origin,heading,heading*Vector3.forward*25);yield return new WaitForFixedUpdate();}
   Move(bot,origin+heading*new Vector3(4,0,10),heading,heading*Vector3.forward*18);
   yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
   Move(bot,origin+heading*new Vector3(4,0,-10),heading,heading*Vector3.forward*18);
   yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
   Check(car.skills.passes==1,"Clean forward pass detected through physics sampling");
   Move(bot,origin+heading*new Vector3(4,0,10),heading,heading*Vector3.forward*18);yield return new WaitForFixedUpdate();
   Move(bot,origin+heading*new Vector3(4,0,-10),heading,heading*Vector3.forward*18);yield return new WaitForFixedUpdate();
   Check(car.skills.passes==1,"Same rival cannot be farmed twice on one lap");
   // A launch must begin on road and end with stable forward road contact.
   car.InterruptDrivingSkills();
   for(int i=0;i<120;i++){Move(car,origin,heading,heading*Vector3.forward*25);yield return new WaitForFixedUpdate();}
   for(int i=0;i<90;i++){Move(car,origin+Vector3.up*4+heading*Vector3.forward*(i*.2f),heading,heading*Vector3.forward*25);yield return new WaitForFixedUpdate();}
   for(int i=0;i<35;i++){Move(car,origin+heading*Vector3.forward*20,heading,heading*Vector3.forward*25);yield return new WaitForFixedUpdate();}
   Check(car.skills.landings==1,"Forward jump earns one landing after stable road contact; landings="+car.skills.landings);
   int landed=car.skills.landings;
   for(int i=0;i<90;i++){Move(car,origin+Vector3.up*4,heading,heading*Vector3.forward*25);yield return new WaitForFixedUpdate();}
   car.Recover();yield return new WaitForFixedUpdate();
   Check(car.skills.landings==landed,"Recovery from flight cannot manufacture a landing");
   yield return Shot("driving-skills-hud");game.phoneMode=true;yield return Shot("driving-skills-phone");game.phoneMode=false;

   // Successful finish is the sole credit settlement path.
   car.skills=run;int before=game.credits;car.FinishRace(60);int after=game.credits;
   Check(game.skillAward==600&&after-before>=600,"Successful finish settles capped skill bonus");
   car.FinishRace(61);game.OnFinish(car);Check(game.credits==after,"Repeated finish cannot duplicate driving rewards");
   yield return new WaitUntil(()=>game.state==Game.State.Results);game.RevealResultStats();yield return Shot("driving-results");
   game.StartRush(0);yield return new WaitUntil(()=>game.state==Game.State.Race);
   Check(game.player.skills.score==0&&game.player.skills.pending==0&&game.skillAward==0,"Restart creates fresh goals, chain and bonus");
   before=game.credits;game.player.skills.score=5000;game.raceTime=game.Challenge.seconds;yield return null;yield return null;
   Check(game.rushFailed&&game.skillAward==0&&game.credits==before,"Rush timeout grants no driving credits");
   game.stuntAiCount=0;game.StartStunts();
   yield return new WaitUntil(()=>game.state==Game.State.Race);
   game.player.manualTest=true;game.player.stats.resets=2;game.player.automaticReturns=0;
   before=game.credits;game.player.FinishRace(100);
   Check(game.credits-before==1700,"Stunts penalizes manual resets using the same 50-credit rate");
   game.MainMenu();
   File.WriteAllText(Path.Combine(evidence,"test-result.txt"),"checks="+checks+" failures="+failures);
   Debug.Log("GAMEPLAY_AUDIT COMPLETE checks="+checks+" failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}

