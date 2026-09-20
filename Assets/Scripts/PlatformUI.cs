using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public struct PhoneContact {public int id;public Vector2 position;public bool began,ended;public PhoneContact(int i,Vector2 p,bool b=false,bool e=false){id=i;position=p;began=b;ended=e;}}
 public partial class Game {
  public enum LaunchScreen {Splash,DeviceChoice,Ready}
  public LaunchScreen launchScreen=LaunchScreen.Splash;
  public bool phoneMode;public float touchSteer,touchThrottle;public bool touchDrift,touchBoost;
  Texture2D logo;float splashStarted;public int mobileFrameRate=120;
  readonly Rect upRect=new Rect(165,528,125,104),downRect=new Rect(165,754,125,104),leftRect=new Rect(29,641,125,104),rightRect=new Rect(301,641,125,104);
  readonly List<PhoneContact> contacts=new List<PhoneContact>();
  readonly Rect gasRect=new Rect(1380,648,174,203),brakeRect=new Rect(1178,737,166,114),driftRect=new Rect(1178,594,166,115),boostRect=new Rect(1380,507,174,113),itemRect=new Rect(1178,450,166,115),pauseRect=new Rect(1100,30,190,65),resetRect=new Rect(30,365,188,62);
  void InitializePlatform(){
   logo=Resources.Load<Texture2D>("Brand/AetherGroundsLogo");splashStarted=Time.unscaledTime;
   phoneMode=Application.isMobilePlatform;Input.multiTouchEnabled=true;Input.backButtonLeavesApp=false;
   if(testing)launchScreen=LaunchScreen.Ready;
   if(Application.isMobilePlatform){Screen.orientation=ScreenOrientation.AutoRotation;Screen.autorotateToLandscapeLeft=Screen.autorotateToLandscapeRight=true;Screen.autorotateToPortrait=Screen.autorotateToPortraitUpsideDown=false;Screen.sleepTimeout=SleepTimeout.NeverSleep;}
  }
  public void ChooseDevice(bool phone){phoneMode=phone;launchScreen=LaunchScreen.Ready;ClearPhoneInput();ApplyDeviceQuality();}
  void ApplyDeviceQuality(){
   Application.targetFrameRate=phoneMode?MobileFrameTarget():120;Time.fixedDeltaTime=phoneMode?1/60f:1/100f;
   QualitySettings.antiAliasing=phoneMode?2:4;QualitySettings.shadowDistance=phoneMode?65:180;QualitySettings.shadowCascades=phoneMode?1:4;QualitySettings.shadowResolution=phoneMode?UnityEngine.ShadowResolution.Low:UnityEngine.ShadowResolution.High;
   cam.allowHDR=!Application.isMobilePlatform;
   if(Application.isMobilePlatform){int width=Screen.width,height=Screen.height;float ratio=Mathf.Min(1,1600f/Mathf.Max(width,height));if(ratio<1)Screen.SetResolution(Mathf.RoundToInt(width*ratio),Mathf.RoundToInt(height*ratio),true);}
  }
  Rect UIArea(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1600f,safe.height/900f);return new Rect(safe.x+(safe.width-1600*scale)*.5f,Screen.height-safe.y-safe.height+(safe.height-900*scale)*.5f,1600*scale,900*scale);}
  public Vector2 ScreenToUI(Vector2 p){var a=UIArea();return new Vector2((p.x-a.x)*1600/a.width,(Screen.height-p.y-a.y)*900/a.height);}
  void SetUIMatrix(){var a=UIArea();GUI.matrix=Matrix4x4.TRS(new Vector3(a.x,a.y),Quaternion.identity,new Vector3(a.width/1600,a.height/900,1));}
  void StartupUpdate(){
   if(launchScreen==LaunchScreen.Splash&&Time.unscaledTime-splashStarted>.35f&&(Input.GetMouseButtonDown(0)||Input.touchCount>0||Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space)))launchScreen=LaunchScreen.DeviceChoice;
  }
  bool StartupGUI(){
   if(launchScreen==LaunchScreen.Ready)return false;
   DrawRushBackdrop(0);Panel(355,20,890,860,.78f);if(logo)GUI.DrawTexture(new Rect(493,33,614,614),logo,ScaleMode.ScaleToFit,true);
   var centered=new GUIStyle(text){alignment=TextAnchor.MiddleCenter,fontSize=28};
   GUI.Label(new Rect(400,650,800,48),"by Aljay Leodones",centered);
   if(launchScreen==LaunchScreen.Splash){GUI.color=new Color(1,1,1,.65f+.35f*Mathf.Sin(Time.unscaledTime*2));centered.fontSize=32;GUI.Label(new Rect(350,754,900,60),"Press anywhere to start",centered);GUI.color=Color.white;}
   else{
    if(Button("Playing on a PC",370,739,410,76,!phoneMode))ChooseDevice(false);
    if(Button("Playing on a Phone",820,739,410,76,phoneMode))ChooseDevice(true);
    centered.fontSize=21;GUI.Label(new Rect(320,834,960,34),"You can change controls later in Controls.",centered);
   }
   return true;
  }
  public void HandleBack(){
   ClearPhoneInput();
   if(launchScreen==LaunchScreen.DeviceChoice){launchScreen=LaunchScreen.Splash;splashStarted=Time.unscaledTime;return;}
   if(launchScreen!=LaunchScreen.Ready)return;
   if(finishCelebrating&&state==State.Race){RevealResultStats();return;}if(state==State.Race||state==State.Countdown){PauseRace();return;}
   if(state==State.Pause){state=beforePause;Time.timeScale=1;return;}
   if(state==State.Results){ReturnMenu();menu="Play";return;}
   if(menu!="Play")menu="Play";else launchScreen=LaunchScreen.DeviceChoice;
  }
  public void PauseRace(){if(state!=State.Race&&state!=State.Countdown)return;beforePause=state;state=State.Pause;Time.timeScale=0;ClearPhoneInput();}
  public void ClearPhoneInput(){touchSteer=touchThrottle=0;touchDrift=touchBoost=false;}
  void OnApplicationPause(bool paused){if(paused&&!testing){PauseRace();Save();}}
  void OnApplicationFocus(bool focus){if(!focus&&!testing)PauseRace();}
  void UpdatePhoneInput(){
   if(!phoneMode||(state!=State.Race&&state!=State.Countdown)||launchScreen!=LaunchScreen.Ready){ClearPhoneInput();return;}
   contacts.Clear();
   foreach(var touch in Input.touches)contacts.Add(new PhoneContact(touch.fingerId,ScreenToUI(touch.position),touch.phase==TouchPhase.Began,touch.phase==TouchPhase.Ended||touch.phase==TouchPhase.Canceled));
   if(Input.touchCount==0&&(Input.GetMouseButton(0)||Input.GetMouseButtonUp(0)))contacts.Add(new PhoneContact(-2,ScreenToUI(Input.mousePosition),Input.GetMouseButtonDown(0),Input.GetMouseButtonUp(0)));
   ApplyPhoneContacts(contacts);
  }
  public void ApplyPhoneContacts(IList<PhoneContact> points){
   touchSteer=touchThrottle=0;touchDrift=touchBoost=false;bool left=false,right=false,up=false,down=false;
   foreach(var touch in points){
    if(touch.ended)continue;var p=touch.position;
    left|=leftRect.Contains(p);right|=rightRect.Contains(p);up|=upRect.Contains(p)||gasRect.Contains(p);down|=downRect.Contains(p)||brakeRect.Contains(p);
    if(driftRect.Contains(p))touchDrift=true;if(boostRect.Contains(p))touchBoost=true;
    if(touch.began&&itemRect.Contains(p)&&player)player.UseItem();
    if(touch.began&&resetRect.Contains(p)&&player)player.Recover();
    if(touch.began&&pauseRect.Contains(p)){PauseRace();return;}
   }
   touchSteer=(right?1:0)-(left?1:0);touchThrottle=down?-1:up?1:0;
  }
  public static int SupportedMobileFrameRate(int preference,double refresh){if(preference<=60)return 60;if(refresh>=118)return 120;if(refresh>=88)return 90;return 60;}
  int MobileFrameTarget()=>Application.isMobilePlatform?SupportedMobileFrameRate(mobileFrameRate,Screen.currentResolution.refreshRateRatio.value):mobileFrameRate;
  public void SetMobileFrameRate(int fps){mobileFrameRate=fps<=60?60:120;ApplyDeviceQuality();}
  void PhonePad(Rect rect,string title,bool held){RushTextures();GUI.color=held?orange:new Color(.12f,.18f,.24f,.93f);GUI.DrawTexture(rect,cutButton);GUI.color=Color.white;var s=new GUIStyle(text){alignment=TextAnchor.MiddleCenter,fontSize=26,fontStyle=FontStyle.Bold};GUI.Label(rect,title,s);}
  void PhoneRaceHUD(){
   Panel(28,25,310,116,.88f);Label(player.place+" / "+racers.Count,48,34,270,56,43,null,true);Label(stuntMode?"Checkpoint "+player.stuntCheckpoint+" / 6":rushMode?Mathf.Max(0,Challenge.meters-player.transform.position.z).ToString("0")+"m to finish":"Lap "+Mathf.Clamp(player.completedLaps+1,1,laps)+" / "+laps,49,96,260,34,24,cyan);
   Panel(555,25,486,114,.94f);Label(stuntMode?"Heartstopper":rushMode?Challenge.name:TrackWorld.Names[mapIndex],580,36,442,34,27,null,true);Label(rushMode?Mathf.Max(0,Challenge.seconds-raceTime).ToString("0.0")+"s left   /   "+player.coins+" coins":TimeFormat(raceTime)+"   /   "+player.coins+" coins",580,86,442,35,23,cyan);if(!rushMode&&!stuntMode)MiniMap();
   Panel(633,737,390,116,.86f);Label(Mathf.RoundToInt(player.speed)+" km/h",657,747,350,45,36,null,true);Bar("Boost",player.energy,100,657,794,340);
   if(state!=State.Race&&state!=State.Countdown)return;
   PhonePad(upRect,"Up",touchThrottle>0);PhonePad(downRect,"Down",touchThrottle<0);PhonePad(leftRect,"Left",touchSteer<0);PhonePad(rightRect,"Right",touchSteer>0);

   PhonePad(gasRect,"Accelerate",touchThrottle>0);PhonePad(brakeRect,"Brake\nReverse",touchThrottle<0);PhonePad(driftRect,"Drift",touchDrift);PhonePad(boostRect,"Boost",touchBoost);PhonePad(itemRect,player.item==""?"No item":player.item,false);PhonePad(resetRect,"Reset car",false);PhonePad(pauseRect,"Pause",false);
  }
 }
}
