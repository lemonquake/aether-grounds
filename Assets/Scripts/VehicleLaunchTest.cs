using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Aether {
 public sealed class VehicleLaunchTest:MonoBehaviour {
  int failures;float began;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherLaunchTest"))new GameObject("Vehicle launch regression").AddComponent<VehicleLaunchTest>();}
  void Check(bool ok,string message){Debug.Log("LAUNCH "+(ok?"PASS ":"FAIL ")+message);if(!ok)failures++;}
  void Update(){if(began>0&&Time.realtimeSinceStartup-began>360){Debug.Log("LAUNCH TIMEOUT");Application.Quit(2);}}
  IEnumerator Start(){
   began=Time.realtimeSinceStartup;yield return new WaitForSeconds(1);
   var game=FindAnyObjectByType<Game>();game.ChooseDevice(false);game.aiCount=10;game.laps=3;game.powerups=false;
   // Exercise normal countdown, inputs and AI on each real circuit. All ten
   // body types appear among the bots; no artificial velocity is injected.
   for(int map=0;map<4;map++){
    game.rushMode=game.stuntMode=false;game.mapIndex=map;game.StartRace();
    yield return new WaitUntil(()=>game.state==Game.State.Race);
    var player=game.player;player.manualTest=true;player.throttle=1;
    var starts=game.racers.Select(v=>v.body.position).ToArray();var peak=new float[game.racers.Count];
    float stop=Time.time+4;
    while(Time.time<stop){for(int i=0;i<peak.Length;i++)peak[i]=Mathf.Max(peak[i],game.racers[i].speed);yield return new WaitForFixedUpdate();}
    for(int i=0;i<peak.Length;i++){var v=game.racers[i];Check(peak[i]>15&&Vector3.Distance(starts[i],v.body.position)>4,TrackWorld.Names[map]+" "+v.driver+" "+v.spec.name+" peak="+peak[i].ToString("F1")+" km/h moved="+Vector3.Distance(starts[i],v.body.position).ToString("F1")+" m");}
    game.PauseRace();yield return new WaitForSecondsRealtime(.1f);game.HandleBack();player.throttle=1;
    yield return new WaitForSeconds(.5f);Check(player.physicalWheels.All(w=>w.brakeTorque==0),"Service brakes release after resume on "+TrackWorld.Names[map]);
   }
   Debug.Log("LAUNCH COMPLETE failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
