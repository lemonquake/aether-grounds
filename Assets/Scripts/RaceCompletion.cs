using System.Collections.Generic;
using UnityEngine;

namespace Aether {
 [System.Serializable] public sealed class RaceStats {
  public float topSpeed, distance, outOfBounds, airtime, driftTime, boostTime, bestLap;
  public int propsDestroyed, civilians, trafficCars, itemsUsed, collisions, resets;
 }
 public partial class Vehicle {
  public RaceStats stats=new RaceStats();
  public int completedLaps, finishOrder, circuitCheckpoint;
  public readonly List<float> lapTimes=new List<float>();
  readonly HashSet<GameObject> struckTraffic=new HashSet<GameObject>();
  Vector3 finishPrevious;bool finishSampleReady;float lapStarted;
  public void ResetFinishSample(){finishPrevious=FinishNose();finishSampleReady=true;}
  Vector3 FinishNose(){var box=GetComponent<BoxCollider>();return body.position+body.rotation*(Vector3.forward*(box?box.size.z*.5f:2));}
  // A swept plane test catches crossings between physics steps, at any speed.
  public static bool CrossedFinish(Vector3 from,Vector3 to,Vector3 origin,Vector3 forward,float halfWidth,float maxHeight,out float fraction){
   forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;
   float a=Vector3.Dot(from-origin,forward),b=Vector3.Dot(to-origin,forward);fraction=0;
   if(a>=0||b<0||b-a<.00001f)return false;
   fraction=-a/(b-a);var hit=Vector3.Lerp(from,to,fraction)-origin;
   return Mathf.Abs(Vector3.Dot(hit,Vector3.Cross(Vector3.up,forward)))<=halfWidth&&hit.y>=-2&&hit.y<=maxHeight;
  }
  void RaceTelemetry(){
   if(finished)return;float dt=Time.fixedDeltaTime;
   stats.topSpeed=Mathf.Max(stats.topSpeed,speed);stats.distance+=body.linearVelocity.magnitude*dt;
   if(!track.InRaceCorridor(body.position))stats.outOfBounds+=dt;
   if(grounded<2&&speed>10)stats.airtime+=dt;
   if(drift&&grounded>=2&&speed>28)stats.driftTime+=dt;
   if(boost>0)stats.boostTime+=dt;
  }
  public void CheckFinishCrossing(){
   Vector3 nose=FinishNose();if(!finishSampleReady){ResetFinishSample();return;}
   var previous=finishPrevious;finishPrevious=nose;if(finished)return;
   // Visit three ordered course sections to prove a lap. A wide section also
   // accepts returning to the road just beyond its gate after a jump or recovery.
   // The finish itself always requires the precise forward plane crossing.
   if(!track.isRush&&!track.isStunts&&circuitCheckpoint<3){int gate=(circuitCheckpoint+1)*TrackWorld.Count/4;
    int near=track.Nearest(nose);bool section=near>=gate&&near<gate+TrackWorld.Count/4-4&&Vector3.Dot(nose-previous,track.Forward(near))>0&&track.InRaceCorridor(nose);
    if(section||CrossedFinish(previous,nose,track.points[gate],track.Forward(gate),track.map==1?5.3f:12.65f,20,out _))circuitCheckpoint++;
   }
   Vector3 origin=track.isStunts?track.StuntPosition(TrackWorld.StuntGates[5]):track.isRush?new Vector3(0,5,track.RushEnd):track.points[0];
   Vector3 forward=track.isRush||track.isStunts?Vector3.forward:track.Forward(0);
   bool eligible=track.isStunts?stuntCheckpoint==5:track.isRush?progress>=TrackWorld.Count-20:circuitCheckpoint==3;
   if(!eligible||!CrossedFinish(previous,nose,origin,forward,track.isStunts?12:track.isRush?12:track.map==1?4.6f:12,track.isStunts?35:15,out float t))return;
   float at=Mathf.Max(0,game.raceTime-(1-t)*Time.fixedDeltaTime);
   if(track.isStunts){stuntCheckpoint=6;progress=460;}
   else if(!track.isRush){completedLaps++;circuitCheckpoint=0;progress=completedLaps*TrackWorld.Count;float lap=at-lapStarted;lapStarted=at;lapTimes.Add(lap);stats.bestLap=stats.bestLap<=0?lap:Mathf.Min(stats.bestLap,lap);if(completedLaps<game.laps){if(player)game.LapCrossed(completedLaps,lap);return;}}
   FinishRace(at);
  }
  public void FinishRace(float at){if(finished)return;if(game.rushMode&&at>game.Challenge.seconds)return;finished=true;finishTime=at;throttle=steer=0;boost=0;drift=false;game.OnFinish(this);}
  public void RecordTraffic(Component target){if(!finished&&game.RaceSimulationActive&&target&&struckTraffic.Add(target.gameObject))stats.trafficCars++;}
  public bool CountingStats=>game&&game.RaceSimulationActive&&!finished;
 }
}
