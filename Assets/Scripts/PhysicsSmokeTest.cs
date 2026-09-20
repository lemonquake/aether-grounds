using System;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Aether {
 public class PhysicsSmokeTest:MonoBehaviour {
  int failures;float diagnosticTimer;
  void Update(){diagnosticTimer+=Time.deltaTime;if(diagnosticTimer>1){diagnosticTimer=0;var g=FindAnyObjectByType<Game>();if(g&&g.player){var v=g.player;Debug.Log("AETHER_DIAG state="+g.state+" manual="+v.manualTest+" throttle="+v.throttle+" speed="+v.speed+" position="+v.body.position+" velocity="+v.body.linearVelocity+" mass="+v.body.mass+" kinematic="+v.body.isKinematic+" constraints="+v.body.constraints+" gear="+v.gear+" rpm="+v.engineRPM);foreach(var w in v.physicalWheels)if(w){w.GetGroundHit(out var hit);Debug.Log("AETHER_WHEEL "+w.name+" motor="+w.motorTorque+" brake="+w.brakeTorque+" rpm="+w.rpm+" grounded="+w.isGrounded+" load="+hit.force+" surface="+(hit.collider?hit.collider.name:"none")+" position="+w.transform.localPosition+" radius="+w.radius);}}}}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Run(){if(Environment.GetCommandLineArgs().Contains("-aetherPhysicsTest"))new GameObject("Physics verification").AddComponent<PhysicsSmokeTest>();}
  void Check(bool passed,string message){if(!passed)failures++;Debug.Log("AETHER_PHYSICS "+(passed?"PASS ":"FAIL ")+message);}
  IEnumerator Start(){
   yield return new WaitForSeconds(2);var game=FindAnyObjectByType<Game>();game.aiCount=0;game.powerups=false;game.laps=1;game.StartRace();
   yield return new WaitUntil(()=>game.state==Game.State.Race);var car=game.player;car.manualTest=true;car.enforceTrackBounds=false;
   // A separate level test surface isolates braking and grip from circuit curvature.
   TrackWorld.Cube("Physics test surface",new Vector3(1600,-.2f,0),new Vector3(120,.4f,1400),ModelLibrary.Material("testground",Color.gray),true);
   car.body.position=new Vector3(1600,.6f,0);car.body.rotation=Quaternion.identity;car.body.linearVelocity=Vector3.zero;car.body.angularVelocity=Vector3.zero;Physics.SyncTransforms();
   yield return new WaitForSeconds(2);car.throttle=1;
   yield return new WaitForSeconds(4);float accelerated=car.speed;Check(accelerated>45,"Acceleration after4s="+accelerated.ToString("F1")+" km/h");Check(car.grounded>=2,"Suspension contact="+car.grounded);
   car.throttle=-1;yield return new WaitForSeconds(1.2f);Check(car.speed<accelerated*.65f,"Brake speed="+car.speed.ToString("F1")+" km/h");
   yield return new WaitForSeconds(2.2f);Check(Vector3.Dot(car.body.linearVelocity,car.transform.forward)<-1,"Reverse speed="+Vector3.Dot(car.body.linearVelocity,car.transform.forward).ToString("F1")+" m/s");
   car.body.position=new Vector3(1600,.3f,0);car.body.rotation=Quaternion.identity;car.body.linearVelocity=Vector3.zero;car.body.angularVelocity=Vector3.zero;Physics.SyncTransforms();car.throttle=1;car.steer=.35f;car.drift=true;yield return new WaitForSeconds(4);Check(float.IsFinite(car.body.linearVelocity.x)&&car.speed>10,"Drift remains stable speed="+car.speed.ToString("F1"));
   car.throttle=0;car.steer=0;car.drift=false;car.Recover();yield return new WaitForFixedUpdate();car.item="Shield";car.UseItem();Check(car.shield>0,"Shield activates");car.Hit(Vector3.right*10);Check(car.shield==0,"Shield absorbs hit");
   car.item="Boost";car.UseItem();Check(car.boost>3,"Boost activates");car.item="Rocket";car.UseItem();Check(FindAnyObjectByType<Rocket>()!=null,"Rocket spawns");
   var targetObject=new GameObject("Collision target");var target=targetObject.AddComponent<Vehicle>();target.Initialize(game,2,false,"Test target",car.transform.position+car.transform.right*8,car.transform.rotation);game.racers.Add(target);target.manualTest=true;
   car.item="Pulse";car.UseItem();yield return new WaitForFixedUpdate();Check(target.body.linearVelocity.magnitude>1,"Pulse pushes opponent");
   car.body.position+=Vector3.up*3;car.body.rotation=Quaternion.Euler(0,0,180);car.Recover();yield return new WaitForFixedUpdate();Check(Vector3.Dot(car.transform.up,Vector3.up)>.99f,"Recovery restores upright orientation");
   var obstacle=TrackWorld.Cube("Collision test wall",car.transform.position+car.transform.forward*7+Vector3.up, new Vector3(8,3,1),ModelLibrary.Material("testwall",Color.gray),true);obstacle.transform.rotation=car.transform.rotation;car.body.linearVelocity=car.transform.forward*15;float before=car.body.linearVelocity.magnitude;
   yield return new WaitForSeconds(.8f);Check(car.body.linearVelocity.magnitude<before*.8f,"Wall collision reduces velocity to="+car.body.linearVelocity.magnitude.ToString("F1"));
   yield return new WaitForSeconds(1);Debug.Log("AETHER_PHYSICS COMPLETE failures="+failures);Application.Quit(failures==0?0:1);
  }
 }
}
