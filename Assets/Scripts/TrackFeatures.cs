using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public enum TileKind {Booster,Jump,Ramp}
 public class RoadTile:MonoBehaviour {
  public TileKind kind;public int point;public float halfWidth=4,length=12,launchForce=11.5f,minForwardSpeed,maxForwardSpeed;readonly Dictionary<Vehicle,float> next=new Dictionary<Vehicle,float>();
  public bool Contains(Vector3 position){var p=transform.InverseTransformPoint(position);return Mathf.Abs(p.x)<halfWidth&&Mathf.Abs(p.z)<length*.5f&&p.y>-1&&p.y<4.8f;}
  public void Apply(Vehicle v){if(!Contains(v.transform.position)||Vector3.Dot(v.transform.forward,transform.forward)<.3f)return;if(next.TryGetValue(v,out float time)&&Time.time<time)return;next[v]=Time.time+1.6f;
   if(minForwardSpeed>0){var f=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;float along=Vector3.Dot(v.body.linearVelocity,f);float target=Mathf.Max(along,minForwardSpeed);if(maxForwardSpeed>0)target=Mathf.Min(target,maxForwardSpeed);v.body.linearVelocity+=f*(target-along);}
   if(kind==TileKind.Booster){v.boost=Mathf.Max(v.boost,2);v.body.AddForce(transform.forward*5,ForceMode.VelocityChange);RaceEffects.Burst(v.transform.position,new Color(.15f,1,.75f),16,4,.22f,.45f);}
   else {v.Launch(kind==TileKind.Ramp?10.5f:launchForce);RaceEffects.Burst(v.transform.position,new Color(.65f,.57f,1),20,4,.25f,.5f);}
   if(v.player)v.NotifyTile(kind==TileKind.Booster?"Booster":kind==TileKind.Ramp?"Ramp jump":"Jump tile");
  }
 }
 public partial class TrackWorld {
  public readonly List<RoadTile> tiles=new List<RoadTile>();public readonly List<CoinPickup> coins=new List<CoinPickup>();
  public int[] WaterStarts=>IsAirah?new int[0]:map==1?new int[0]:map==0?new[]{146,174}:new[]{146,328};
  public bool Broken(int i)=>isRush||isStunts||IsAirah?false:map==1?false:map==0?(i>=146&&i<154)||(i>=174&&i<182):(i>=146&&i<154)||(i>=328&&i<336);
  public bool InWater(Vector3 pos,int near){return Broken(near)&&Vector3.Dot(pos-points[near],Right(near))<3.5f&&Mathf.Abs(Vector3.Dot(pos-points[near],Right(near)))<Width*.5f&&pos.y<points[near].y+1.1f;}
  public Material WaterMaterial(){var m=new Material(Shader.Find("Aether/Water"));m.SetColor("_Color",new[]{new Color(.025f,.4f,.46f,.85f),new Color(.19f,.34f,.24f,.85f),new Color(.07f,.18f,.38f,.85f)}[map]);return m;}
  void MakeFeatures(){
   if(IsAirah){foreach(int n in new[]{24,94,180,279,360,435})AddTile(n,TileKind.Booster);foreach(int n in new[]{54,210,389})AddTile(n,TileKind.Jump);AddRamp(112);AddRamp(302);}
   else if(IsAirah){foreach(int n in new[]{24,94,180,279,360,435})AddTile(n,TileKind.Booster);foreach(int n in new[]{54,210,389})AddTile(n,TileKind.Jump);AddRamp(112);AddRamp(302);}
   else if(map==1){AddTile(36,TileKind.Booster);AddTile(221,TileKind.Booster);AddTile(397,TileKind.Jump);foreach(var tile in tiles)tile.transform.localScale=new Vector3(.45f,1,.5f);}
   else {AddTile(36,TileKind.Booster);AddRamp(42);AddTile(221,TileKind.Booster);AddRamp(227);AddTile(101,TileKind.Jump);AddTile(397,TileKind.Jump);AddTile(179,TileKind.Booster);AddTile(420,TileKind.Booster);}
   foreach(int start in WaterStarts){
    var vs=new List<Vector3>();var ts=new List<int>();for(int i=start;i<=start+8;i++){vs.Add(points[i]-Right(i)*11.9f+Vector3.up*.13f);vs.Add(points[i]+Right(i)*3.5f+Vector3.up*.13f);if(i<start+8){int k=(i-start)*2;ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
    MeshObject(map==0?"Flooded coastal washout":map==1?"Flooded canyon crossing":"Flooded garden causeway",vs.ToArray(),ts.ToArray(),WaterMaterial(),false,transform);
    for(int j=0;j<12;j++){int i=start+j%8;float side=j%2==0?-10.8f:3.5f;var o=Cube("Broken road slabs",points[i]+Right(i)*side+Vector3.up*.1f,new Vector3(1.1f,.22f,1.7f),ModelLibrary.Material("Broken stone "+map,map==1?new Color(.56f,.3f,.17f):map==0?new Color(.34f,.42f,.45f):new Color(.27f,.24f,.37f)),false);o.transform.rotation=Quaternion.LookRotation(Forward(i))*Quaternion.Euler(j%3*5,j*17,4);o.transform.SetParent(transform);}
    Sign(start-5,"Water crossing",new Color(.35f,.82f,1));
   }
   for(int i=15;i<Count;i+=9){float lane=Broken(i)?7:Mathf.Sin(i*.12f)*(map==1?1.5f:6);Vector3 p=points[i]+Right(i)*lane+Vector3.up*1.15f;var o=new GameObject("Lap coin");o.transform.SetParent(transform);o.transform.position=p;var coin=o.AddComponent<CoinPickup>();coin.home=p;coins.Add(coin);var gold=ModelLibrary.Material("Coin gold",new Color(1,.64f,.08f),.5f);gold.SetFloat("_Metallic",.72f);CarParts.Part(o.transform,"Coin rim",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.88f,.07f,.88f),gold,new Vector3(90,0,0));CarParts.Part(o.transform,"Coin inset",PrimitiveType.Cylinder,new Vector3(0,0,.078f),new Vector3(.64f,.012f,.64f),ModelLibrary.Material("Coin center",new Color(.87f,.38f,.025f),.22f),new Vector3(90,0,0));CarParts.Part(o.transform,"Coin mark",PrimitiveType.Cube,new Vector3(0,0,.10f),new Vector3(.10f,.42f,.03f),gold);CarParts.Combine(o.transform);}
   if(map==2)MakeLandmarks();
  }
  public void MakeCityRaceFeatures(){MakeFeatures();}
  public void ConnectFeatures(Game game){foreach(var c in coins)c.game=game;}
  void AddTile(int n,TileKind kind){var root=new GameObject(kind.ToString()+" tile");root.transform.SetParent(transform);root.transform.SetPositionAndRotation(points[n]+Vector3.up*.045f,Quaternion.LookRotation(Forward(n)));var tile=root.AddComponent<RoadTile>();tile.kind=kind;tile.point=n;tiles.Add(tile);var color=kind==TileKind.Booster?new Color(.06f,1,.58f):new Color(.65f,.35f,1);var edge=ModelLibrary.Material("Tile "+kind,color,2);CarParts.Part(root.transform,"Tile bed",PrimitiveType.Cube,Vector3.zero,new Vector3(8,.05f,12),ModelLibrary.Material("Tile bed",new Color(.06f,.1f,.15f)));
   for(int s=-1;s<=1;s+=2)CarParts.Part(root.transform,"Edge strip",PrimitiveType.Cube,new Vector3(s*3.92f,.04f,0),new Vector3(.12f,.03f,12),edge);
   for(int j=0;j<4;j++)for(int s=-1;s<=1;s+=2)CarParts.Part(root.transform,"Direction arrow",PrimitiveType.Cube,new Vector3(s*1.05f,.07f,-4.4f+j*2.5f),new Vector3(.35f,.03f,2.9f),edge,new Vector3(0,s*-43,0));CarParts.Combine(root.transform);Sign(n-3,kind==TileKind.Booster?"Boost":"Jump",color);
  }
  void AddRamp(int n){var root=new GameObject("Launch ramp");root.transform.SetParent(transform);root.transform.SetPositionAndRotation(points[n],Quaternion.LookRotation(Forward(n)));Vector3[] vs={new Vector3(-4,0,-9),new Vector3(4,0,-9),new Vector3(-4,2.8f,9),new Vector3(4,2.8f,9),new Vector3(-4,0,9),new Vector3(4,0,9)};int[] ts={0,2,1,1,2,3,2,4,3,3,4,5,0,4,2,1,3,5};MeshObject("Ramp surface",vs,ts,ModelLibrary.Material("Ramp steel",new Color(.26f,.3f,.36f)),true,root.transform);
   for(int j=0;j<7;j++)CarParts.Part(root.transform,"Ramp grip",PrimitiveType.Cube,new Vector3(0,.10f+j*.38f,-8+j*2.45f),new Vector3(7.9f,.045f,.15f),ModelLibrary.Material("Ramp stripe",new Color(1,.66f,.16f),.8f));
   var lip=new GameObject("Ramp launch lip");lip.transform.SetParent(root.transform,false);lip.transform.localPosition=new Vector3(0,2.1f,7);var tile=lip.AddComponent<RoadTile>();tile.point=n;tile.kind=TileKind.Ramp;tile.length=4;tiles.Add(tile);Sign(n-2,"Ramp",new Color(1,.65f,.2f));
  }
  void Sign(int n,string label,Color color){var root=new GameObject(label+" sign").transform;root.SetParent(transform);root.position=points[n]-Right(n)*(map==1?5.3f:14);root.rotation=Quaternion.LookRotation(Forward(n));CarParts.Part(root,"Post",PrimitiveType.Cube,new Vector3(0,2,0),new Vector3(.16f,4,.16f),ModelLibrary.Material("Signpost",new Color(.16f,.2f,.26f)));CarParts.Part(root,"Board",PrimitiveType.Cube,new Vector3(0,3.8f,0),new Vector3(5.8f,1.4f,.18f),ModelLibrary.Material("Sign board",new Color(.025f,.055f,.08f)));var t=new GameObject("Label").AddComponent<TextMesh>();t.transform.SetParent(root,false);t.transform.localPosition=new Vector3(0,3.8f,-.11f);t.transform.localRotation=Quaternion.identity;t.text=label;t.fontSize=64;t.characterSize=.14f;t.anchor=TextAnchor.MiddleCenter;t.color=color;t.gameObject.AddComponent<StructureSign>();}
  void MakeLandmarks(){
   var root=new GameObject("Theme architecture").transform;root.SetParent(transform);
   var warm=ModelLibrary.Material("Canyon sandstone",new Color(.64f,.34f,.18f));warm.SetFloat("_Surface",2);var pale=ModelLibrary.Material("Coastal plaster",new Color(.77f,.83f,.79f));var dark=ModelLibrary.Material("Garden stone",new Color(.19f,.18f,.28f));dark.SetFloat("_Surface",3);
   for(int i=0;i<Count;i+=24){Vector3 p=points[i]+Right(i)*(i%48==0?40:-42);p.y=Height(p.x,p.z);var node=new GameObject("Landmark "+i).transform;node.SetParent(root);node.SetPositionAndRotation(p,Quaternion.LookRotation(Forward(i)));
    if(map==0){CarParts.Part(node,"Beach pavilion",PrimitiveType.Cube,new Vector3(0,2.6f,0),new Vector3(9,5.2f,7),pale);CarParts.Part(node,"Turquoise roof",PrimitiveType.Cube,new Vector3(0,5.4f,0),new Vector3(10,.5f,8),ModelLibrary.Material("Coastal roof",new Color(.04f,.42f,.46f)));for(int j=-1;j<=1;j++)CarParts.Part(node,"Pavilion windows",PrimitiveType.Cube,new Vector3(j*2.5f,3,-3.53f),new Vector3(1.5f,2,.07f),ModelLibrary.Material("Glass",new Color(.04f,.22f,.3f)));}
    else if(map==1){for(int k=0;k<5;k++)CarParts.Part(node,"Layered mesa",PrimitiveType.Cylinder,new Vector3(0,k*4,0),new Vector3(21-k*2.4f,2.3f,17-k*1.9f),k%2==0?warm:ModelLibrary.Material("Canyon strata",new Color(.42f,.20f,.11f)));for(int s=-1;s<=1;s+=2)CarParts.Part(node,"Cactus",PrimitiveType.Capsule,new Vector3(s*11,2.5f,4),new Vector3(.7f,3,.7f),ModelLibrary.Material("Cactus",new Color(.15f,.31f,.15f)));}
    else {for(int s=-1;s<=1;s+=2)CarParts.Part(node,"Garden columns",PrimitiveType.Cylinder,new Vector3(s*5,4,0),new Vector3(1.3f,4,1.3f),dark);CarParts.Part(node,"Garden arch",PrimitiveType.Cube,new Vector3(0,8,0),new Vector3(13,1,2),dark);CarParts.Part(node,"Lantern",PrimitiveType.Sphere,new Vector3(0,6.8f,0),Vector3.one*1.6f,ModelLibrary.Material("Garden lantern",new Color(.67f,.3f,1),3));for(int j=0;j<6;j++)CarParts.Part(node,"Luminous mushroom",PrimitiveType.Sphere,new Vector3(j*2-5,.8f,j%2*3+3),new Vector3(1.8f,.45f,1.8f),ModelLibrary.Material("Mushroom "+j,new Color(.25f+j*.08f,.75f-j*.08f,.8f),.8f));}
   }
   if(map==0){int n=68;var p=points[n]+Right(n)*47;var lighthouse=new GameObject("Lighthouse").transform;lighthouse.SetParent(root);lighthouse.position=new Vector3(p.x,Height(p.x,p.z),p.z);for(int j=0;j<6;j++)CarParts.Part(lighthouse,"Tower band",PrimitiveType.Cylinder,new Vector3(0,j*3+1.5f,0),new Vector3(5-j*.35f,1.5f,5-j*.35f),j%2==0?pale:ModelLibrary.Material("Lighthouse red",new Color(.72f,.18f,.13f)));CarParts.Part(lighthouse,"Beacon",PrimitiveType.Sphere,new Vector3(0,19,0),Vector3.one*2,ModelLibrary.Material("Beacon",new Color(1,.89f,.53f),3));}
   CarParts.Combine(root);
   if(map==2){for(int n=12;n<Count;n+=36){var ps=RaceEffects.Emitter(transform,"Garden fireflies",new Color(.43f,1,.75f,.75f),4,.12f,.3f,24);ps.transform.position=points[n]+Right(n)*15+Vector3.up*2;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(8,3,12);var e=ps.emission;e.rateOverTime=4;}}
  }
 }
 public class CoinPickup:MonoBehaviour {
  public Game game;public Vector3 home;public readonly Dictionary<Vehicle,int> collected=new Dictionary<Vehicle,int>();Renderer[] visuals;
  public bool Collect(Vehicle v){int lap=Mathf.Max(0,v.completedLaps);if(collected.TryGetValue(v,out int last)&&last>=lap)return false;collected[v]=lap;v.coins++;if(v.player){RaceEffects.Burst(home,new Color(1,.76f,.18f),9,3,.18f,.5f);game.Toast("+100 points · "+v.coins+" coins");}return true;}
  void Start(){visuals=GetComponentsInChildren<Renderer>();}
  void Update(){if(!game||!game.RaceSimulationActive)return;transform.rotation=Quaternion.Euler(0,Time.time*100,0);transform.position=home+Vector3.up*Mathf.Sin(Time.time*3+home.x)*.14f;foreach(var v in game.racers)if(!v.finished&&(v.transform.position+Vector3.up*.6f-home).sqrMagnitude<5.8f)Collect(v);bool show=!collected.TryGetValue(game.player,out int lap)||lap<Mathf.Max(0,game.player.completedLaps);foreach(var r in visuals)if(r)r.enabled=show;}
 }
}
