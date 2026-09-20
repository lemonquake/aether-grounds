using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aether {
 public partial class Game {
  public bool RaceSimulationActive=>!loading&&(state==State.Race||state==State.Results&&!rushFailed&&player&&raceTime<ResultsDeadline&&racers.Any(v=>!v.finished));
  float ResultsDeadline=>rushMode?Challenge.seconds:(player?player.finishTime:raceTime)+60;
  public bool finishCelebrating;public float finishStarted;float resultsStarted,lapFlashUntil,statsStarted=-1;bool resultRewards;int circuitAward;
  string lapFlash;ResultsStage resultsStage;Vector2 resultsScroll;Vehicle statsCar;bool showStats;
  public ResultsStage ResultStage=>resultsStage;
  Color ResultAccent=>stuntMode?new Color(.70f,.52f,1):rushMode?Challenge.color:new Color(.23f,.94f,.83f);
  string ResultMode=>stuntMode?"Heartstopper":rushMode?Challenge.name:"Circuit";
  public void UpdateRacePlaces(){var sorted=racers.OrderBy(v=>v.finished?0:1).ThenBy(v=>v.finished?v.finishOrder:0).ThenByDescending(v=>v.progress).ToArray();for(int i=0;i<sorted.Length;i++)sorted[i].place=i+1;}
  public void LapCrossed(int lap,float time){lapFlash="Lap "+lap+" complete   ·   "+TimeFormat(time);lapFlashUntil=Time.unscaledTime+2;FinishCelebration.Chime(cam.transform,volume*.45f,false);}
  public void BeginFinishCelebration(){finishCelebrating=true;finishStarted=Time.unscaledTime;messageTimer=0;statsCar=player;FinishCelebration.Confetti(player.transform.position+Vector3.up*2,player.transform);FinishCelebration.Chime(cam.transform,volume,true);}
  public void BeginFailedResults(){finishCelebrating=false;showStats=true;statsCar=player;OpenResultsStage();}
  void OpenResultsStage(){state=State.Results;resultsStarted=Time.unscaledTime;Time.timeScale=1;ClearPhoneInput();resultsStage=new GameObject("Match results car show").AddComponent<ResultsStage>();resultsStage.Build(this,ResultAccent);messageTimer=0;}
  void UpdateResultsPresentation(){
   if(finishCelebrating&&state==State.Race&&Time.unscaledTime-finishStarted>=3.6f)OpenResultsStage();
   if(resultsStage){resultsStage.Refresh(racers);if(Time.unscaledTime-resultsStarted>5)showStats=true;}
  }
  void ClearResults(){if(resultsStage){resultsStage.gameObject.SetActive(false);Destroy(resultsStage.gameObject);}resultsStage=null;finishCelebrating=false;showStats=false;statsCar=null;resultsScroll=Vector2.zero;statsStarted=-1;resultRewards=false;circuitAward=0;lapFlashUntil=0;}
  void ResultRect(Rect r,Color color){GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
  void ResultText(string value,Rect r,int size,Color color,TextAnchor anchor=TextAnchor.MiddleLeft,bool bold=false){var style=new GUIStyle(text){fontSize=size,alignment=anchor,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,wordWrap=false};style.normal.textColor=color;GUI.Label(r,value,style);}
  public void RevealResultStats(){showStats=true;}
  bool ResultsPresentationGUI(){
   if(!player)return false;
   if(finishCelebrating&&state==State.Race){
    float t=Time.unscaledTime-finishStarted;Color accent=ResultAccent;
    ResultRect(new Rect(0,0,1600,900),new Color(.012f,.02f,.04f,.20f));
    ResultRect(new Rect(0,0,1600,110),new Color(.008f,.014f,.025f,.92f));ResultRect(new Rect(0,770,1600,130),new Color(.008f,.014f,.025f,.92f));
    // Staggered checkers travel behind the title, then give way to the placement.
    for(int i=0;i<30;i++){float x=(i*100+t*330)%1900-150;ResultRect(new Rect(x,285+(i%2)*65,60,60),new Color(accent.r,accent.g,accent.b,.16f));}
    string title=t<1.45f?"FINISH":Ordinal(player.finishOrder);float phase=t<1.45f?t:t-1.45f;
    float scale=1+Mathf.Exp(-phase*8)*.45f;var matrix=GUI.matrix;GUIUtility.ScaleAroundPivot(Vector2.one*scale,new Vector2(800,425));
    ResultText(title,new Rect(0,325,1600,185),t<1.45f?145:132,Color.white,TextAnchor.MiddleCenter,true);GUI.matrix=matrix;
    ResultText(t<1.45f?ResultMode:player.finishOrder==1?"First place": "Place "+player.finishOrder+" of "+racers.Count,new Rect(0,527,1600,65),34,accent,TextAnchor.MiddleCenter,true);
    ResultText(TimeFormat(player.finishTime)+"   ·   "+player.spec.name,new Rect(0,790,1600,65),30,Color.white,TextAnchor.MiddleCenter);
    ResultRect(new Rect(0,765,1600*Mathf.Clamp01(t/3.6f),5),accent);
    if(Button("Show results",1320,28,245,55)){OpenResultsStage();showStats=true;}return true;
   }
   if(state==State.Results&&resultsStage){DrawCarShowResults();return true;}
   if(state==State.Race&&Time.unscaledTime<lapFlashUntil){ResultRect(new Rect(485,330,630,85),new Color(.02f,.04f,.07f,.94f));ResultText(lapFlash,new Rect(500,340,600,65),29,ResultAccent,TextAnchor.MiddleCenter,true);}
   return false;
  }
  public static string Ordinal(int n)=>n+(n%100>=11&&n%100<=13?"th":n%10==1?"st":n%10==2?"nd":n%10==3?"rd":"th");
  void DrawCarShowResults(){
   float age=Time.unscaledTime-resultsStarted;var accent=ResultAccent;float reveal=showStats?Mathf.SmoothStep(0,1,Mathf.Clamp01((age-0.15f)*2)):0;
   ResultRect(new Rect(0,0,1600,900),new Color(.012f,.019f,.033f));
   Rect stageRect=showStats?new Rect(22,135,930,442):new Rect(0,90,1600,735);
   GUI.DrawTexture(stageRect,resultsStage.Output,ScaleMode.StretchToFill,false);
   ResultRect(new Rect(0,0,1600,123),new Color(.018f,.027f,.044f,.98f));ResultRect(new Rect(34,121,1532,3),accent);
   ResultText(rushFailed?"Time limit reached":ResultMode+" Results",new Rect(34,20,920,61),43,Color.white,TextAnchor.MiddleLeft,true);
   string detail=stuntMode?"Six checkpoints · Two island jumps":rushMode?Challenge.subtitle:TrackWorld.Names[mapIndex]+" · "+laps+(laps==1?" lap":" laps");
   ResultText(detail,new Rect(36,81,950,32),21,muted);
   ResultText(rushFailed?"Run ended":Ordinal(player.finishOrder)+" / "+racers.Count,new Rect(1185,22,380,61),42,rushFailed?orange:new Color(1,.81f,.35f),TextAnchor.MiddleRight,true);
   ResultText(rushFailed?TimeFormat(raceTime):TimeFormat(player.finishTime),new Rect(1185,81,380,32),23,Color.white,TextAnchor.MiddleRight);
   for(int i=0;i<3;i++){
    var v=resultsStage.Winners[i];Vector2 point=new Vector2(stageRect.x+stageRect.width*(i==0?.5f:i==1?.19f:.81f),stageRect.yMax-92);float width=showStats?270:440;
    ResultRect(new Rect(point.x-width/2,point.y,width,showStats?58:72),new Color(.01f,.02f,.032f,.91f));
    ResultText(v?Ordinal(i+1)+" · "+v.driver:"Place "+(i+1),new Rect(point.x-width/2+10,point.y+2,width-20,30),showStats?19:25,ResultsStage.Medals[i],TextAnchor.MiddleCenter,true);
    ResultText(v?v.spec.name+" · "+TimeFormat(v.finishTime):RaceSimulationActive?"Waiting for finisher":"No finisher",new Rect(point.x-width/2,point.y+31,width,28),showStats?17:20,muted,TextAnchor.MiddleCenter);
   }
   if(!showStats){ResultText(rushFailed?"Challenge ended": "Podium",new Rect(35,143,700,50),34,accent,TextAnchor.MiddleLeft,true);if(Button("Show all stats",1165,811,400,59,true))showStats=true;return;}
   // Results retain distinct mode objectives and featured statistics.
   if(statsStarted<0)statsStarted=Time.unscaledTime;float statsAge=Time.unscaledTime-statsStarted;Vehicle selected=statsCar?statsCar:player;RaceStats s=selected.stats;
   ResultRect(new Rect(980,145,586,651),new Color(.029f,.042f,.064f));ResultRect(new Rect(980,145,5,651),accent);
   var portrait=resultsStage.Portrait(selected);if(portrait)GUI.DrawTexture(new Rect(996,155,188,116),portrait,ScaleMode.ScaleToFit,true);
   ResultText(selected.driver,new Rect(1200,161,350,38),28,Color.white,TextAnchor.MiddleLeft,true);ResultText(selected.spec.name,new Rect(1200,202,350,34),24,accent);
   string objective=stuntMode?"Air time   "+s.airtime.ToString("0.0")+" s":rushMode?"Top speed   "+s.topSpeed.ToString("0")+" km/h":"Best lap   "+(s.bestLap>0?TimeFormat(s.bestLap):"—");
   ResultRect(new Rect(1003,280,540,64),new Color(accent.r*.15f,accent.g*.15f,accent.b*.15f));ResultText(objective,new Rect(1020,284,510,55),28,accent,TextAnchor.MiddleLeft,true);
   string[] labels={"Top speed","Out of bounds","Props destroyed","Civilians hit","Traffic cars hit","Distance driven","Air time","Drifting","Boost time","Power-ups used","Collisions","Car resets"};
   string[] values={s.topSpeed.ToString("0")+" km/h",s.outOfBounds.ToString("0.0")+" s",s.propsDestroyed.ToString(),s.civilians.ToString(),s.trafficCars.ToString(),(s.distance/1000).ToString("0.00")+" km",s.airtime.ToString("0.0")+" s",s.driftTime.ToString("0.0")+" s",s.boostTime.ToString("0.0")+" s",s.itemsUsed.ToString(),s.collisions.ToString(),s.resets.ToString()};
   for(int i=0;i<labels.Length;i++){int col=i%2,row=i/2;float x=1004+col*276,y=357+row*56;float enter=Mathf.Clamp01((statsAge-(i*.035f))/.35f);ResultText(labels[i],new Rect(x+12*(1-enter),y,264,25),18,muted);ResultText(values[i],new Rect(x,y+23,264,30),24,Color.white,TextAnchor.MiddleLeft,true);}
   int award=(stuntMode?stuntAward:rushMode?rushAward:circuitAward)+skillAward;
   ResultRect(new Rect(1003,709,540,66),new Color(.01f,.02f,.035f));
   ResultText(selected==player?(rushFailed?"No finish reward":"+"+award.ToString("N0")+" credits · "+player.coins+" coins"):"Finish  "+(selected.finished?TimeFormat(selected.finishTime):"Not finished"),new Rect(1016,715,518,31),23,accent,TextAnchor.MiddleLeft,true);
   ResultText(stuntMode?"Best run "+TimeFormat(stuntBest)+" · 4 upgrade parts":rushMode?(rushFailed?"Distance reached "+(Mathf.Clamp01(player.progress/(TrackWorld.Count-1))*100).ToString("0")+"%":new[]{"","Bronze","Silver","Gold"}[rushMedal]+" medal · Rewards saved"):player.coins*100+" coin points · "+player.completedLaps+" laps completed",new Rect(1016,747,518,25),18,muted);
   DrawModeResultDetails(selected);ResultText("Race standings",new Rect(35,587,510,38),27,Color.white,TextAnchor.MiddleLeft,true);
   ResultText(RaceSimulationActive?"Waiting for remaining racers":"Final standings",new Rect(480,593,456,30),18,muted,TextAnchor.MiddleRight);
   var order=racers.OrderBy(v=>v.place).ToArray();resultsScroll=GUI.BeginScrollView(new Rect(34,634,922,162),resultsScroll,new Rect(0,0,895,order.Length*76));
   for(int i=0;i<order.Length;i++){
    var v=order[i];float y=i*76;bool chosen=selected==v;
    ResultRect(new Rect(0,y,894,70),chosen?new Color(.065f,.14f,.16f):new Color(.033f,.049f,.071f));
    if(GUI.Button(new Rect(0,y,894,70),GUIContent.none,GUIStyle.none))statsCar=v;
    ResultText(v.finished?v.finishOrder.ToString():"—",new Rect(13,y,52,70),28,v.finished&&v.finishOrder<=3?ResultsStage.Medals[v.finishOrder-1]:muted,TextAnchor.MiddleCenter,true);
    var pic=resultsStage.Portrait(v);if(pic)GUI.DrawTexture(new Rect(69,y+1,113,68),pic,ScaleMode.ScaleToFit,true);
    ResultText(v.driver,new Rect(197,y+5,330,33),22,v==player?accent:Color.white,TextAnchor.MiddleLeft,true);ResultText(v.spec.name,new Rect(197,y+36,330,28),18,muted);
    ResultText(v.finished?TimeFormat(v.finishTime):RaceSimulationActive?"On course":"Did not finish",new Rect(555,y+3,320,36),23,Color.white,TextAnchor.MiddleRight);
    ResultText(v.finished?(v.finishOrder==1?"Winner":"+"+(v.finishTime-order[0].finishTime).ToString("0.00")+" s"):"Progress "+(Mathf.Clamp01(v.progress/(stuntMode?460:rushMode?TrackWorld.Count-1:laps*TrackWorld.Count))*100).ToString("0")+"%",new Rect(555,y+37,320,25),18,muted,TextAnchor.MiddleRight);
   }GUI.EndScrollView();
   if(Button("Restart",34,825,270,55,true)){if(stuntMode)StartStunts();else StartRace();}
   if(Button("Game Setup",320,825,270,55)){ReturnMenu();menu=stuntMode||rushMode?"Play":"Game Setup";}
   if(Button("Main Menu",606,825,270,55))MainMenu();
   if(Button("Rewards",980,825,185,55))resultRewards=true;ResultText(SkillGoalSummary,new Rect(1180,828,390,48),19,muted,TextAnchor.MiddleCenter);if(resultRewards)DrawResultRewards();
  }
  void DrawModeResultDetails(Vehicle selected){
   Rect r=new Rect(34,549,916,32);ResultRect(r,new Color(.022f,.038f,.057f,.96f));
   if(stuntMode){for(int i=0;i<6;i++){float x=44+i*151;bool passed=i<selected.stuntCheckpoint;ResultText((passed?"✓ ":"○ ")+(i==5?"Finish":"Gate "+(i+1)),new Rect(x,549,145,32),19,passed?ResultAccent:muted);}}
   else if(rushMode){ResultText("Gold  "+TimeFormat(Challenge.gold)+"    Silver  "+TimeFormat(Challenge.gold+30)+"    Limit  "+TimeFormat(Challenge.seconds),new Rect(49,549,888,32),20,ResultAccent);}
   else {string splits=selected.lapTimes.Count==0?"Lap times appear after crossing the finish line":string.Join("     ",selected.lapTimes.Select((lap,i)=>"Lap "+(i+1)+"  "+TimeFormat(lap)));ResultText(splits,new Rect(49,549,888,32),19,ResultAccent);}
  }
  void DrawResultRewards(){
   ResultRect(new Rect(0,0,1600,900),new Color(0,0,0,.72f));ResultRect(new Rect(410,170,780,590),new Color(.025f,.043f,.065f));ResultRect(new Rect(410,170,780,4),ResultAccent);
   ResultText("Your rewards",new Rect(446,186,700,62),39,Color.white,TextAnchor.MiddleLeft,true);
   int award=(stuntMode?stuntAward:rushMode?rushAward:circuitAward)+skillAward;
   ResultText(rushFailed?"No finish reward":"+"+award.ToString("N0")+" credits",new Rect(449,251,700,53),33,ResultAccent,TextAnchor.MiddleLeft,true);
   string details=rushFailed?"The time limit was reached before the finish.":stuntMode?"4 upgrade parts\nBest Heartstopper time: "+TimeFormat(stuntBest):rushMode?rushReward:player.coins+" coins collected\n"+player.coins*100+" coin points\nCoin credits are included in your race reward.";
   var style=new GUIStyle(text){fontSize=25,wordWrap=true};GUI.Label(new Rect(449,315,697,140),details,style);
   ResultText("Driving skills: "+player.skills.score.ToString("N0")+" points · "+SkillGoalSummary,new Rect(449,473,700,36),25,ResultAccent);
   ResultText(rushFailed?"Skill bonus requires a successful finish":"Includes +"+skillAward+" driving credits · "+(newSkillBest?"New skill record":"Previous best: "+previousSkillBest),new Rect(449,512,700,34),22,muted);
   ResultText("Drifts: "+player.skills.drifts+" · Clean overtakes: "+player.skills.passes+" · Landings: "+player.skills.landings,new Rect(449,552,700,34),22,muted);
   ResultText("Best chain: "+player.skills.bestChain+" · Clean driving: "+player.skills.bestCleanTime.ToString("0.0")+"s",new Rect(449,590,700,34),22,muted);
   if(Button("Close",450,669,700,57,true))resultRewards=false;
  }
 }
 public static class FinishCelebration {
  static Material confetti;static AudioClip finishSound,lapSound;
  public static void Confetti(Vector3 position,Transform parent=null){
   if(!confetti)confetti=new Material(Shader.Find("Sprites/Default")){mainTexture=Texture2D.whiteTexture};
   for(int side=-1;side<=1;side+=2){var go=new GameObject("Finish confetti");go.transform.position=position+Vector3.right*side*3; if(parent)go.transform.SetParent(parent,true);
    var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var m=ps.main;m.loop=false;m.duration=1;m.useUnscaledTime=true;m.startLifetime=new ParticleSystem.MinMaxCurve(3,5);m.startSpeed=new ParticleSystem.MinMaxCurve(8,15);m.startSize3D=true;m.startSizeX=.13f;m.startSizeY=.27f;m.startSizeZ=.1f;m.startRotation=new ParticleSystem.MinMaxCurve(0,6.28f);m.gravityModifier=.65f;m.simulationSpace=ParticleSystemSimulationSpace.World;m.maxParticles=320;
    var colors=new Gradient();colors.SetKeys(new[]{new GradientColorKey(new Color(1,.73f,.16f),0),new GradientColorKey(new Color(.1f,1,.8f),.33f),new GradientColorKey(new Color(.7f,.35f,1),.66f),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,1)});m.startColor=new ParticleSystem.MinMaxGradient(colors){mode=ParticleSystemGradientMode.RandomColor};
    var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=35;shape.radius=.3f;go.transform.rotation=Quaternion.Euler(-90,0,0);
    var em=ps.emission;em.rateOverTime=0;em.SetBursts(new[]{new ParticleSystem.Burst(0,120),new ParticleSystem.Burst(.35f,65)});var spin=ps.rotationOverLifetime;spin.enabled=true;spin.z=new ParticleSystem.MinMaxCurve(-7,7);
    var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.7f),new GradientAlphaKey(0,1)});fade.color=gradient;
    ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=confetti;ps.Play();Object.Destroy(go,7);
   }
  }
  public static void Chime(Transform parent,float volume,bool finish){
   AudioClip clip=finish?finishSound:lapSound;if(!clip){int count=44100*(finish?3:1);float[] samples=new float[count];float[] notes=finish?new[]{523.25f,659.25f,783.99f,1046.5f}:new[]{659.25f,783.99f};for(int i=0;i<count;i++){float t=i/44100f,value=0;for(int n=0;n<notes.Length;n++){float dt=t-n*.14f;if(dt>=0)value+=(Mathf.Sin(dt*notes[n]*Mathf.PI*2)+.25f*Mathf.Sin(dt*notes[n]*Mathf.PI*4))*Mathf.Exp(-dt*3.3f)*Mathf.Min(1,dt*60)*.15f;}samples[i]=value;}clip=AudioClip.Create(finish?"Finish fanfare":"Lap chime",count,1,44100,false);clip.SetData(samples,0);if(finish)finishSound=clip;else lapSound=clip;}
   var go=new GameObject("Race celebration audio");go.transform.SetParent(parent,false);var sound=go.AddComponent<AudioSource>();sound.clip=clip;sound.volume=volume;sound.ignoreListenerPause=true;sound.Play();Object.Destroy(go,4);
  }
 }
}
