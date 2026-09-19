from pathlib import Path
root=Path(__file__).resolve().parents[1]
def edit(name,changes):
 p=root/name;s=p.read_text(encoding='utf-8-sig')
 for a,b in changes:
  if a not in s:raise RuntimeError(f'Missing pattern in {name}: {a[:100]}')
  s=s.replace(a,b)
 p.write_text(s,encoding='utf-8')
def method(s,signature,new):
 start=s.index(signature);brace=s.index('{',start);depth=1;i=brace+1
 while depth:
  if s[i]=='{':depth+=1
  if s[i]=='}':depth-=1
  i+=1
 return s[:start]+new+s[i:]
edit('Assets/Scripts/ModelLibrary.cs',[
 ('m.SetFloat("_Emission",glow);','m.SetFloat("_Metallic",key.Contains("Paint")?.55f:key.Contains("Metal")||key.Contains("Wheel")?.8f:key.Contains("Glass")?.35f:0);m.SetFloat("_Smoothness",key.Contains("Glass")?.93f:key.Contains("Paint")?.78f:key.Contains("Metal")?.7f:.35f);m.SetFloat("_Emission",glow);'),
 ('new CarSpec("Comet","Muscle coupe • strong acceleration"','new CarSpec("Comet","Classic muscle coupe • powerful launch"'),
 ('new CarSpec("Spectre","Long-tail racer • highest top speed",79,11.7f,22,8.9f,1280)','new CarSpec("Spectre","Open-wheel Formula racer • precise steering",79,13.2f,26,10.2f,980),new CarSpec("Atlas","Wide rally pickup • heavy and stable",62,13.5f,25,9.0f,1650),new CarSpec("Pip","Bubble compact • light and nimble",61,15,26,11,820),new CarSpec("Cinder","Open hot rod • explosive acceleration",71,16,22,8.3f,1130),new CarSpec("Relay","Retro van • relaxed and sturdy",60,12.8f,25,9.8f,1480),new CarSpec("Manta","Enclosed prototype • smooth high-speed grip",77,12.7f,24,10.1f,1160)')])
p=root/'Assets/Scripts/ModelLibrary.cs';s=p.read_text();s=method(s,'public static void Customize(','''public static void Customize(GameObject model,Color paint,Color accent,int wheels,int windows){
   foreach(var r in model.GetComponentsInChildren<MeshRenderer>()){
    Color? color=r.name=="Paint"?paint:r.name=="Accent"?accent:r.name=="Wheel"?Game.Paints[Mathf.Clamp(wheels,0,Game.Paints.Length-1)]:r.name=="Glass"?CarParts.Glass[Mathf.Clamp(windows,0,12)]:(Color?)null;
    if(color.HasValue){var m=Material(r.name+" "+color.Value,color.Value);r.sharedMaterial=m;}
   }
  }''');p.write_text(s)

edit('Assets/Scripts/TrackWorld.cs',[
 ('public class TrackWorld :','public partial class TrackWorld :'),
 ('MakeRoad();MakeTerrain();','MakeRoad();MakeTerrain();MakeFeatures();'),
 ('ModelLibrary.Material("water"+index,new Color(.13f,.42f,.54f),.05f,0)','WaterMaterial()'),
 ('lake.transform.SetParent(decor)','lake.transform.SetParent(transform)'),
 ('ModelLibrary.Material("lagoon",new Color(.15f,.67f,.70f),.05f,0)','WaterMaterial()'),
 ('Planet();CombineDecor();','Planet();CombineDecor();Presentation.Lighting(index);Presentation.Probe(transform,points[0]+Vector3.up*5);'),
 ('ModelLibrary.Material("asphalt",new Color(.13f,.19f,.26f),0,0)','RoadMaterial()'),
 ('if(i<Count){int k=i*2;ts.AddRange','if(i<Count&&!Broken(i)){int k=i*2;ts.AddRange'),
 ('for(int s=-1;s<=1;s+=2){\n    for(int i=0;i<Count;i++){','for(int i=0;i<Count;i++)if(Broken(i)){int j=(i+1)%Count;MeshObject("Dry bypass",new[]{points[i]+Right(i)*3.5f,points[i]+Right(i)*12,points[j]+Right(j)*3.5f,points[j]+Right(j)*12},new[]{0,2,1,1,2,3},RoadMaterial(),true,transform);}\n   for(int s=-1;s<=1;s+=2){\n    for(int i=0;i<Count;i++){'),
 ('void MakeTerrain(){','Material RoadMaterial(){var m=ModelLibrary.Material("Road theme "+map,new[]{new Color(.19f,.26f,.29f),new Color(.38f,.24f,.16f),new Color(.22f,.22f,.31f)}[map],0,0);m.SetFloat("_Surface",map+1);m.SetFloat("_Smoothness",map==2?.5f:.24f);return m;}\n  void MakeTerrain(){')])

p=root/'Assets/Scripts/Game.cs';s=p.read_text();s=s.replace('public class Game:','public partial class Game:').replace('aiCount=7','aiCount=11')
start=s.index('public static readonly Color[] Paints=');end=s.index('public string menu=',start)
names=['Graphite','Lagoon','Berry','Sunburst','Pearl','Iris','Crimson','Coral','Tangerine','Lemon','Lime','Forest','Mint','Teal','Sky','Cobalt','Navy','Lavender','Plum','Rose','Blush','Copper','Bronze','Sand','Silver','Slate','Ink','Snow','Cream','Ice','Olive','Chocolate']
hexes=['18212d','20b3b3','bf2b75','f0902e','ced9e3','5740ad','cb1738','f56f62','ff831b','f4cf37','9fcf38','236f48','7fe1bc','167d88','4aaee6','315adb','1c315d','ab92e4','673d80','e25699','e3b0b7','b66940','96804b','c2ae8c','a4b4bf','566774','101923','eeeeed','e5d9b8','a1d8e5','69783c','563d34']
palette=','.join('new Color('+','.join(str(round(int(h[i:i+2],16)/255,4))+'f' for i in (0,2,4))+')' for h in hexes)
s=s[:start]+'public static readonly Color[] Paints={'+palette+'};\n  public static readonly string[] PaintNames={'+','.join('"'+n+'"' for n in names)+'};\n  '+s[end:]
s=s.replace('"No Turn Intended"','"No Turn Intended","Vin Diesel-ish","Duke of Donuts","Tire-d Already","Waffle Throttle","Major Speedbump","Spud Racer","Jean Clutch Van Damme","Biscuit Bandit"')
s=s.replace('Screen.SetResolution(1600,900,FullScreenMode.Windowed);Load();','Screen.SetResolution(1600,900,FullScreenMode.Windowed);testing=Environment.GetCommandLineArgs().Any(a=>a.StartsWith("-aether"));Load();')
s=s.replace('MakeShowroom();string[] args','cam.gameObject.AddComponent<Presentation>();Physics.IgnoreLayerCollision(8,9);Physics.IgnoreLayerCollision(9,9);MakeShowroom();string[] args').replace('testing=args.Contains("-aetherTest");','testing=args.Any(a=>a.StartsWith("-aether"));')
s=method(s,'void Load(){','void Load(){LoadGarage();}')
s=method(s,'void Save(){','''void Save(){StoreCar();if(testing)return;PlayerPrefs.SetString("garage-v2",JsonUtility.ToJson(garage));PlayerPrefs.SetInt("credits",credits);PlayerPrefs.SetFloat("volume",volume);PlayerPrefs.Save();}''')
s=method(s,'void ShowCar(){','''void ShowCar(){if(preview){preview.SetActive(false);Destroy(preview);}preview=ModelLibrary.Create(CarSpec.All[selectedCar].name,showroom.transform);StoreCar();CarParts.Apply(preview,CurrentCar);foreach(var t in preview.GetComponentsInChildren<Transform>())t.gameObject.layer=8;}''')
s=s.replace('ShowCar();\n  }','''var floorMat=floor.GetComponent<Renderer>().sharedMaterial;floorMat.SetFloat("_Surface",4);floorMat.SetFloat("_Smoothness",.64f);floorMat.SetFloat("_Metallic",.25f);
   Presentation.Lighting(-1);
   for(int i=0;i<3;i++){var light=new GameObject("Studio soft light").AddComponent<Light>();light.transform.SetParent(showroom.transform);light.type=LightType.Point;light.range=17;light.intensity=i==0?4:2.4f;light.color=i==1?new Color(.35f,.75f,1):i==2?new Color(1,.7f,.45f):Color.white;light.transform.position=new Vector3(i==0?-4:4,5,i==2?-4:4);}
   Presentation.Probe(showroom.transform,new Vector3(0,2,0));ShowCar();
  }''')
s=s.replace('track.Generate(mapIndex);','track.Generate(mapIndex);track.ConnectFeatures(this);')
s=s.replace('(i-1)%5','(i-1)%CarSpec.All.Length')
s=s.replace('ModelLibrary.Customize(v.model,Paints[paintIndex],paintIndex==0?new Color(.95f,.055f,.11f):new Color(.26f,.93f,.87f),wheelIndex,windowIndex);','CarParts.Apply(v.model,CurrentCar);')
s=s.replace('else ModelLibrary.Customize(v.model,Paints[(i+1)%6],Paints[(i+3)%6],i%3,i%3);','else CarParts.Apply(v.model,new SavedCar{body=v.carIndex,paint=(i*3+1)%Paints.Length,accent=(i*2+4)%Paints.Length,wheel=i%10,wheelColor=(i+4)%Paints.Length,glass=i%13,engine=i%11});')
s=s.replace('var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name="Power-up";Destroy(o.GetComponent<Collider>());o.transform.position=p;o.transform.localScale=Vector3.one*1.3f;o.GetComponent<Renderer>().sharedMaterial=ModelLibrary.Material("pickup",new Color(.25f,.93f,.84f),.3f);','var o=RaceEffects.Crate();o.transform.position=p;')
s=s.replace('if(state==State.Menu)orbit+=Time.deltaTime*5;','GarageInput();')
s=s.replace('if(testing)TestDriver();','if(Environment.GetCommandLineArgs().Contains("-aetherTest"))TestDriver();')
s=s.replace('new Vector3(3.3f,2.5f,7.4f)','new Vector3(0,menu=="Garage"?viewHeight:2.8f,menu=="Garage"?zoom:8.5f)')
s=s.replace('menu=="Play"?-14:0','menu=="Play"?-8:menu=="Garage"?5:0')
s=s.replace('if(state==State.Menu)MenuGUI();','if(state==State.Menu)NewMenuGUI();')
s=s.replace('ref aiCount,0,11','ref aiCount,0,19')
s=s.replace('int award=500+Mathf.Max(0,aiCount+2-finishedCount)*80;','int award=500+Mathf.Max(0,aiCount+2-finishedCount)*80+v.coins*10;')
s=s.replace('MiniMap();Panel','Label(player.coins+" coins · "+(player.coins*100)+" points",636,110,430,36,22,cyan,true);MiniMap();Panel')
s=s.replace('var order=racers.OrderBy(v=>v.place).Take(12).ToArray();','Label(player.coins+" coins · "+player.coins*100+" points · +"+player.coins*10+" coin credits",451,244,680,30,18,cyan);var order=racers.OrderBy(v=>v.place).Take(20).ToArray();')
s=s.replace('453,272+i*30,450,29,20','453+(i/10)*349,283+(i%10)*31,245,29,18').replace('974,272+i*30,180,29,19','699+(i/10)*349,283+(i%10)*31,104,29,16')
s=s.replace('if(level>=3','if(level>=20')
p.write_text(s)

# Detailed crate collection keeps cooldown visuals and fracture effects together.
p=root/'Assets/Scripts/Game.cs';s=p.read_text();start=s.index(' public class Pickup:');s=s[:start]+''' public class Pickup:MonoBehaviour {
  public Game game;public Vector3 home;float cooldown;Renderer[] visuals;
  void Start(){visuals=GetComponentsInChildren<Renderer>();}
  void Update(){if(game.state!=Game.State.Race&&game.state!=Game.State.Countdown)return;transform.rotation=Quaternion.Euler(0,Time.time*42,Mathf.Sin(Time.time)*5);transform.position=home+Vector3.up*Mathf.Sin(Time.time*2+home.x)*.2f;
   if(cooldown>0)cooldown-=Time.deltaTime;bool visible=cooldown<=0;foreach(var r in visuals)if(r)r.enabled=visible;if(!visible)return;
   foreach(var v in game.racers)if(!v.finished&&v.item==""&&Vector3.Distance(v.transform.position+Vector3.up,home)<2.7f){v.item=new[]{"Boost","Shield","Rocket","Pulse"}[UnityEngine.Random.Range(0,4)];cooldown=7;RaceEffects.Debris(home,new Color(.4f,.27f,.12f),10,5);RaceEffects.Burst(home,new Color(.16f,1,.72f),24,5,.2f,.65f);RaceEffects.Ring(home,new Color(.16f,1,.72f),3);foreach(var r in visuals)if(r)r.enabled=false;if(v.player)game.Toast(v.item+" collected · press E");break;}
  }
 }
}
''';p.write_text(s)

edit('Assets/Editor/BuildGame.cs',[
 ('"Vanta","Comet","Miso","Nomad","Spectre","AlienTree"','"Vanta","Comet","Miso","Nomad","Spectre","Atlas","Pip","Cinder","Relay","Manta","AlienTree"'),
 ('PlayerSettings.colorSpace=ColorSpace.Gamma','PlayerSettings.colorSpace=ColorSpace.Linear'),
 ('Shader.Find("Aether/Shield"),','Shader.Find("Aether/Shield"),Shader.Find("Aether/Water"),Shader.Find("Hidden/Aether/Post"),'),
 ('options=BuildOptions.Development','options=BuildOptions.None')])
print('Integration complete')
