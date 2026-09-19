from pathlib import Path
root=Path(__file__).resolve().parents[1]
def edit(name, old, new):
 p=root/name; s=p.read_text(encoding='utf-8'); assert old in s, (name,old[:100]);p.write_text(s.replace(old,new),encoding='utf-8')
edit('Assets/Scripts/Game.cs','stuntMode?0:rushMode?Challenge.opponents:aiCount','stuntMode?Mathf.Clamp(stuntAiCount,0,9):rushMode?Challenge.opponents:aiCount')
edit('Assets/Scripts/Game.cs','track.StuntPosition(0)+Vector3.up*.55f','track.StuntPosition(-i/2*7)+Vector3.right*(i%2==0?-3:3)+Vector3.up*.55f')
edit('Assets/Scripts/Game.cs','track.ConnectFeatures(this);','track.BuildMapDetails();track.ConnectFeatures(this);')
edit('Assets/Scripts/Game.cs','else if(player){Vector3 forward','else if(player&&stuntMode){UpdateStuntCamera();}\n   else if(player){Vector3 forward')
edit('Assets/Scripts/Game.cs','void ReturnMenu(){','public void MainMenu(){ReturnMenu();menu="Play";stuntMode=false;rushMode=false;stuntTab=false;circuitTab=false;}\n  void ReturnMenu(){')
edit('Assets/Scripts/Game.cs','if(Button("Restart",451,662,319,57,true))StartRace();if(Button("Game Setup",801,662,345,57)){ReturnMenu();menu="Game Setup";}','if(Button("Restart",451,662,215,57,true))StartRace();if(Button("Game Setup",681,662,215,57)){ReturnMenu();menu="Game Setup";}if(Button("Main Menu",911,662,235,57))MainMenu();')
edit('Assets/Scripts/StuntUI.cs','public bool stuntMode,stuntTab;','public int stuntAiCount=5;public bool stuntMode,stuntTab;')
edit('Assets/Scripts/StuntUI.cs','Label("Solo skill course · 4.8 km · no time limit",44,801,900,38,23,cyan);','Stepper("AI opponents",ref stuntAiCount,0,9,44,793);')
edit('Assets/Scripts/StuntUI.cs','if(Button("Play menu",800,604,407,65)){ReturnMenu();menu="Play";stuntTab=true;}','if(Button("Main Menu",800,604,407,65))MainMenu();')
edit('Assets/Scripts/StuntUI.cs','if(Button("Play menu",586,516,428,63)){ReturnMenu();menu="Play";stuntTab=true;}','if(Button("Main Menu",586,516,428,63))MainMenu();')
edit('Assets/Scripts/StuntUI.cs','Label(TimeFormat(raceTime),49,93,304,34,23,paper);','Label(TimeFormat(raceTime)+"  ·  "+player.place+" / "+racers.Count,49,93,304,34,23,paper);')
edit('Assets/Scripts/RushUI.cs','if(Button("Rush Challenges",933,661,291,63)){ReturnMenu();menu="Play";circuitTab=false;}','if(Button("Main Menu",933,661,291,63))MainMenu();')
edit('Assets/Scripts/CarParts.cs','public int body,paint=1','public int paintStyle; public int body,paint=1')
edit('Assets/Scripts/CarParts.cs','public void Validate(){','public void Validate(){paintStyle=Mathf.Clamp(paintStyle,0,PaintStyles.Names.Length-1);')
edit('Assets/Scripts/CarParts.cs','SupportStore.Apply(model,c);','SupportStore.Apply(model,c);PaintStyles.Apply(model,c);')
edit('Assets/Scripts/GarageUI.cs','y=472+i/8*49','y=453+i/8*43')
edit('Assets/Scripts/GarageUI.cs','Label("32 colors · changes save automatically",1078,686,460,30,18,muted);','Choice("Paint Style",ref c.paintStyle,PaintStyles.Names,1078,631);')
edit('Assets/Scripts/Vehicle.cs','void DriveAI(){','void DriveAI(){\n   if(track.isStunts){DriveStuntAI();return;}')
edit('Assets/Scripts/Vehicle.cs','var emission=smoke.emission;emission.rateOverTime=active&&grounded>1&&speed>25&&(drift||slip>4)?34:0;','var emission=smoke.emission;emission.rateOverTime=0;UpdateDrivingEffects(active);')
edit('Assets/Scripts/TrackRecovery.cs','if(isStunts){float z=car.stuntCheckpoint<=0?0:StuntGates[Mathf.Min(car.stuntCheckpoint-1,4)]-12;position=StuntPosition(z)+Vector3.up*.7f;point=Nearest(position);rotation=Quaternion.LookRotation(Forward(point));return;}','''if(isStunts){float z=car.stuntCheckpoint<=0?0:StuntGates[Mathf.Min(car.stuntCheckpoint-1,4)]-12;position=StuntPosition(z)+Vector3.up*.7f;
    for(int row=0;row<5;row++){bool found=false;foreach(float lane in new[]{0f,-4f,4f,-8f,8f}){var candidate=StuntPosition(z-row*6)+Vector3.right*lane+Vector3.up*.7f;bool occupied=false;foreach(var other in game.racers)if(other&&other!=car&&(other.transform.position-candidate).sqrMagnitude<32){occupied=true;break;}if(!occupied){position=candidate;found=true;break;}}if(found)break;}
    point=Nearest(position);rotation=Quaternion.LookRotation(StuntPosition(position.z+1)-StuntPosition(position.z-1));return;}''')
edit('Assets/Scripts/StuntWorld.cs','var ocean=Cube("Heartstopper ocean",new Vector3(0,-7,2300),new Vector3(15000,1,16000),WaterMaterial(),false);','var ocean=Cube("Heartstopper ocean",new Vector3(0,-7,2300),new Vector3(15000,1,16000),new Material(Shader.Find("Aether/RushOcean")),false);')
edit('Assets/Scripts/StuntTest.cs','game.StartStunts();yield return new WaitUntil(()=>game.state==Game.State.Race);car=game.player;car.manualTest=true;Check(game.track.isStunts&&!game.rushMode&&game.racers.Count==1,"STUNTS is separate solo mode");','game.stuntAiCount=0;game.StartStunts();yield return new WaitUntil(()=>game.state==Game.State.Race);car=game.player;car.manualTest=true;Check(game.track.isStunts&&!game.rushMode&&game.racers.Count==1,"STUNTS supports zero opponents");')
print('Integrated detail update')
