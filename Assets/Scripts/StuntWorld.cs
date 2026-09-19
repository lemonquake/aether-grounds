using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class TrackWorld {
  public bool isStunts;public readonly List<StuntTile> stuntTiles=new List<StuntTile>();
  public static readonly float[] StuntGates={45,1380,2350,3440,4300,4750};
  public static bool StuntGap(float z)=>(z>1450&&z<2050)||(z>3520&&z<3930);
  public static float StuntHeight(float z){
   if(z<100)return 1600;
   if(z<1150)return Mathf.Lerp(1600,600,Mathf.SmoothStep(0,1,(z-100)/1050));
   if(z<1450){float t=(z-1150)/300;return 600+135*t*t;}
   if(z<2050)return Mathf.Lerp(735,650,(z-1450)/600);
   if(z<2500)return 650;
   if(z<3290)return Mathf.Lerp(650,80,Mathf.SmoothStep(0,1,(z-2500)/790));
   if(z<3520){float t=(z-3290)/230;return 80+100*t*t;}
   if(z<3930)return Mathf.Lerp(180,120,(z-3520)/410);
   return 120;
  }
  public static float StuntX(float z)=>z<3930?0:Mathf.Sin(Mathf.Clamp01((z-3930)/870)*Mathf.PI*2)*38;
  public static float StuntWidth(float z)=>z<80?36:z>=2050&&z<2450?150:z>=3930&&z<4350?110:z>4350?20:26;
  public Vector3 StuntPosition(float z)=>new Vector3(StuntX(z),StuntHeight(z),z);
  public void GenerateStunts(){isStunts=true;map=0;length=4800;rng=new System.Random(491);
   for(int i=0;i<Count;i++)points[i]=StuntPosition(i*length/(Count-1));
   var road=ModelLibrary.Material("Heartstopper road",new Color(.1f,.15f,.21f));var rail=ModelLibrary.Material("Heartstopper rail",new Color(.23f,.32f,.4f));var glow=ModelLibrary.Material("Heartstopper cyan",new Color(.1f,.85f,.91f),.7f);var white=ModelLibrary.Material("Heartstopper white",new Color(.82f,.92f,.92f));
   road.SetFloat("_Surface",1);road.SetFloat("_Smoothness",.22f);road.SetTexture("_MainTex",Resources.Load<Texture2D>("Textures/DowntownAsphalt"));road.SetFloat("_TextureMix",.35f);road.SetFloat("_TextureScale",.18f);
   // Exact gap edges keep a continuous launch lip; dense strips soften suspension transitions.
   var samples=new SortedSet<float>();for(float z=-45;z<=4800;z+=5)samples.Add(z);foreach(float z in new[]{80,100,1150,1380,1450,2050,2350,2450,2500,3290,3440,3520,3930,4300,4350,4750,4800})samples.Add(z);var zs=new List<float>(samples);
   for(int i=0;i<zs.Count-1;i++){float a=zs[i],b=zs[i+1],mid=(a+b)*.5f;if(StuntGap(mid))continue;Vector3 p=StuntPosition(a),q=StuntPosition(b);float width=StuntWidth(mid)*.5f;Vector3 right=Vector3.Cross(Vector3.up,(q-p).normalized).normalized;
    MeshObject("Heartstopper road",new[]{p-right*width,p+right*width,q-right*width,q+right*width},new[]{0,2,1,1,2,3},road,true,transform);
    for(int s=-1;s<=1;s+=2){var edge=Cube("Track edge",(p+q)*.5f+right*s*(width-.25f)+Vector3.up*.07f,new Vector3(.3f,.12f,Vector3.Distance(p,q)+.1f),glow,false);edge.transform.rotation=Quaternion.LookRotation(q-p);edge.transform.SetParent(transform);
     if(mid<1350||mid>2450&&mid<3390||mid>4350){var guard=Cube("Stunt guardrail",(p+q)*.5f+right*s*(width+.25f)+Vector3.up*.65f,new Vector3(.4f,1.3f,Vector3.Distance(p,q)+.1f),rail,true);guard.transform.rotation=Quaternion.LookRotation(q-p);guard.transform.SetParent(transform);}
    }
    if(i%5==0){var mark=Cube("Road direction",(p+q)*.5f+Vector3.up*.035f,new Vector3(.23f,.025f,3),white,false);mark.transform.rotation=Quaternion.LookRotation(q-p);mark.transform.SetParent(transform);}
   }
   for(int i=0;i<StuntGates.Length;i++)BuildStuntGate(StuntGates[i],i==0?"Start":i==5?"Finish":"Checkpoint "+i,glow);
   foreach(float z in new[]{160,430,900,1290,2590,2870,3220,3360,4020,4540})MakeStuntTile(z,z>2500?7:0,StuntTileKind.Boost);
   MakeStuntTile(2670,-6,StuntTileKind.Slippery);MakeStuntTile(2800,6,StuntTileKind.Water);MakeStuntTile(3010,-6,StuntTileKind.Jump);MakeStuntTile(3130,6,StuntTileKind.Slippery);MakeStuntTile(4440,-4,StuntTileKind.Water);MakeStuntTile(4600,4,StuntTileKind.Jump);
   foreach(float z in new[]{2730,2940,3100,4240,4490}){MakeStuntTile(z,(int)z%3==0?-6:6,StuntTileKind.Trap);}
   foreach(float z in new[]{500,1100,2320,2580,3210,4100,4450})pickups.Add(StuntPosition(z)+Vector3.up*1.6f);
   var ocean=Cube("Heartstopper ocean",new Vector3(0,-7,2300),new Vector3(15000,1,16000),new Material(Shader.Find("Aether/RushOcean")),false);ocean.transform.SetParent(transform);Presentation.Lighting(0);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00013f;RenderSettings.fogColor=new Color(.43f,.62f,.75f);var sky=new Material(Shader.Find("Aether/RushSky"));sky.SetColor("_Top",new Color(.12f,.31f,.54f));sky.SetColor("_Horizon",new Color(.63f,.82f,.93f));RenderSettings.skybox=sky;var sun=GameObject.Find("Sun").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(32,-35,0);sun.intensity=1.15f;Presentation.Probe(transform,new Vector3(0,1604,30));CombineRushSector(transform);
  }
  void BuildStuntGate(float z,string label,Material glow){var root=new GameObject(label).transform;root.SetParent(transform);root.position=StuntPosition(z);for(int s=-1;s<=1;s+=2)CarParts.Part(root,"Gate light",PrimitiveType.Cube,new Vector3(s*12,5,0),new Vector3(.6f,10,.6f),glow);CarParts.Part(root,"Gate beam",PrimitiveType.Cube,new Vector3(0,10,0),new Vector3(24.5f,1.4f,.7f),ModelLibrary.Material("Stunt gate beam",new Color(.03f,.065f,.1f)));var text=new GameObject("Gate name").AddComponent<TextMesh>();text.transform.SetParent(root,false);text.transform.localPosition=new Vector3(0,10,-.5f);text.text=label;text.fontSize=72;text.characterSize=.35f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;}
  void MakeStuntTile(float z,float lane,StuntTileKind kind){var o=new GameObject(kind+" stunt tile");o.transform.SetParent(transform);o.transform.position=StuntPosition(z)+Vector3.right*lane+Vector3.up*.06f;o.transform.rotation=Quaternion.LookRotation(StuntPosition(z+2)-StuntPosition(z-2));var t=o.AddComponent<StuntTile>();t.kind=kind;t.Build();stuntTiles.Add(t);}
  public bool StuntCorridor(Vector3 p){if(p.z< -60||p.z>4900)return false;if(StuntGap(p.z))return Mathf.Abs(p.x)<160&&p.y>StuntHeight(p.z)-200;return Mathf.Abs(p.x-StuntX(p.z))<StuntWidth(p.z)*.5f+2&&p.y>StuntHeight(p.z)-8;}
 }
 public enum StuntTileKind {Boost,Slippery,Jump,Water,Trap}
 public class StuntTile:MonoBehaviour {
  Material trapMaterial;public StuntTileKind kind;readonly Dictionary<Vehicle,float> next=new Dictionary<Vehicle,float>();
  public bool Contains(Vector3 p){p=transform.InverseTransformPoint(p);return Mathf.Abs(p.x)<4.5f&&Mathf.Abs(p.z)<12&&p.y>-.5f&&p.y<2.4f;}
  public void Build(){var color=new[]{new Color(.1f,1,.5f),new Color(.58f,.45f,1),new Color(1,.7f,.13f),new Color(.12f,.65f,1),new Color(1,.15f,.1f)}[(int)kind];var mat=ModelLibrary.Material("Stunt tile "+kind,color,.35f);CarParts.Part(transform,"Tile surface",PrimitiveType.Cube,Vector3.zero,new Vector3(9,.08f,24),kind==StuntTileKind.Water?new Material(Shader.Find("Aether/Water")):mat);
   for(int j=0;j<5;j++)for(int s=-1;s<=1;s+=2)CarParts.Part(transform,"Tile arrow",PrimitiveType.Cube,new Vector3(s*1.3f,.09f,-8+j*4),new Vector3(.23f,.05f,3.3f),ModelLibrary.Material("Stunt arrow",Color.white,.2f),new Vector3(0,-s*45,0));
   if(kind==StuntTileKind.Trap)trapMaterial=mat;if(kind==StuntTileKind.Trap)for(int j=0;j<4;j++){var spike=CarParts.Part(transform,"Retractable trap",PrimitiveType.Cube,new Vector3(j*2-3,.55f,0),new Vector3(.4f,1.1f,1.4f),mat,new Vector3(0,0,30));}
   var label=new GameObject("Tile sign").AddComponent<TextMesh>();label.transform.SetParent(transform,false);label.transform.localPosition=new Vector3(0,2.8f,-12);label.text=kind.ToString();label.fontSize=64;label.characterSize=.18f;label.anchor=TextAnchor.MiddleCenter;label.color=color;
  }
  void Update(){if(trapMaterial)trapMaterial.color=Mathf.Repeat(Time.time,4)>1.4f?new Color(1,.08f,.035f):new Color(.2f,.09f,.055f);}
  public void Apply(Vehicle v){if(!Contains(v.transform.position))return;if(kind==StuntTileKind.Water){v.body.AddForce(-v.body.linearVelocity*.55f,ForceMode.Acceleration);v.wet=true;}
   if(kind==StuntTileKind.Slippery)v.oilFor=Mathf.Max(v.oilFor,.5f);if(next.TryGetValue(v,out float time)&&Time.time<time)return;next[v]=Time.time+1.2f;
   if(kind==StuntTileKind.Boost){v.boost=2;v.body.AddForce(transform.forward*12,ForceMode.VelocityChange);}
   if(kind==StuntTileKind.Jump)v.Launch(13);
   if(kind==StuntTileKind.Water)RaceEffects.Splash(v.transform.position,30);
   if(kind==StuntTileKind.Trap&&Mathf.Repeat(Time.time,4)>1.4f)v.PushHit(Vector3.up*14+transform.right*7);
   if(v.player)v.NotifyTile(kind==StuntTileKind.Trap?"Trap · time your crossing":kind+" tile");
  }
 }
 public partial class Vehicle {
  public int stuntCheckpoint;public float stuntAirTime;
  void StuntPhysics(){if(!track.isStunts)return;
   foreach(var tile in track.stuntTiles)tile.Apply(this);
   float courseZ=transform.position.z;
   if(speed>10&&!TrackWorld.StuntGap(courseZ)&&rolloverLock<=0&&Vector3.Dot(transform.up,Vector3.up)>.15f&&Physics.Raycast(body.position+Vector3.up*.4f,Vector3.down,out var surface,3,1,QueryTriggerInteraction.Ignore)){
    body.AddForce(-surface.normal*32,ForceMode.Acceleration);
    if(grounded>=2){var heading=Vector3.ProjectOnPlane(transform.forward,surface.normal).normalized;if(heading.sqrMagnitude>.1f)body.MoveRotation(Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(heading,surface.normal),120*Time.fixedDeltaTime));}
   }
   if(grounded<2){stuntAirTime+=Time.fixedDeltaTime;if(rolloverLock<=0){body.AddRelativeTorque(new Vector3(throttle*.8f,steer*1.6f,-steer*.7f),ForceMode.Acceleration);body.angularDamping=1.2f;}}else{stuntAirTime=0;body.angularDamping=2.1f;}
   // Preserve gravity-driven speed on descents while bounding terminal speed for reliable landings.
   float z=transform.position.z;float cap=z>1300&&z<1450?Mathf.Lerp(125,87,Mathf.Clamp01((z-1300)/85)):z>3380&&z<3520?Mathf.Lerp(125,70,Mathf.Clamp01((z-3380)/80)):125;
   if(body.linearVelocity.magnitude>cap)body.linearVelocity=Vector3.ClampMagnitude(body.linearVelocity,cap);
  }
  void StuntProgress(){if(!track.isStunts)return;float z=transform.position.z;
   if(stuntCheckpoint<TrackWorld.StuntGates.Length-1&&z>=TrackWorld.StuntGates[stuntCheckpoint]&&z<TrackWorld.StuntGates[stuntCheckpoint]+100&&grounded>=2&&track.StuntCorridor(transform.position)){
    stuntCheckpoint++;if(player)game.Toast(stuntCheckpoint==6?"Heartstopper complete":"Checkpoint saved · "+stuntCheckpoint+" / 6");
   }
   progress=stuntCheckpoint*70+Mathf.Clamp(z,0,4800)/4800*40;

  }
 }
}

