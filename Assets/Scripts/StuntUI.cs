using UnityEngine;
namespace Aether {
 public partial class Game {
  public int stuntAiCount=5;public bool stuntMode,stuntTab;int stuntAward;bool stuntRewardGranted;float stuntBest;
  public void StartStunts(){rushMode=false;stuntMode=true;stuntAward=0;stuntRewardGranted=false;stuntBest=PlayerPrefs.GetFloat("heartstopper-best",0);StartRace();}
  void AwardStunts(Vehicle v){if(!v.player||stuntRewardGranted)return;stuntRewardGranted=true;stuntAward=Mathf.Max(600,1800-v.automaticReturns*50)+v.coins*10;credits+=stuntAward;inventory.Add("upgrade-part",4);if(stuntBest<=0||v.finishTime<stuntBest){stuntBest=v.finishTime;if(!testing)PlayerPrefs.SetFloat("heartstopper-best",stuntBest);}Save();Toast("Heartstopper complete · +"+stuntAward+" credits");}
  void StuntHomeGUI(){Panel(0,172,1600,728,.97f);Label("STUNTS",37,196,890,74,62,cyan,true);Label("Heartstopper",39,290,830,66,47,paper,true);Label("Start 1,600 metres above the ocean. Descend 1,000 metres, climb the launch ramp and steer through the air to the next island.",43,382,625,132,26,muted);
   Label("The second descent adds traps, slippery surfaces, jump tiles, boost lanes and water. Six checkpoints save your progress through each run.",43,535,625,122,24,paper);
   if(Button("Play Heartstopper",42,708,370,65,true))StartStunts();if(Button("Garage",430,708,234,65)){menu="Garage";}
   Stepper("AI opponents",ref stuntAiCount,0,9,44,793);
   Card(710,226,847,433,new Color(.027f,.06f,.09f));Label("Course profile",738,249,780,37,25,paper,true);DrawStuntProfile(new Rect(746,313,758,264));Label("1,600 m",748,307,160,28,19,cyan);Label("Ocean",1360,572,138,27,18,muted);Label("1 km descent",770,603,266,32,21,cyan);Label("Two island jumps",1165,603,360,32,21,orange);
   Card(710,682,847,155);Label("Air controls",735,700,785,31,25,paper,true);Label("A / D: turn and bank    W / S: pitch down / up\nRelease controls to settle. R returns to your saved checkpoint.\nPhone: steer to turn; Accelerate / Brake controls pitch.",737,747,782,79,20,muted);
  }
  void DrawStuntProfile(Rect r){for(int i=0;i<180;i++){float a=i/180f*4800,b=(i+1)/180f*4800;Vector2 from=new Vector2(r.x+a/4800*r.width,r.y+r.height-TrackWorld.StuntHeight(a)/1750*r.height),to=new Vector2(r.x+b/4800*r.width,r.y+r.height-TrackWorld.StuntHeight(b)/1750*r.height);var old=GUI.matrix;GUIUtility.RotateAroundPivot(Mathf.Atan2(to.y-from.y,to.x-from.x)*Mathf.Rad2Deg,from);GUI.color=TrackWorld.StuntGap(a)?new Color(.2f,.35f,.4f):a>2500?orange:cyan;GUI.DrawTexture(new Rect(from.x,from.y,Vector2.Distance(from,to)+1,4),Texture2D.whiteTexture);GUI.matrix=old;GUI.color=Color.white;}}
  void StuntRaceGUI(){if(!player)return;
   if(phoneMode)PhoneRaceHUD();else{Panel(28,27,349,115);Label("Heartstopper",48,39,310,43,30,cyan,true);Label(TimeFormat(raceTime)+"  ·  "+player.place+" / "+racers.Count,49,93,304,34,23,paper);Panel(1235,27,338,115);Label("Checkpoint "+player.stuntCheckpoint+" / 6",1255,39,306,40,26,paper,true);Label("Altitude "+player.transform.position.y.ToString("N0")+" m",1255,95,295,33,22,muted);Panel(1280,716,293,151);Label(player.speed.ToString("000")+" km/h",1300,735,265,61,39,paper,true);Bar("Boost · Shift",player.energy,100,1300,809,245);Panel(28,742,415,125);Label(player.item==""?"Collect a power-up":player.item,47,758,376,37,27,cyan,true);Label("E  Use item · R  Checkpoint",48,813,373,29,20,muted);}
   if(player.grounded<2){Panel(546,185,506,84,.85f);Label("Air control · A / D turn · W / S pitch",565,201,471,37,23,cyan,true);Label("Aim for the wide illuminated landing deck",565,241,476,25,18,muted);}
   TrackReturnGUI();if(state==State.Countdown){Panel(546,331,508,169);Label(countdown>1?Mathf.CeilToInt(countdown-1).ToString():"Ready",700,351,260,90,67,cyan,true);Label("Reach all six checkpoints",591,448,430,35,24,paper);}
   if(state==State.Pause){Panel(0,0,1600,900,.78f);Panel(550,232,500,420);Label("Paused",588,260,425,65,45,paper,true);if(Button("Resume",586,350,428,63,true))HandleBack();if(Button("Restart Heartstopper",586,433,428,63))StartStunts();if(Button("Main Menu",586,516,428,63))MainMenu();}
   if(state==State.Results){Panel(0,0,1600,900,.8f);Panel(350,181,900,541);Label("Heartstopper complete",388,212,827,66,43,cyan,true);Label(TimeFormat(player.finishTime)+" · "+player.automaticReturns+" automatic returns",391,308,803,51,29,paper);Label("+"+stuntAward+" credits · 4 upgrade parts",391,383,810,54,29,orange,true);Label("Best time  "+TimeFormat(stuntBest),391,456,810,43,24,muted);if(Button("Play again",391,604,388,65,true))StartStunts();if(Button("Main Menu",800,604,407,65))MainMenu();}
  }
 }
}
