using UnityEngine;
namespace Aether {
 public partial class TrackWorld {
  // Measure the road corridor against segments, so the gaps between sampled points
  // and legitimate jumps do not create false off-track detections.
  public bool OnRoad(Vector3 position)=>InRaceCorridor(position)||CityStreet(position)||NeighborhoodStreet(position);
  public bool InRaceCorridor(Vector3 position){
   if(isStunts)return StuntCorridor(position);
   if(isRush)return Mathf.Abs(position.x)<=12.65f&&position.z>=-110&&position.z<=length+200&&position.y>2;
   int n=Nearest(position);float best=float.MaxValue;float roadHeight=points[n].y;
   for(int offset=-2;offset<=1;offset++){
    int a=(n+offset+Count)%Count,b=(a+1)%Count;Vector3 edge=points[b]-points[a];Vector3 flat=Vector3.ProjectOnPlane(edge,Vector3.up);
    float t=Mathf.Clamp01(Vector3.Dot(Vector3.ProjectOnPlane(position-points[a],Vector3.up),flat)/Mathf.Max(.01f,flat.sqrMagnitude));
    Vector3 q=Vector3.Lerp(points[a],points[b],t);float d=new Vector2(position.x-q.x,position.z-q.z).sqrMagnitude;
    if(d<best){best=d;roadHeight=q.y;}
   }
   float margin=map==1?5.3f:map==0?12.65f:Width*.5f+.65f;return best<=margin*margin&&position.y>roadHeight-3;
  }
  public void SafeReturnPose(Vehicle car,Game game,out Vector3 position,out Quaternion rotation,out int point){
   if(IsAirah){int n=Nearest(car.transform.position);if(AirahGap(n)||n>=28&&n<=38&&car.transform.position.y<points[n].y-2){point=27;position=points[27]+Right(27)*(car.player?-4:4)+Vector3.up*.8f;rotation=Quaternion.LookRotation(Forward(27));return;}}
   if(isStunts){float z=car.stuntCheckpoint<=0?0:StuntGates[Mathf.Min(car.stuntCheckpoint-1,4)]-12;position=StuntPosition(z)+Vector3.up*.7f;
    for(int row=0;row<5;row++){bool found=false;foreach(float lane in new[]{0f,-4f,4f,-8f,8f}){var candidate=StuntPosition(z-row*6)+Vector3.right*lane+Vector3.up*.7f;bool occupied=false;foreach(var other in game.racers)if(other&&other!=car&&(other.transform.position-candidate).sqrMagnitude<32){occupied=true;break;}if(!occupied){position=candidate;found=true;break;}}if(found)break;}
    point=Nearest(position);rotation=Quaternion.LookRotation(StuntPosition(position.z+1)-StuntPosition(position.z-1));return;}
   int near=Nearest(car.transform.position);if(isRush)near=Mathf.Min(near,Mathf.Max(0,car.rushSafePoint));point=near;position=points[near]+Vector3.up*.65f;rotation=Quaternion.LookRotation(Forward(near));float best=float.MaxValue;
   // Prefer the closest free road location. Avoid water, ramp faces, and other cars.
   for(int offset=-18;offset<=18;offset++){
    int n=isRush?Mathf.Clamp(near+offset,0,Count-1):(near+offset+Count)%Count;if(AirahGap(n))continue;if(isRush&&n>Mathf.Max(0,car.rushSafePoint))continue;bool ramp=false;
    foreach(var tile in tiles)if(tile.kind==TileKind.Ramp&&Mathf.Min((n-tile.point+Count)%Count,(tile.point-n+Count)%Count)<5)ramp=true;
    foreach(float lane in (map==1?new[]{0f,-1.6f,1.6f}:new[]{0f,-6f,6f,-9f,9f})){
     if(Broken(n)&&lane<5)continue;if(ramp&&Mathf.Abs(lane)<6)continue;
     Vector3 candidate=points[n]+Right(n)*lane+Vector3.up*.65f;bool occupied=false;
     foreach(var other in game.racers)if(other&&other!=car&&(other.transform.position-candidate).sqrMagnitude<30){occupied=true;break;}
     if(occupied||Physics.CheckBox(candidate+Vector3.up*.75f,new Vector3(1.15f,.6f,2.5f),Quaternion.LookRotation(Forward(n)),(1<<10)|1,QueryTriggerInteraction.Ignore))continue;float d=(candidate-car.transform.position).sqrMagnitude;if(d<best){best=d;position=candidate;rotation=Quaternion.LookRotation(Forward(n));point=n;}
    }
   }
   // A packed grid still gets a dry, level fallback with brief shield protection.
   if(best==float.MaxValue){position=points[near]+Right(near)*(map==1?1.6f:9)+Vector3.up*.8f;}
  }
 }
 public partial class Vehicle {
  public bool enforceTrackBounds=true;public float offTrackTime,wrongWayTime,trappedTime;public int automaticReturns,wrongWayReturns,trappedReturns;Vector3 motionAnchor;
  public const float WrongWayDelay=5, OffTrackDelay=5;
  public float returnRemaining=>Mathf.Max(0,(offTrackTime>0?OffTrackDelay:WrongWayDelay)-(offTrackTime>0?offTrackTime:wrongWayTime>0?wrongWayTime:trappedTime));
  public bool wrongWayWarning=>wrongWayTime>0&&offTrackTime<=0;
  float returnFlash;GameObject returnIndicator;LineRenderer returnArc;TextMesh returnLabel;
  public static bool IsWrongWay(Vector3 forward,Vector3 velocity,Vector3 tangent){
   forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;tangent=Vector3.ProjectOnPlane(tangent,Vector3.up).normalized;
   return Vector3.Dot(forward,tangent)<-.45f||Vector3.Dot(velocity,tangent)<-2.5f;
  }
  void UpdateTrackBounds(){
   if(!enforceTrackBounds||finished){offTrackTime=wrongWayTime=trappedTime=0;if(returnIndicator)returnIndicator.SetActive(false);return;}
   bool outside=!track.InRaceCorridor(transform.position);
   if(outside){offTrackTime+=Time.deltaTime;wrongWayTime=0;}
   else{
    offTrackTime=0;
    bool wrong=IsWrongWay(transform.forward,body.linearVelocity,track.Forward(track.Nearest(transform.position)));
    if(!wrong)wrongWayTime=0;
    else if(grounded>=2&&Vector3.Dot(transform.up,Vector3.up)>.6f&&stun<=0&&launchGrace<=0)wrongWayTime+=Time.deltaTime;
   }
   bool moved=Vector3.ProjectOnPlane(transform.position-motionAnchor,Vector3.up).sqrMagnitude>1.2f;
   if(moved)motionAnchor=transform.position;
   bool suspended=grounded<=1&&Mathf.Abs(body.linearVelocity.y)<.5f;
   bool trying=Mathf.Abs(throttle)>.25f;
   bool jammed=!outside&&(wrongWayTime<=0||suspended)&&!moved&&body.linearVelocity.sqrMagnitude<1.5f&&(suspended||trying)&&launchGrace<=0&&stun<=0;
   if(jammed&&suspended)wrongWayTime=0;
   trappedTime=jammed?trappedTime+Time.deltaTime:0;
   float elapsed=outside?offTrackTime:wrongWayTime>0?wrongWayTime:trappedTime,delay=outside?OffTrackDelay:WrongWayDelay;
   if(elapsed>0){
    if(!returnIndicator){
     returnIndicator=new GameObject("Return to track countdown");returnArc=returnIndicator.AddComponent<LineRenderer>();returnArc.useWorldSpace=false;returnArc.positionCount=49;returnArc.widthMultiplier=.11f;returnArc.sharedMaterial=Game.FXMaterial();
     returnLabel=new GameObject("Return countdown").AddComponent<TextMesh>();returnLabel.transform.SetParent(returnIndicator.transform,false);returnLabel.fontSize=64;returnLabel.characterSize=.06f;returnLabel.anchor=TextAnchor.MiddleCenter;
    }
    returnIndicator.SetActive(true);returnIndicator.transform.position=transform.position+Vector3.up*.18f;
    Color warning=Color.Lerp(new Color(1,.79f,.18f),new Color(.25f,1,1),elapsed/delay);returnArc.startColor=returnArc.endColor=warning;
    for(int i=0;i<49;i++){float angle=(i/48f)*Mathf.PI*2*Mathf.Clamp01(1-elapsed/delay);returnArc.SetPosition(i,new Vector3(Mathf.Cos(angle)*2.7f,.02f,Mathf.Sin(angle)*2.7f));}
    returnLabel.text=(outside?"Return in ":wrongWayTime>0?"Wrong way: ":"Car stuck: ")+returnRemaining.ToString("0.0")+"s";returnLabel.color=warning;returnLabel.transform.position=transform.position+Vector3.up*3;returnLabel.transform.rotation=game.cam.transform.rotation;
    if(elapsed>delay-.2f)model.transform.localScale=Vector3.one*Mathf.Lerp(1,.18f,(elapsed-delay+.2f)/.2f);
    if(elapsed>=delay){automaticReturns++;if(!outside){if(wrongWayTime>0)wrongWayReturns++;else trappedReturns++;}TeleportToRoad(true);}
   }else {if(returnIndicator)returnIndicator.SetActive(false);if(returnFlash<=0)model.transform.localScale=Vector3.one;}
   if(returnFlash>0){returnFlash=Mathf.Max(0,returnFlash-Time.deltaTime);model.transform.localScale=Vector3.one*Mathf.Lerp(.18f,1,1-returnFlash/.24f);}
  }
  void TeleportToRoad(bool automatic){
   if(CountingStats)stats.resets++;Vector3 old=transform.position;track.SafeReturnPose(this,game,out Vector3 p,out Quaternion q,out int n);
   ResetFlipRecovery();RaceEffects.Teleport(old);body.position=p;body.rotation=q;if(track.IsAirah)transform.SetPositionAndRotation(p,q);body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;lastPoint=n;Physics.SyncTransforms();ResetFinishSample();
   ClearWeaponStatus();offTrackTime=0;wrongWayTime=0;trappedTime=0;motionAnchor=p;stuck=0;overturned=0;watchdog=0;watchProgress=progress;righting=0;rollGrace=0;rolloverLock=0;launchGrace=0;wet=false;shield=Mathf.Max(shield,1.2f);returnFlash=.24f;
   if(returnIndicator)returnIndicator.SetActive(false);foreach(var t in trails)t.Clear();foreach(var t in flames)t.Clear();
   RaceEffects.Teleport(p);if(player)game.Toast(automatic?"Returned to the track":"Car reset to the road");
  }
  void OnDestroy(){if(returnIndicator)Destroy(returnIndicator);}
 }
 public partial class Game {
  void TrackReturnGUI(){if(player.offTrackTime<=0&&player.wrongWayTime<=0&&player.trappedTime<=0)return;
   bool wrong=player.wrongWayWarning;bool trapped=player.trappedTime>0&&!wrong&&player.offTrackTime<=0;
   Panel(583,188,434,134,.96f);Label(trapped?"Car stuck":wrong?"Wrong way":"Return to track",605,202,330,35,27,null,true);Label(player.returnRemaining.ToString("0.0")+"s",925,202,80,38,29,cyan,true);
   Label(trapped?"Move clear to cancel the return":wrong?"Face the race direction to cancel":"Drive back to cancel the return",605,247,384,30,20,muted);Panel(605,292,390,9,1);GUI.color=cyan;GUI.DrawTexture(new Rect(605,292,390*Mathf.Clamp01(trapped?player.trappedTime/Vehicle.WrongWayDelay:wrong?player.wrongWayTime/Vehicle.WrongWayDelay:player.offTrackTime/Vehicle.OffTrackDelay),9),Texture2D.whiteTexture);GUI.color=Color.white;
  }
 }
}
