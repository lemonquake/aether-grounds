using UnityEngine;
namespace Aether {
 public partial class Game {
  // Surface normal keeps the drone above the roof even on a near-vertical descent.
  public void UpdateStuntCamera(){
   var p=player.transform.position;float z=p.z;
   Vector3 tangent=(track.StuntPosition(z+12)-track.StuntPosition(z-12)).normalized;
   Vector3 travel=Vector3.ProjectOnPlane(player.transform.forward,Vector3.up).normalized;
   if(travel.sqrMagnitude<.2f)travel=Vector3.forward;
   bool road= !TrackWorld.StuntGap(z)&&Mathf.Abs(p.y-TrackWorld.StuntHeight(z))<12;
   Vector3 normal=road?Vector3.Cross(tangent,Vector3.right).normalized:Vector3.up;
   float fast=Mathf.Clamp01(player.speed/360),slope=road?Mathf.Clamp01(-tangent.y):0;
   Vector3 desired=p-travel*Mathf.Lerp(11,16,fast)+Vector3.up*7+normal*(7+slope*12);
   // Preview the falling road, but cap aim depth so the player remains in frame.
   Vector3 ahead=road?track.StuntPosition(z+Mathf.Lerp(22,40,fast))-p:travel*18;
   ahead=Vector3.ClampMagnitude(ahead,28);
   Vector3 focus=p+ahead*(tangent.y>0?.18f:.32f)+Vector3.up;
   desired+=player.body.linearVelocity*.065f;
   if(Vector3.Distance(cam.transform.position,desired)>65){cam.transform.position=desired;cameraVel=Vector3.zero;}
   cam.transform.position=Vector3.SmoothDamp(cam.transform.position,desired,ref cameraVel,.065f,600,Time.unscaledDeltaTime);
   // Never lag under the climbing road/roof at the bottom of a descent.
   if(road){float clearance=Vector3.Dot(cam.transform.position-p,normal);if(clearance<6)cam.transform.position+=normal*(6-clearance);}
   cam.transform.rotation=Quaternion.Slerp(cam.transform.rotation,Quaternion.LookRotation(focus-cam.transform.position,Vector3.up),1-Mathf.Exp(-12*Time.unscaledDeltaTime));
   cam.fieldOfView=Mathf.Lerp(cam.fieldOfView,74+fast*5+slope*4+(player.boost>0?3:0),1-Mathf.Exp(-4*Time.unscaledDeltaTime));
  }
 }
 public partial class Vehicle {
  void DriveStuntAI(){
   if(finished){throttle=0;steer=0;drift=false;return;}
   float z=transform.position.z;float aim=TrackWorld.StuntX(z+35);
   float lane=Mathf.Sin(carIndex*2.4f+driver.Length)*5;
   bool launch=z>1320&&z<2090||z>3370&&z<3980;
   if(launch)lane*=.3f;if(z>4300)lane*=.25f;
   foreach(var other in game.racers){if(other==this)continue;Vector3 d=other.transform.position-transform.position;if(d.z>0&&d.z<24&&Mathf.Abs(d.x)<3.3f)lane+=d.x>0?-3.5f:3.5f;}
   foreach(var tile in track.stuntTiles){float dz=tile.transform.position.z-z;if(dz>0&&dz<80&&(tile.kind==StuntTileKind.Trap||tile.kind==StuntTileKind.Slippery||z>4250&&(tile.kind==StuntTileKind.Jump||tile.kind==StuntTileKind.Water)))lane=tile.transform.position.x>TrackWorld.StuntX(tile.transform.position.z)?-5:5;}
   aiLane=Mathf.MoveTowards(aiLane,Mathf.Clamp(lane,-7,7),Time.deltaTime*4);
   Vector3 pathHeading=Vector3.ProjectOnPlane(track.StuntPosition(z+25)-track.StuntPosition(z+5),Vector3.up).normalized;
   float yaw=Vector3.SignedAngle(pathHeading,Vector3.ProjectOnPlane(transform.forward,Vector3.up),Vector3.up);
   float crossSpeed=Vector3.Dot(body.linearVelocity,Vector3.Cross(Vector3.up,pathHeading));
   steer=Mathf.Clamp((aim+aiLane-transform.position.x)*.015f-crossSpeed*.035f-yaw*.02f,-.4f,.4f);
   throttle=grounded>=2?1:Mathf.Clamp(Mathf.Asin(Mathf.Clamp(transform.forward.y,-1,1))*2,-1,1);
   if(z>4280&&grounded>=2&&speed>165)throttle=-.8f;
   if(grounded<2)steer=Mathf.Clamp((aim-transform.position.x)*.003f-yaw*.01f,-.12f,.12f);
   drift=false;energy=Mathf.Min(100,energy+Time.deltaTime*9);
   if(grounded>=2&&z<4250&&!launch&&Mathf.Abs(steer)<.1f&&speed<180&&energy>30){boost=Mathf.Max(boost,.12f);energy-=Time.deltaTime*26;}
   aiItemTimer+=Time.deltaTime;if(aiItemTimer>4&&!launch&&item!=""){UseItem();aiItemTimer=0;}
  }
 }
}
