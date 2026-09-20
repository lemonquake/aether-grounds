using UnityEngine;
namespace Aether {
 public static class WeaponCatalog {
  public static readonly string[] Items={"Boost","Shield","Rocket","Pulse","Ice Blast","EMP","Oil Slick","Gravity Mine","Shockwave"};
  public static bool IsExtra(string item)=>System.Array.IndexOf(Items,item)>=4;
 }
 public partial class Vehicle {
  public float frozenFor,empFor,oilFor;GameObject ice;float iceAge;
  public void Freeze(){
   if(BlockStatus())return;InterruptDrivingSkills();frozenFor=1.5f;iceAge=0;body.linearVelocity*=.15f;
   if(!ice){ice=new GameObject("Ice enclosure");ice.transform.SetParent(transform,false);ice.transform.localPosition=Vector3.up*.8f;
    var mat=new Material(Shader.Find("Aether/Ice"));
    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;CarParts.Part(ice.transform,"Growing ice crystal",PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*1.05f,0,Mathf.Cos(a)*1.8f),new Vector3(.75f,1.9f,1.15f),mat,new Vector3(i%2*8,45*i,12));}
   }
   RaceEffects.Ring(transform.position,new Color(.5f,.9f,1),7);RaceEffects.Burst(transform.position+Vector3.up,Color.cyan,40,5,.24f,.8f);if(player)game.Toast("Frozen · ice breaks in 1.5 seconds");
  }
  public bool BlockStatus(){if(shield<=0)return false;shield=0;RefreshShieldMass();RaceEffects.Ring(transform.position,Color.cyan,4);return true;}
  public void ClearWeaponStatus(){frozenFor=empFor=oilFor=0;BreakIce();}
  void BreakIce(){if(!ice)return;RaceEffects.Burst(transform.position+Vector3.up,new Color(.68f,.93f,1),48,9,.23f,.7f,1);RaceEffects.Debris(transform.position+Vector3.up,new Color(.4f,.82f,.95f),18,7);var mat=ice.GetComponentInChildren<Renderer>().sharedMaterial;Destroy(ice);Destroy(mat);ice=null;}
  void UpdateWeaponStatus(){
   if(frozenFor>0){frozenFor=Mathf.Max(0,frozenFor-Time.deltaTime);iceAge+=Time.deltaTime;if(ice){ice.transform.localScale=Vector3.one*Mathf.SmoothStep(.12f,1,iceAge/.16f);ice.transform.localRotation=Quaternion.Euler(0,0,frozenFor<.25f?Mathf.Sin(Time.time*95)*2:0);}if(frozenFor<=0)BreakIce();}
   empFor=Mathf.Max(0,empFor-Time.deltaTime);oilFor=Mathf.Max(0,oilFor-Time.deltaTime);
   if(empFor>0){boost=0;energy=Mathf.Min(energy,10);}
  }
  void UseExtraWeapon(string used){
   if(used=="Oil Slick"||used=="Gravity Mine"){var o=new GameObject(used);o.transform.SetParent(track.transform);o.transform.position=transform.position-transform.forward*4;var h=o.AddComponent<WeaponHazard>();h.Initialize(game,this,used=="Gravity Mine");return;}
   if(!WeaponCatalog.IsExtra(used))return;
   Color color=used=="Ice Blast"?new Color(.55f,.9f,1):used=="EMP"?new Color(.65f,.35f,1):new Color(1,.66f,.22f);
   float range=used=="Ice Blast"?23:used=="EMP"?27:32;
   RaceEffects.Ring(transform.position,color,range);RaceEffects.Burst(transform.position+Vector3.up,color,64,14,.25f,.7f);
   foreach(var v in game.racers){if(v==this||v.finished)continue;Vector3 to=v.transform.position-transform.position;if(to.magnitude>range)continue;
    if(used=="Ice Blast"){if(Vector3.Dot(transform.forward,to.normalized)>-.2f)v.Freeze();}
    else if(used=="EMP"){if(!v.BlockStatus()){v.empFor=3;v.boost=0;v.energy=Mathf.Min(v.energy,10);RaceEffects.Ring(v.transform.position,color,4);}}
    else v.Hit(to.normalized*18+Vector3.up*7);
   }
   if(used=="Shockwave")CityPhysics.Blast(transform.position,range,34,this);
  }
 }
 public class WeaponHazard:MonoBehaviour {
  readonly System.Collections.Generic.HashSet<Vehicle> immune=new System.Collections.Generic.HashSet<Vehicle>();
  Game game;Vehicle owner;bool mine,triggered;float age,detonation;GameObject visual;
  public void Initialize(Game g,Vehicle car,bool gravity){game=g;owner=car;mine=gravity;
   if(Physics.Raycast(transform.position+Vector3.up*4,Vector3.down,out var hit,10,~((1<<8)|(1<<9)|(1<<10))))transform.position=hit.point+Vector3.up*.08f;
   visual=CarParts.Part(transform,mine?"Gravity core":"Oil puddle",mine?PrimitiveType.Sphere:PrimitiveType.Cylinder,Vector3.zero,mine?Vector3.one*.85f:new Vector3(6,.025f,9),ModelLibrary.Material(mine?"Gravity core":"Oil puddle",mine?new Color(.55f,.12f,.9f):new Color(.025f,.03f,.045f),mine?1.4f:0));
  }
  void Update(){if(!game||!game.RaceSimulationActive)return;age+=Time.deltaTime;if(!mine&&age>12){Destroy(gameObject);return;}
   if(mine){visual.transform.localScale=Vector3.one*(.85f+Mathf.Sin(age*8)*.12f);visual.transform.localPosition=Vector3.up*(.4f+Mathf.Sin(age*3)*.15f);}
   if(age<.65f)return;
   foreach(var car in game.racers){if(car==owner||car.finished)continue;var delta=transform.position-car.transform.position;float distance=delta.magnitude;
    if(!mine){if(distance<4&&!immune.Contains(car)){if(car.BlockStatus())immune.Add(car);else car.oilFor=2.5f;}continue;}
    if(distance<11&&!triggered){triggered=true;RaceEffects.Ring(transform.position,new Color(.7f,.2f,1),12);}
    if(triggered&&distance<15&&car.shield<=0)car.body.AddForce(delta.normalized*22,ForceMode.Acceleration);
   }
   if(triggered){detonation+=Time.deltaTime;if(detonation>.85f){RaceEffects.Explosion(transform.position,.9f,owner);foreach(var car in game.racers)if(car!=owner&&Vector3.Distance(car.transform.position,transform.position)<15)car.Hit((car.transform.position-transform.position).normalized*18+Vector3.up*9);Destroy(gameObject);}}
  }
 }
}
