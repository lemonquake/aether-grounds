using System.Collections.Generic;
using UnityEngine;

namespace Aether {
 // Pure run state. Only active simulation advances these clocks; rewards are
 // settled by the existing once-only finish path, never by HUD rendering.
 public sealed class DrivingSkillRun {
  public int score, pending, events, bestChain, drifts, passes, landings;
  public float remaining, cleanTime, bestCleanTime;
  public string lastEvent="Build a chain with skilled driving";
  public int Multiplier=>Mathf.Min(3,1+events/3);
  public void Add(string label,int points){
   pending=Mathf.Min(3000,pending+points*Multiplier);events++;
   remaining=4;lastEvent=label;
  }
  public void Tick(float dt,bool clean){
   if(dt<=0)return;
   cleanTime=clean?cleanTime+dt:0;bestCleanTime=Mathf.Max(bestCleanTime,cleanTime);
   if(pending>0){remaining=Mathf.Max(0,remaining-dt);if(remaining<=0)Bank();}
  }
  public void Bank(){if(pending<=0)return;bestChain=Mathf.Max(bestChain,pending);score=Mathf.Min(999999,score+pending);lastEvent="Banked "+pending+" skill points";pending=events=0;remaining=0;}
  public void Break(){if(pending>0)lastEvent="Chain lost · banked points kept";pending=events=0;remaining=cleanTime=0;}
  public int TechniqueCount(bool stunts,bool rush,bool solo)=>stunts?landings:rush&&!solo?passes:drifts;
  public int TechniqueTarget(bool stunts)=>stunts?2:3;
  public int Goals(bool stunts,bool rush,bool solo)=>(score>=1500?1:0)+(bestCleanTime>=20?1:0)+(TechniqueCount(stunts,rush,solo)>=TechniqueTarget(stunts)?1:0);
  public int FinishBonus(bool stunts,bool rush,bool solo)=>Mathf.Min(300,score/10)+Goals(stunts,rush,solo)*100;
 }

 public partial class Vehicle {
  public DrivingSkillRun skills=new DrivingSkillRun();
  float skillDriftTime,skillAirTime,skillLandingTime,skillLock;
  bool skillWasGrounded,skillJump;
  Vector3 skillTakeoff;
  readonly HashSet<Vehicle> passCandidates=new HashSet<Vehicle>();
  readonly Dictionary<Vehicle,int> rewardedPasses=new Dictionary<Vehicle,int>();

  public bool ControlledDrift {
   get {
    if(!body||grounded<2||!drift||Mathf.Abs(steer)<.15f||stun>0||frozenFor>0||oilFor>0)return false;
    float forward=Vector3.Dot(body.linearVelocity,body.rotation*Vector3.forward);
    float lateral=Mathf.Abs(Vector3.Dot(body.linearVelocity,body.rotation*Vector3.right));
    float angle=Mathf.Atan2(lateral,Mathf.Abs(forward))*Mathf.Rad2Deg;
    return forward>15&&angle>=8&&angle<=55&&Vector3.Dot(body.rotation*Vector3.up,Vector3.up)>.6f;
   }
  }

  public bool BoostAvailable=>empFor<=0&&frozenFor<=0;
  public void UpdateBoostEnergy(bool held,float dt){
   if(!BoostAvailable){boost=0;return;}
   if(held&&energy>0&&throttle>0){boost=Mathf.Max(boost,.12f);energy=Mathf.Max(0,energy-dt*27*(player&&game.inventory.engine=="storm-coil"?.8f:1));}
   else energy=Mathf.Min(100,energy+dt*9);
  }

  public void InterruptDrivingSkills(){
   skills.Break();skillDriftTime=skillAirTime=skillLandingTime=0;
   skillJump=skillWasGrounded=false;skillLock=1;passCandidates.Clear();
  }

  void UpdateDrivingSkills(){
   if(!CountingStats)return;
   float dt=Time.fixedDeltaTime;skillLock=Mathf.Max(0,skillLock-dt);
   if(!player)return;
   bool road=track.InRaceCorridor(body.position);
   Vector3 tangent=track.Forward(track.Nearest(body.position));
   bool forward=Vector3.Dot(body.linearVelocity,tangent)>8&&!IsWrongWay(body.rotation*Vector3.forward,body.linearVelocity,tangent);
   bool safe=road&&forward&&stun<=0&&frozenFor<=0&&skillLock<=0;
   bool onGround=grounded>=2;
   if(!road||stun>0||frozenFor>0||IsWrongWay(body.rotation*Vector3.forward,body.linearVelocity,tangent)){
    InterruptDrivingSkills();return;
   }
   skills.Tick(dt,safe&&onGround&&speed>=60);
   if(safe&&ControlledDrift){
    if(empFor<=0)energy=Mathf.Min(100,energy+dt*16);
    skillDriftTime+=dt;
    if(skillDriftTime>=1){skillDriftTime-=1;skills.drifts++;skills.Add("Controlled drift",100);}
   }else skillDriftTime=0;

   if(!onGround){
    if(skillWasGrounded&&safe){skillTakeoff=body.position;skillJump=true;skillAirTime=0;}
    if(skillJump)skillAirTime+=dt;
    skillLandingTime=0;
   }else if(skillJump){
    skillLandingTime+=dt;
    if(!safe||Vector3.Dot(body.rotation*Vector3.up,Vector3.up)<.65f){skillJump=false;}
    else if(skillLandingTime>=.2f){
     if(skillAirTime>=.6f&&Vector3.ProjectOnPlane(body.position-skillTakeoff,Vector3.up).magnitude>=12){
      skills.landings++;skills.Add("Clean landing",Mathf.Min(400,150+Mathf.FloorToInt(skillAirTime*30)));
      if(BoostAvailable)energy=Mathf.Min(100,energy+8);
     }
     skillJump=false;
    }
   }
   skillWasGrounded=onGround;

   foreach(var other in game.racers){
    if(!other||other==this||other.finished)continue;
    Vector3 local=Quaternion.Inverse(body.rotation)*(other.body.position-body.position);
    bool close=Mathf.Abs(local.x)<7&&Mathf.Abs(local.y)<3&&Mathf.Abs(local.z)<28;
    if(!safe||!onGround||!close||other.skillLock>0||Vector3.Dot(body.rotation*Vector3.forward,other.body.rotation*Vector3.forward)<.5f){passCandidates.Remove(other);continue;}
    int lap=completedLaps;
    if(rewardedPasses.TryGetValue(other,out int paidLap)&&paidLap==lap)continue;
    if(local.z>5&&other.speed>20&&speed>other.speed+3)passCandidates.Add(other);
    if(local.z< -5&&passCandidates.Remove(other)){
     rewardedPasses[other]=lap;skills.passes++;skills.Add("Clean overtake",200);
     if(BoostAvailable)energy=Mathf.Min(100,energy+6);
    }
   }
  }
 }

 public partial class Game {
  public int skillAward,previousSkillBest;public bool newSkillBest;
  string skillRecordKey;
  bool skillSolo;
  void BeginDrivingSkills(){
   skillAward=0;newSkillBest=false;skillSolo=racers.Count<=1;
   // Records are skill scores, not time records. Keep race settings separate.
   skillRecordKey="driving-skills-v1-"+(stuntMode?"stunts":rushMode?"rush-"+rushIndex:"circuit-"+mapIndex+"-laps-"+laps)+"-opponents-"+(racers.Count-1)+"-difficulty-"+difficulty+"-items-"+powerups;
   previousSkillBest=Mathf.Max(0,PlayerPrefs.GetInt(skillRecordKey,0));
  }
  void AwardDrivingSkills(){
   player.skills.Bank();skillAward=player.skills.FinishBonus(stuntMode,rushMode,skillSolo);
   credits+=skillAward;newSkillBest=player.skills.score>previousSkillBest;
   if(newSkillBest&&!testing)PlayerPrefs.SetInt(skillRecordKey,player.skills.score);
  }
  string TechniqueGoal=>stuntMode?"Land 2 jumps":rushMode&&!skillSolo?"Make 3 clean overtakes":"Hold 3 seconds of controlled drift";
  string SkillGoalSummary=>player.skills.Goals(stuntMode,rushMode,skillSolo)+" / 3 driving goals";
  void DrivingSkillsGUI(){
   if(!player||state!=State.Race||player.finished)return;
   var s=player.skills;float x=phoneMode?552:28,y=phoneMode?400:365,w=phoneMode?496:365;
   Panel(x,y,w,phoneMode?137:178,.89f);
   Label("Driving skills · "+s.score.ToString("N0"),x+16,y+10,w-32,32,23,cyan,true);
   string chain=s.pending>0?s.pending+" pending · ×"+s.Multiplier+" · "+s.remaining.ToString("0.0")+"s":s.lastEvent;
   Label(chain,x+16,y+46,w-32,50,phoneMode?21:20,muted);
   if(phoneMode)Label(SkillGoalSummary+" · Finish bonus up to 600",x+16,y+98,w-32,30,19,cyan);
   else {Label(SkillGoalSummary,x+16,y+100,w-32,30,20,cyan);Label(TechniqueGoal,x+16,y+134,w-32,40,18,muted);}
  }
  void DrivingGoalsPauseGUI(){
   Panel(34,235,470,397,.98f);Label("Driving goals",58,255,422,47,32,cyan,true);
   Label("Complete each goal for +100 finish credits. Banked skill points add up to +300 more.",58,314,422,93,23,muted);
   var s=player.skills;
   Label("Bank 1,500 points: "+Mathf.Min(1500,s.score)+" / 1500\n"+TechniqueGoal+": "+Mathf.Min(s.TechniqueTarget(stuntMode),s.TechniqueCount(stuntMode,rushMode,skillSolo))+" / "+s.TechniqueTarget(stuntMode)+"\nDrive cleanly at 60+ km/h: "+Mathf.Min(20,s.bestCleanTime).ToString("0")+" / 20s",58,416,422,145,22);
   Label("Crashes and resets lose pending points. Chains bank after 4 seconds without a new skill.",58,558,422,63,19,muted);
  }
 }
}
