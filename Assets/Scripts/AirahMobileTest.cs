using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Aether {
 public class AirahMobileTest:MonoBehaviour {
  Game game;int failures;string output;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherAirahMobileTest"))new GameObject("Airah mobile and rollover verification").AddComponent<AirahMobileTest>();}
  void Check(bool ok,string message){Debug.Log("AIRAH MOBILE "+(ok?"PASS ":"FAIL ")+message);if(!ok)failures++;}
  IEnumerator Start(){
   output=Path.GetFullPath(Application.dataPath+"/../../Evidence/Airah");yield return new WaitForSeconds(1);game=FindAnyObjectByType<Game>();game.ChooseDevice(true);
   Check(Game.SupportedMobileFrameRate(120,120)==120&&Game.SupportedMobileFrameRate(120,90)==90&&Game.SupportedMobileFrameRate(120,60)==60&&Game.SupportedMobileFrameRate(60,120)==60,"Mobile frame target supports 60 / 90 / 120 Hz with 60 FPS option");
   game.ApplyPhoneContacts(new[]{new PhoneContact(1,new Vector2(355,693),true),new PhoneContact(2,new Vector2(227,580),true),new PhoneContact(3,new Vector2(1230,650),true),new PhoneContact(4,new Vector2(1440,550),true)});
   Check(game.touchSteer==1&&game.touchThrottle==1&&game.touchDrift&&game.touchBoost,"Right + Up + Drift + Boost simultaneous");
   game.ApplyPhoneContacts(new[]{new PhoneContact(1,new Vector2(90,693)),new PhoneContact(2,new Vector2(225,805))});Check(game.touchSteer==-1&&game.touchThrottle==-1&&!game.touchBoost,"Left + Down brake/reverse independent");
   game.ApplyPhoneContacts(new[]{new PhoneContact(1,new Vector2(90,693)),new PhoneContact(2,new Vector2(355,693))});Check(game.touchSteer==0,"Opposing directional buttons cancel");game.ApplyPhoneContacts(new PhoneContact[0]);Check(game.touchSteer==0&&game.touchThrottle==0,"Releasing buttons clears held input");
   game.rushMode=game.stuntMode=false;game.mapIndex=3;game.aiCount=0;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);var car=game.player;car.manualTest=true;car.enforceTrackBounds=false;
   yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"09-mobile-direction-buttons.png"));
   game.PauseRace();Check(game.touchSteer==0&&game.touchThrottle==0,"Pause clears directional input");game.HandleBack();
   var center=new Vector3(3500,1500,0);var pad=TrackWorld.Cube("Rollover test raised object",center-Vector3.up*2,new Vector3(80,4,80),ModelLibrary.Material("Rollover test surface",Color.gray),true);
   void Put(Vector3 p,Quaternion q){car.body.position=p;car.body.rotation=q;car.transform.SetPositionAndRotation(p,q);car.body.linearVelocity=car.body.angularVelocity=Vector3.zero;car.rolloverLock=car.shield=car.stun=0;Physics.SyncTransforms();}
   int before=car.flipRecoveries;car.body.useGravity=false;Put(center+Vector3.up*25,Quaternion.Euler(0,0,180));yield return new WaitForSeconds(2);
   Check(car.flipRecoveries==before&&car.flipSupportedSeconds==0&&Vector3.Dot(car.transform.up,Vector3.up)<-.95f,"Upside-down airborne car does not auto-rotate after two seconds");
   car.body.useGravity=true;Put(center+Vector3.up*1.05f,Quaternion.Euler(0,0,180));yield return new WaitForSeconds(.7f);Check(car.flipRecoveries==before,"Ground contact under one second does not trigger flip recovery");
   float elapsed=0;while(car.flipRecoveries==before&&elapsed<3){elapsed+=Time.fixedDeltaTime;yield return new WaitForFixedUpdate();}
   Check(car.flipRecoveries>before,"Resting upside-down on raised object triggers after one second");yield return new WaitForSeconds(2);Check(Vector3.Dot(car.transform.up,Vector3.up)>.60f,"Car completes grounded righting");
   car.body.useGravity=false;Put(center+Vector3.up*25,Quaternion.Euler(0,0,180));yield return new WaitForSeconds(.15f);before=car.flipRecoveries;yield return new WaitForSeconds(1.3f);Check(car.flipRecoveries==before&&car.flipSupportedSeconds==0,"Leaving support resets timer and stops automatic righting in air");
   Destroy(pad);game.ChooseDevice(false);car.body.useGravity=true;car.Recover();
   File.WriteAllText(Path.Combine(output,"mobile-result.txt"),"failures="+failures);Debug.Log("AIRAH MOBILE COMPLETE failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
