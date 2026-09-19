from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=root/'Assets/Scripts/Vehicle.cs';s=p.read_text(encoding='utf-8').replace('public class Vehicle:','public partial class Vehicle:')
s=s.replace('if(!manualTest){if(player&&!finished)','UpdateTrackBounds();\n   if(!manualTest){if(player&&!finished)')
old='if(!player&&stuck>3)Recover();if(transform.position.y<-25||Vector3.Dot(transform.up,Vector3.up)<.15f){stuck+=Time.deltaTime;if(stuck>2.5f)Recover();}'
new='if(!player&&stuck>3&&offTrackTime<=0)Recover();'
assert old in s;s=s.replace(old,new)
s=s.replace('TrackWorld.FlatDistance(transform.position,track.points[point])<22','track.OnRoad(transform.position)')
s=s.replace('if(watchdog>5){Recover();','if(watchdog>5&&offTrackTime<=0){Recover();')
s=s.replace('if(transform.position.y<-20&&overturned>.7f){Recover();overturned=0;}','')
start=s.index('public void Recover(){');end=s.index('public void UseItem()',start)
s=s[:start]+'public void Recover(){TeleportToRoad(false);}\n  '+s[end:]
p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/Game.cs';s=p.read_text(encoding='utf-8');s=s.replace('if(state==State.Countdown){Panel(662','TrackReturnGUI();\n   if(state==State.Countdown){Panel(662');p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/PhysicsSmokeTest.cs';s=p.read_text(encoding='utf-8').replace('car.manualTest=true;','car.manualTest=true;car.enforceTrackBounds=false;');p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/OverhaulTest.cs';s=p.read_text(encoding='utf-8');marker='   for(int map=0;map<3;map++){'
test='''   car.manualTest=true;car.throttle=0;car.steer=0;car.boost=0;int returns=car.automaticReturns;
   Place(car,190,32);yield return new WaitForSeconds(1.1f);Check(car.automaticReturns==returns&&car.offTrackTime>1&&car.returnRemaining<1,"Off-track countdown runs before teleport");yield return Capture("track-return-countdown");
   yield return new WaitForSeconds(.3f);Check(car.automaticReturns==returns+1&&game.track.OnRoad(car.transform.position),"Off-track player returns to valid road after two seconds");yield return Capture("track-return-arrival");
   returns=car.automaticReturns;Place(car,194,30);yield return new WaitForSeconds(.6f);Place(car,194,7);yield return new WaitForSeconds(1.6f);Check(car.automaticReturns==returns&&car.offTrackTime==0,"Driving back cancels the return countdown");
   target=game.racers[2];target.manualTest=true;target.throttle=0;target.steer=0;returns=target.automaticReturns;Place(target,270,-32);yield return new WaitForSeconds(2.3f);Check(target.automaticReturns==returns+1&&game.track.OnRoad(target.transform.position),"AI also returns after two seconds outside the track");
'''
assert marker in s;s=s.replace(marker,test+marker);p.write_text(s,encoding='utf-8')
