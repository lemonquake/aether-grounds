using System;
using UnityEngine;
namespace Aether {
 public partial class Game {
  public GarageSave garage=new GarageSave();public SavedCar CurrentCar=>garage.cars[garage.selected];
  string garageTab="Paint",paintTarget="Body",bodyTab="Wing";float zoom=8.5f,viewHeight=2.8f;bool autoRotate,showPartWeights;Vector2 partWeightScroll;Texture2D rounded;Texture2D[] mapThumbs=new Texture2D[4];
  public void LoadGarage(){
   try{if(PlayerPrefs.HasKey("garage-v2"))garage=JsonUtility.FromJson<GarageSave>(PlayerPrefs.GetString("garage-v2"));}catch(Exception){garage=new GarageSave();}
   if(garage==null||garage.cars==null)garage=new GarageSave();
   garage.cars.RemoveAll(car=>car==null);
   if(garage.cars.Count==0)garage.cars.Add(new SavedCar{body=Mathf.Clamp(PlayerPrefs.GetInt("car",0),0,9),paint=Mathf.Clamp(PlayerPrefs.GetInt("paint",1),0,31),wheelColor=new[]{24,22,4}[Mathf.Clamp(PlayerPrefs.GetInt("wheels",0),0,2)],glass=Mathf.Clamp(PlayerPrefs.GetInt("glass",0),0,2),engineLevel=Mathf.Clamp(PlayerPrefs.GetInt("engine",0),0,20),tireLevel=Mathf.Clamp(PlayerPrefs.GetInt("tires",0),0,20),brakeLevel=Mathf.Clamp(PlayerPrefs.GetInt("brakes",0),0,20)});
   if(!PlayerPrefs.HasKey("garage-v2"))garage.cars[0].name="My "+CarSpec.All[garage.cars[0].body].name;
   garage.selected=Mathf.Clamp(garage.selected,0,garage.cars.Count-1);foreach(var car in garage.cars)car.Validate();SyncCar();credits=PlayerPrefs.GetInt("credits",1200);volume=PlayerPrefs.GetFloat("volume",.55f);
  }
  void SyncCar(){var c=CurrentCar;selectedCar=c.body;paintIndex=c.paint;wheelIndex=c.wheelColor;windowIndex=c.glass;engineLevel=c.engineLevel;tireLevel=c.tireLevel;brakeLevel=c.brakeLevel;}
  void StoreCar(){var c=CurrentCar;c.body=selectedCar;c.paint=paintIndex;c.wheelColor=wheelIndex;c.glass=windowIndex;c.engineLevel=engineLevel;c.tireLevel=tireLevel;c.brakeLevel=brakeLevel;}
  void Changed(){SyncCar();ShowCar();Save();}
  public bool AddCar(){if(garage.cars.Count>=8)return false;StoreCar();garage.cars.Add(new SavedCar{body=garage.cars.Count%10,name="My "+CarSpec.All[garage.cars.Count%10].name,paint=(garage.cars.Count*4+1)%Paints.Length});garage.selected=garage.cars.Count-1;Changed();return true;}
  void GarageInput(){if(state!=State.Menu)return;Vector2 m=ScreenToUI(Input.mousePosition);if(menu=="Garage"&&new Rect(365,240,675,465).Contains(m)){if(phoneMode&&Input.touchCount>0){var touch=Input.GetTouch(0);if(touch.phase==TouchPhase.Moved){orbit-=touch.deltaPosition.x*.22f;viewHeight=Mathf.Clamp(viewHeight-touch.deltaPosition.y*.006f,1.2f,5);autoRotate=false;}if(Input.touchCount>1){var second=Input.GetTouch(1);float previous=Vector2.Distance(touch.position-touch.deltaPosition,second.position-second.deltaPosition);float current=Vector2.Distance(touch.position,second.position);zoom=Mathf.Clamp(zoom-(current-previous)*.015f,5.8f,11);}}else if(Input.GetMouseButton(0)){orbit-=Input.GetAxis("Mouse X")*5;viewHeight=Mathf.Clamp(viewHeight-Input.GetAxis("Mouse Y")*.08f,1.2f,5);autoRotate=false;}zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*.5f,5.8f,11);}if(menu!="Garage"||autoRotate)orbit+=Time.deltaTime*7;}
  void Card(float x,float y,float w,float h,Color? color=null){GUI.color=color??new Color(.045f,.065f,.09f,.97f);GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=Color.white;Line(x,y,w,new Color(.19f,.28f,.35f,.8f));}
  void NewMenuGUI(){
   Panel(0,0,1600,98,.97f);Label("Aether Grounds",32,23,390,53,34,null,true);Label("By Aljay Leodones",34,69,330,23,16,muted);
   string[] tabs={"Play","Game Setup","Garage","Controls","Store"};for(int i=0;i<tabs.Length;i++)if(Button(tabs[i],455+i*163,26,153,48,menu==tabs[i])){menu=tabs[i];orbit=-32;}
   Label(credits.ToString("N0")+" credits",1300,38,270,35,23,cyan,true);
   if(menu=="Store"||menu=="Support"){SupportGUI();return;}if(menu=="Garage"){NewGarageGUI();return;}
   if(menu=="Game Setup"){SetupGUI();return;}if(menu=="Controls"){ControlsGUI();return;}
   Card(32,131,432,459);Label("Play your next race",58,160,380,110,47,null,true);Label("10 cars. Four unique circuits.\nBuild a car and choose your race.",61,287,360,73,23,muted);
   Line(61,381,372,cyan);Label(CarSpec.All[selectedCar].name+"  ·  "+CurrentCar.name,61,405,365,65,24,null,true);
   if(Button("Play",60,507,233,56,true))StartRace();if(Button("Garage",306,507,130,56)){menu="Garage";orbit=-32;}
   Card(1224,131,344,235);Label("Race settings",1247,153,305,43,28,null,true);Label(aiCount+" opponents  ·  "+laps+" laps",1247,209,302,38,21,cyan);Label(new[]{"Easy","Normal","Hard"}[difficulty]+" difficulty\n"+(powerups?"Power-ups enabled":"Power-ups disabled"),1247,252,294,72,21,muted);
   MapQuirks(1224,384,344);Label("Choose a circuit",34,617,600,45,29,null,true);
   CircuitCards(674,191);
  }
  void DrawRoute(int index,Rect rect){
   if(!mapThumbs[index]){var tex=new Texture2D(180,180,TextureFormat.RGBA32,false);Color bg=new Color(.03f,.05f,.075f);var pixels=new Color[180*180];for(int j=0;j<pixels.Length;j++)pixels[j]=bg;Color route=new[]{new Color(.19f,.84f,.77f),new Color(1,.6f,.26f),new Color(.7f,.48f,1),new Color(.68f,.84f,.32f)}[index];var cp=index==0?PhilippineCity.CityShape:index==1?JsonUtility.FromJson<NeighborhoodData>(Resources.Load<TextAsset>("Malasugue").text).route:index==3?TrackWorld.AirahShape:index==3?TrackWorld.AirahShape:TrackWorld.Shapes[index];for(int j=0;j<1200;j++){float t=j/(float)1200*cp.Length;int n=(int)t;float f=t-n;Vector3 a=cp[(n+cp.Length-1)%cp.Length],b=cp[n],c=cp[(n+1)%cp.Length],d=cp[(n+2)%cp.Length];Vector3 pos=.5f*((2*b)+(-a+c)*f+(2*a-5*b+4*c-d)*f*f+(-a+3*b-3*c+d)*f*f*f);int x=Mathf.Clamp(Mathf.RoundToInt(90+pos.x*(index==3?.06f:.21f)),3,176),y=Mathf.Clamp(Mathf.RoundToInt(90+pos.z*(index==3?.06f:.21f)),3,176);for(int dx=-2;dx<=2;dx++)for(int dy=-2;dy<=2;dy++)pixels[(y+dy)*180+x+dx]=route;}tex.SetPixels(pixels);tex.Apply();mapThumbs[index]=tex;}GUI.DrawTexture(rect,mapThumbs[index]);
  }
  void NewGarageGUI(){
   Panel(0,98,1600,130,.94f);Panel(0,754,1600,146,.96f);Label("Your garage",32,116,370,45,30,null,true);Label("Each car keeps its own parts, colors and upgrades.",359,127,920,34,20,muted);
   for(int i=0;i<garage.cars.Count;i++){float x=32+i*170;if(Button((i+1)+" · "+CarSpec.All[garage.cars[i].body].name,x,172,158,45,garage.selected==i)){StoreCar();garage.selected=i;Changed();}}
   if(garage.cars.Count<8&&Button("+ Add car",32+garage.cars.Count*170,172,158,45,true))AddCar();
   var c=CurrentCar;var spec=SupportStore.Tuned(CarSpec.All[c.body],c);var assembled=VehicleBuild.Calculate(c,inventory);
   Card(32,239,321,507);Label(spec.name,54,259,280,58,41,null,true);Label(spec.description,56,327,272,65,19,muted);
   string oldName=c.name;Label("Car name",55,402,270,28,18,muted);c.name=GUI.TextField(new Rect(55,437,276,39),c.name,24,new GUIStyle(GUI.skin.textField){fontSize=21,padding=new RectOffset(9,7,7,5)});if(c.name!=oldName)Save();
   Bar("Estimated top speed",assembled.estimatedTopSpeed*3.6f,355,55,493,276," km/h");Bar("Launch acceleration",assembled.estimatedAcceleration,18,55,553,276," m/s²");
   Label(assembled.mass.ToString("N1")+" kg  ·  "+assembled.powerKW.ToString("N0")+" kW",55,618,280,33,22,cyan,true);Label("Power / weight: "+(assembled.powerKW/assembled.mass*1000).ToString("N0")+" kW/t",55,655,280,30,18,muted);
   if(Button(showPartWeights?"Close part weights":"View part weights",55,695,276,38,showPartWeights))showPartWeights=!showPartWeights;
   Card(1054,239,514,507);Label("Customize",1076,255,466,42,28,null,true);
   string[] categories={"Paint","Wheels","Engine","Body parts","Suspension","Upgrades"};for(int i=0;i<categories.Length;i++){float x=1078+(i%3)*153;var oldFont=button.fontSize;button.fontSize=19;if(Button(categories[i],x,307+(i/3)*44,143,38,garageTab==categories[i]))garageTab=categories[i];button.fontSize=oldFont;}
   if(garageTab=="Paint"){
    string[] targets={"Body","Trim","Wheel"};for(int i=0;i<3;i++)if(Button(targets[i],1078+i*153,403,143,36,paintTarget==targets[i]))paintTarget=targets[i];
    int chosen=paintTarget=="Body"?c.paint:paintTarget=="Trim"?c.accent:c.wheelColor;Label(PaintNames[chosen],1079,444,451,30,21,cyan);
    for(int i=0;i<Paints.Length;i++){float x=1080+i%8*57,y=477+i/8*40;GUI.backgroundColor=Paints[i];if(GUI.Button(new Rect(x,y,47,36),i==chosen?"●":"",button)){if(paintTarget=="Body")c.paint=i;else if(paintTarget=="Trim")c.accent=i;else c.wheelColor=i;Changed();}GUI.backgroundColor=Color.white;}
    Choice("Paint style",ref c.paintStyle,PaintStyles.Names,1078,640);
   }else if(garageTab=="Wheels"){
    Choice("Wheel design",ref c.wheel,CarParts.Wheels,1078,405);Choice("Window tint",ref c.glass,CarParts.GlassNames,1078,515);
    if(Button("Choose wheel color",1078,624,458,43)){garageTab="Paint";paintTarget="Wheel";}Label(assembled.wheelMass.ToString("F1")+" kg per wheel  ·  "+(assembled.radius*200).ToString("F0")+" cm diameter\nHeavier wheels take more torque to accelerate.",1078,677,458,58,18,muted);
   }else if(garageTab=="Engine"){
    Choice("Powertrain",ref c.engine,CarParts.Engines,1078,405);Label(assembled.powerKW.ToString("N0")+" kW  ·  "+assembled.peakTorque.ToString("N0")+" Nm peak\n\nEngine choice changes power, mass and balance. Top speed also depends on drag, tires and gearing.",1078,515,454,143,21,muted);
    if(Button("Inspect engine",1078,671,458,44)){orbit=-10;viewHeight=4.3f;zoom=5.8f;}
   }else if(garageTab=="Body parts"){
    string[] parts={"Wing","Exhaust","Livery"};for(int i=0;i<3;i++)if(Button(parts[i],1078+i*153,407,143,40,bodyTab==parts[i]))bodyTab=parts[i];
    if(bodyTab=="Wing")Choice("Rear wing",ref c.spoiler,CarParts.Spoilers,1078,470);if(bodyTab=="Exhaust")Choice("Exhaust model",ref c.exhaust,CarParts.Exhausts,1078,470);if(bodyTab=="Livery")Choice("Body graphics",ref c.livery,CarParts.Liveries,1078,470);
    Label("Every installed part contributes to total mass. Wings also change downforce and drag.\n\nTrim paint colors accessories and graphics.",1078,582,458,140,20,muted);
   }else if(garageTab=="Suspension"){
    Choice("Spring tension",ref c.springTune,VehicleBuild.SpringNames,1078,402);Choice("Bounce damping",ref c.damperTune,VehicleBuild.DamperNames,1078,503);Choice("Chassis",ref c.chassis,VehicleBuild.ChassisNames,1078,604);
    Label((assembled.travel*100).ToString("F0")+" cm travel  ·  "+(assembled.springRate/1000).ToString("F1")+" kN/m per corner",1078,710,458,28,18,muted);
   }else {
    UpgradeRow("Engine",ref c.engineLevel,1078,402);UpgradeRow("Tires",ref c.tireLevel,1078,483);UpgradeRow("Brakes",ref c.brakeLevel,1078,564);UpgradeRow("Suspension",ref c.suspensionLevel,1078,645);
   }
   Panel(450,637,543,37,.88f);Label("Drag to rotate · scroll to zoom",461,640,551,33,19,muted);
   if(Button("Front",398,691,117,40)){orbit=0;zoom=8.5f;viewHeight=2.8f;}if(Button("Side",526,691,117,40)){orbit=90;zoom=8.5f;viewHeight=2.5f;}if(Button("Rear",654,691,117,40)){orbit=180;zoom=8.5f;viewHeight=2.8f;}if(Button(autoRotate?"Stop rotation":"Rotate",782,691,230,40,autoRotate))autoRotate=!autoRotate;
   if(showPartWeights){Card(374,239,655,434);Label("Installed parts",397,256,595,38,27,null,true);Label("Total "+assembled.mass.ToString("N2")+" kg · front load "+(assembled.frontWeight*100).ToString("F0")+"%",397,300,595,31,20,cyan);
    partWeightScroll=GUI.BeginScrollView(new Rect(391,343,621,309),partWeightScroll,new Rect(0,0,590,assembled.parts.Count*36));for(int i=0;i<assembled.parts.Count;i++){var part=assembled.parts[i];Label(part.name,4,i*36,456,33,18,muted);Label(part.kilograms.ToString("F2")+" kg",464,i*36,123,33,18,cyan);}GUI.EndScrollView();}
   Label("Base car",32,767,500,29,22,muted);for(int i=0;i<CarSpec.All.Length;i++){float x=32+i*154;if(Button(CarSpec.All[i].name,x,809,143,55,c.body==i)){c.body=i;c.name="My "+CarSpec.All[i].name;Changed();}Label((i+1).ToString("00"),x+7,869,140,22,16,muted);}
   if(preview&&Event.current.type==EventType.Repaint){/* The model remains directly interactive through the unobstructed center viewport. */}
  }
  void Choice(string label,ref int value,string[] names,float x,float y){Label(label,x,y,451,30,19,muted);if(Button("‹",x,y+39,49,48)){value=(value+names.Length-1)%names.Length;Changed();}Label(names[value],x+64,y+48,322,65,23,cyan,true);if(Button("›",x+409,y+39,49,48)){value=(value+1)%names.Length;Changed();}}
  void UpgradeRow(string label,ref int value,float x,float y){int cost=200+value*35;Label(label+"  "+value+" / 20",x,y,458,33,23,null,true);Label(value==20?"Fully upgraded":cost+" credits",x,y+42,226,31,19,muted);GUI.enabled=value<20&&credits>=cost;if(Button(value==20?"Complete":"Upgrade",x+249,y+34,209,45,true)&&value<20&&credits>=cost){credits-=cost;value++;SyncCar();Save();}GUI.enabled=true;}
 }
}

