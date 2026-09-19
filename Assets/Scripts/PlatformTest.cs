using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Aether {
 public class PlatformTest:MonoBehaviour {
  Game game;int failures;float watchdog;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherPlatformTest"))new GameObject("Platform verification").AddComponent<PlatformTest>();}
  void Check(bool condition,string message){Debug.Log("PLATFORM "+(condition?"PASS ":"FAIL ")+message);if(!condition)failures++;}
  void Update(){watchdog+=Time.unscaledDeltaTime;if(watchdog>160)Application.Quit(2);}
  IEnumerator Start(){
   yield return new WaitForSeconds(1);game=FindAnyObjectByType<Game>();
   game.launchScreen=Game.LaunchScreen.DeviceChoice;game.HandleBack();Check(game.launchScreen==Game.LaunchScreen.Splash,"Device choice Back returns to splash");game.ChooseDevice(true);Check(game.phoneMode&&game.launchScreen==Game.LaunchScreen.Ready,"Phone choice opens main menu");
   game.menu="Controls";game.HandleBack();Check(game.menu=="Play","Android Back from Controls returns to main menu");game.HandleBack();Check(game.launchScreen==Game.LaunchScreen.DeviceChoice,"Main menu Back returns to device choice without exiting");game.ChooseDevice(true);
   game.ApplyPhoneContacts(new[]{new PhoneContact(1,new Vector2(355,693),true),new PhoneContact(2,new Vector2(1440,740),true),new PhoneContact(3,new Vector2(1230,650),true),new PhoneContact(4,new Vector2(1440,550),true)});
   Check(game.touchSteer>.8f&&game.touchThrottle==1&&game.touchDrift&&game.touchBoost,"Four simultaneous touches: right, accelerate, drift and boost");
   game.ApplyPhoneContacts(new[]{new PhoneContact(2,new Vector2(1240,790),false)});Check(game.touchSteer==0&&game.touchThrottle==-1&&!game.touchBoost,"Released joystick clears and brake/reverse remains independent");game.ClearPhoneInput();
   game.ChooseDevice(false);game.mapIndex=0;game.aiCount=0;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);
   game.HandleBack();Check(game.state==Game.State.Pause&&Time.timeScale==0,"Back pauses a live race");game.HandleBack();Check(game.state==Game.State.Race&&Time.timeScale==1,"Back resumes pause");
   var car=game.player;var track=game.track;car.manualTest=true;car.throttle=0;
   car.body.position=track.points[70]+Vector3.up*2.2f;car.body.rotation=Quaternion.LookRotation(track.Forward(70));car.body.linearVelocity=car.body.angularVelocity=Vector3.zero;car.lastPoint=70;car.wrongWayTime=car.offTrackTime=0;
   var support=TrackWorld.Cube("Test narrow ledge supporting chassis",track.points[70]+Vector3.up*2.24f,new Vector3(.35f,.4f,2),ModelLibrary.Material("Test ledge",Color.gray),true);support.transform.rotation=car.transform.rotation;Physics.SyncTransforms();
   yield return new WaitForSeconds(1);Check(car.grounded<=1,"Elevated chassis trap leaves wheels unsupported");car.body.rotation=Quaternion.LookRotation(-track.Forward(70));car.wrongWayTime=2;Physics.SyncTransforms();int before=car.trappedReturns;yield return new WaitForSeconds(5.5f);Check(car.trappedReturns==before+1&&track.InRaceCorridor(car.transform.position),"Unsupported stuck car automatically returns to usable road");Destroy(support);
   game.mapIndex=1;game.StartRace();yield return new WaitUntil(()=>game.state==Game.State.Race);car=game.player;car.manualTest=true;car.throttle=0;car.body.constraints=RigidbodyConstraints.FreezeAll;
   var traffic=FindObjectsByType<TricycleTraffic>();string destroyedName=traffic[4].name,displacedName=traffic[7].name;Destroy(traffic[4].gameObject);traffic[7].GetComponent<Rigidbody>().position=new Vector3(650,4,650);Physics.SyncTransforms();
   yield return new WaitForSeconds(18.7f);Check(!GameObject.Find(destroyedName),"Destroyed tricycle waits for the 20-second respawn timer");yield return new WaitForSeconds(3.5f);
   var replacement=GameObject.Find(destroyedName);var displaced=GameObject.Find(displacedName);
   Check(replacement&&replacement.GetComponent<TricycleTraffic>()&&!replacement.GetComponent<CityProp>().broken,"Destroyed tricycle respawns with functioning road AI");
   Check(displaced&&game.track.InRaceCorridor(displaced.transform.position),"Blown-away tricycle returns after 20 seconds");
   if(replacement){var p=replacement.transform.position;yield return new WaitForSeconds(2);Check(Vector3.Distance(p,replacement.transform.position)>1,"Respawned tricycle resumes driving");}
   var dir=Path.GetFullPath(Application.dataPath+"/../../Evidence/Platform");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"test-result.txt"),"failures="+failures);Debug.Log("PLATFORM COMPLETE failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
