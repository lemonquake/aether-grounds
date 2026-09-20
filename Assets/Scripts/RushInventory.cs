using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Aether {
 public sealed class RushMap {
  public string name,subtitle,description,hazards,prize,art; public int meters,seconds,gold,opponents,credits; public Color color;
  public static readonly RushMap[] All={
   new RushMap{name="Sunset Coast",subtitle="Epic Drag Race",description="A six-kilometre sprint beside a sparkling ocean. Leap ramps, dodge oncoming cars and collect offensive power-ups.",hazards="Moving traffic · launch ramps · oil traps",prize="sun-turbo",art="Sunset",meters=6000,seconds=180,gold=110,opponents=7,credits=900,color=new Color(1,.49f,.22f)},
   new RushMap{name="Redrock Run",subtitle="Canyon Gauntlet",description="A longer straight through towering sandstone. Falling boulders and staggered roadblocks keep every lane changing.",hazards="Falling rocks · barricades · jump lanes",prize="rock-plate",art="Canyon",meters=7500,seconds=210,gold=140,opponents=9,credits=1200,color=new Color(1,.7f,.3f)},
   new RushMap{name="Stormbreak",subtitle="Thunder Sprint",description="Nine kilometres over a stormy sea. Read the warning lights, avoid electric lanes and race through bridge towers.",hazards="Electric lanes · slick road · oncoming cars",prize="storm-coil",art="Storm",meters=9000,seconds=245,gold=165,opponents=11,credits=1500,color=new Color(.38f,.77f,1)}
  };
 }
 public sealed class InventoryDef {
  public string id,name,category,rarity,description,slot; public int icon; public Color color;
  public static readonly InventoryDef[] All={
   new InventoryDef{id="sun-turbo",name="Solar Turbo",category="Attachments",rarity="Rare",slot="Engine",icon=0,color=new Color(1,.54f,.23f),description="Adds 8% engine power and 18.4 kg. Earned on Sunset Coast. Fits every saved car."},
   new InventoryDef{id="rock-plate",name="Redrock Plating",category="Equipment",rarity="Epic",slot="Chassis",icon=1,color=new Color(.85f,.56f,1),description="Reduces unshielded weapon impulse by 25%. Earned on Redrock Run. Fits every saved car."},
   new InventoryDef{id="storm-coil",name="Storm Coil",category="Attachments",rarity="Epic",slot="Engine",icon=2,color=new Color(.38f,.79f,1),description="Boost drains 20% less energy. Shares the Engine slot with Solar Turbo. Earned on Stormbreak."},
   new InventoryDef{id="grip-kit",name="Road Grip Kit",category="Equipment",rarity="Common",slot="Chassis",icon=3,color=new Color(.4f,.88f,.78f),description="Adds tire grip and 5.6 kg. Shares the Chassis slot with Redrock Plating. Included in your starter inventory."},
   new InventoryDef{id="rocket-pack",name="Rocket Pack",category="Items",rarity="Uncommon",slot="Starting item",icon=4,color=new Color(1,.58f,.38f),description="Prepare one Rocket for your next Rush. One pack is consumed at Go, including on race restarts."},
   new InventoryDef{id="shield-pack",name="Shield Pack",category="Items",rarity="Uncommon",slot="Starting item",icon=1,color=new Color(.4f,.85f,1),description="Prepare one Shield for your next Rush. Press E or tap Item to activate it. Consumed at Go."},
   new InventoryDef{id="upgrade-part",name="Upgrade Parts",category="Upgrades",rarity="Common",slot="Upgrade",icon=5,color=new Color(.85f,.9f,.94f),description="Spend 3 parts for one engine, tire, brake or suspension level on the selected garage car. Maximum 20 levels."}
  };
  public static InventoryDef Find(string id)=>All.FirstOrDefault(d=>d.id==id);
 }
 [Serializable] public class InventoryStack {public string id;public int count;}
 [Serializable] public class RushRecord {public int finishes,bestMedal;public float bestTime;}
 [Serializable] public class RushSave {
  public int version=1;public List<InventoryStack> items=new List<InventoryStack>();public string engine="",chassis="grip-kit",prepared="";public RushRecord[] records={new RushRecord(),new RushRecord(),new RushRecord()};
  public int Count(string id)=>items.FirstOrDefault(x=>x.id==id)?.count??0;
  public void Add(string id,int amount){if(InventoryDef.Find(id)==null||amount<=0)return;var stack=items.FirstOrDefault(x=>x.id==id);if(stack==null){stack=new InventoryStack{id=id};items.Add(stack);}stack.count=Mathf.Min(99999,stack.count+amount);}
  public bool Spend(string id,int amount){var stack=items.FirstOrDefault(x=>x.id==id);if(amount<=0||stack==null||stack.count<amount)return false;stack.count-=amount;return true;}
  public static RushSave Fresh(){var s=new RushSave();s.Add("grip-kit",1);s.Add("rocket-pack",2);s.Add("shield-pack",1);return s;}
  public void Validate(){if(items==null)items=new List<InventoryStack>();items=items.Where(x=>x!=null&&InventoryDef.Find(x.id)!=null).GroupBy(x=>x.id).Select(g=>new InventoryStack{id=g.Key,count=(int)Math.Min(99999,g.Sum(x=>(long)Mathf.Clamp(x.count,0,99999)))}).ToList();if(records==null||records.Length!=3)records=new[]{new RushRecord(),new RushRecord(),new RushRecord()};for(int i=0;i<3;i++){if(records[i]==null)records[i]=new RushRecord();records[i].bestMedal=Mathf.Clamp(records[i].bestMedal,0,3);if(float.IsNaN(records[i].bestTime)||float.IsInfinity(records[i].bestTime)||records[i].bestTime<0)records[i].bestTime=0;}if(Count(engine)==0||InventoryDef.Find(engine)?.slot!="Engine")engine="";if(Count(chassis)==0||InventoryDef.Find(chassis)?.slot!="Chassis")chassis="";if(Count(prepared)==0||InventoryDef.Find(prepared)?.category!="Items")prepared="";}
 }
 public partial class Game {
  public RushSave inventory;public bool rushMode;public int rushIndex;public bool rushFailed;public string rushReward="";public int rushMedal,rushAward;bool rushRewardGranted;
  public RushMap Challenge=>RushMap.All[Mathf.Clamp(rushIndex,0,2)];
  public void LoadInventory(){try{inventory=PlayerPrefs.HasKey("rush-inventory-v1")?JsonUtility.FromJson<RushSave>(PlayerPrefs.GetString("rush-inventory-v1")):RushSave.Fresh();}catch(Exception){inventory=RushSave.Fresh();}if(inventory==null)inventory=RushSave.Fresh();inventory.Validate();}
  public void SaveInventory(){if(testing)return;PlayerPrefs.SetString("rush-inventory-v1",JsonUtility.ToJson(inventory));PlayerPrefs.Save();}
  public bool EquipInventory(string id){var d=InventoryDef.Find(id);if(d==null||inventory.Count(id)<1)return false;if(d.slot=="Engine")inventory.engine=inventory.engine==id?"":id;else if(d.slot=="Chassis")inventory.chassis=inventory.chassis==id?"":id;else if(d.category=="Items")inventory.prepared=inventory.prepared==id?"":id;else return false;if(state==State.Menu&&showroom)ShowCar();SaveInventory();return true;}
  public bool UpgradeWithParts(int category){var c=CurrentCar;int level=category==0?c.engineLevel:category==1?c.tireLevel:category==2?c.brakeLevel:c.suspensionLevel;if(category<0||category>3||level>=20||!inventory.Spend("upgrade-part",3))return false;if(category==0)c.engineLevel++;else if(category==1)c.tireLevel++;else if(category==2)c.brakeLevel++;else c.suspensionLevel++;SyncCar();Save();SaveInventory();return true;}
  public void StartRush(int index){rushIndex=Mathf.Clamp(index,0,2);rushMode=true;stuntMode=false;StartRace();}
  void RushReset(){rushFailed=false;rushRewardGranted=false;rushReward="";rushMedal=rushAward=0;}
  void RushGo(){if(!rushMode)return;string id=inventory.prepared;if(inventory.Spend(id,1)){player.item=id=="rocket-pack"?"Rocket":"Shield";if(inventory.Count(id)==0)inventory.prepared="";SaveInventory();}}
  void RushUpdate(){if(!rushMode||state!=State.Race||!player||player.finished)return;if(raceTime>=Challenge.seconds){rushFailed=true;BeginFailedResults();}}
  public void AwardRush(Vehicle v){if(!rushMode||v!=player||!v.finished||v.finishTime>Challenge.seconds||rushRewardGranted||rushFailed)return;rushRewardGranted=true;var d=Challenge;var record=inventory.records[rushIndex];rushMedal=v.finishTime<=d.gold&&v.place<=3?3:v.finishTime<=d.gold+30?2:1;int parts=2+rushMedal;bool first=record.finishes==0;record.finishes++;record.bestMedal=Mathf.Max(record.bestMedal,rushMedal);if(record.bestTime==0||v.finishTime<record.bestTime)record.bestTime=v.finishTime;
   inventory.Add("upgrade-part",parts);inventory.Add("rocket-pack",1);rushReward=parts+" Upgrade Parts  +  1 Rocket Pack";
   if(first||inventory.Count(d.prize)==0){inventory.Add(d.prize,1);rushReward=InventoryDef.Find(d.prize).name+"  +  "+rushReward;}else{inventory.Add("upgrade-part",2);rushReward+="  +  2 duplicate parts";}
   if(UnityEngine.Random.value<.35f){inventory.Add("shield-pack",1);rushReward+="  +  1 Shield Pack";}
   rushAward=d.credits+(rushMedal-1)*200+v.coins*10;credits+=rushAward;Save();SaveInventory();Toast("Challenge complete · rewards saved");
  }
 }
}
