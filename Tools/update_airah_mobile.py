from pathlib import Path
root=Path(__file__).resolve().parents[1]
def edit(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8');assert old in s,(file,old[:50]);p.write_text(s.replace(old,new),encoding='utf-8')
p=root/'Assets/Scripts/Vehicle.cs';s=p.read_text(encoding='utf-8');start=s.index('   float upright=Vector3.Dot(transform.up,Vector3.up);if(active');end=s.index('\n',start);s=s[:start]+'   UpdateFlipRecovery(active);'+s[end:];s=s.replace('void OnCollisionEnter(Collision c){','void OnCollisionEnter(Collision c){RecordFlipContact(c);');s=s.replace('if(!player&&!manualTest&&stuck>3','if(!player&&!manualTest&&grounded>0&&stuck>3').replace('if(watchdog>5&&offTrackTime','if(watchdog>5&&grounded>0&&offTrackTime');p.write_text(s,encoding='utf-8')
edit('Assets/Scripts/TrackRecovery.cs','RaceEffects.Teleport(old);body.position=p;','ResetFlipRecovery();RaceEffects.Teleport(old);body.position=p;')
p=root/'Assets/Scripts/PlatformUI.cs';s=p.read_text(encoding='utf-8')
s=s.replace('Texture2D logo,controlDisc;int steeringFinger=-1;Vector2 stickOffset;float splashStarted;','Texture2D logo;float splashStarted;public int mobileFrameRate=120;\n  readonly Rect upRect=new Rect(165,528,125,104),downRect=new Rect(165,754,125,104),leftRect=new Rect(29,641,125,104),rightRect=new Rect(301,641,125,104);')
s=s.replace('Application.targetFrameRate=phoneMode?60:120;','Application.targetFrameRate=phoneMode?MobileFrameTarget():120;')
s=s.replace('public void ClearPhoneInput(){steeringFinger=-1;stickOffset=Vector2.zero;','public void ClearPhoneInput(){')
start=s.index('   touchSteer=touchThrottle=0;touchDrift=touchBoost=false;bool hasSteering=false;');end=s.index('  void PhonePad(',start)
s=s[:start]+'''   touchSteer=touchThrottle=0;touchDrift=touchBoost=false;bool left=false,right=false,up=false,down=false;
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
''' +s[end:]
start=s.index('   GUI.color=new Color(.04f,.10f,.14f,.72f);GUI.DrawTexture(new Rect(81,579,260,260)');end=s.index('\n',start)
s=s[:start]+'''   PhonePad(upRect,"Up",touchThrottle>0);PhonePad(downRect,"Down",touchThrottle<0);PhonePad(leftRect,"Left",touchSteer<0);PhonePad(rightRect,"Right",touchSteer>0);
''' +s[end:]
p.write_text(s,encoding='utf-8')
edit('Assets/Scripts/Game.cs','void ControlsGUI(){','void ControlsGUI(){if(phoneMode&&Button(mobileFrameRate==120?"Frame rate: Up to 120 FPS":"Frame rate: 60 FPS",1065,728,465,58,true))SetMobileFrameRate(mobileFrameRate==120?60:120);')
edit('Assets/Scripts/Game.cs','"Left stick     Steer"','"Left / Right     Steer"')
edit('Assets/Scripts/Game.cs','"Accelerate     Hold to drive"','"Up / Accelerate     Hold to drive"')
edit('Assets/Scripts/Game.cs','"Brake / Reverse     Hold to reverse"','"Down / Brake     Brake or reverse"')
print('Ground-contact flip recovery and directional buttons integrated')
