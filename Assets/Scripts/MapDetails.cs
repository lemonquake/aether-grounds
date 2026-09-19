using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class TrackWorld {
  public int detailPropCount;public readonly HashSet<string> detailTypes=new HashSet<string>();
  static readonly Dictionary<string,Bounds> detailBounds=new Dictionary<string,Bounds>();
  static readonly string[] flowers={"flower_purpleA","flower_redA","flower_yellowA"};
  static readonly string[] plants={"grass_large","grass_leafs","grass_leafsLarge","plant_bushDetailed","plant_bushLarge","plant_bushSmall"};
  static readonly string[] stones={"stone_largeA","stone_largeC","stone_smallA","stone_smallC","stone_tallA","stone_tallC"};
  Transform DetailSector(string name,Vector3 p=default,Quaternion q=default){var t=new GameObject(name).transform;t.SetParent(transform);t.position=p;t.rotation=q==default?Quaternion.identity:q;return t;}
  GameObject Detail(Transform root,string pack,string name,Vector3 p,float size,float yaw=0){
   string key="D_"+pack+"_"+name;var o=ModelLibrary.Create(key,root);
   if(!detailBounds.TryGetValue(key,out var b)){bool first=true;b=new Bounds();foreach(var mf in o.GetComponentsInChildren<MeshFilter>()){if(first){b=mf.sharedMesh.bounds;first=false;}else b.Encapsulate(mf.sharedMesh.bounds);}detailBounds[key]=b;}
   float scale=size/Mathf.Max(.001f,Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z)));var rot=Quaternion.Euler(0,yaw,0);o.transform.localScale=Vector3.one*scale;o.transform.localRotation=rot;o.transform.localPosition=p-rot*new Vector3(b.center.x,b.min.y,b.center.z)*scale;
   detailPropCount++;detailTypes.Add(key);return o;
  }
  Material DetailMetal=>ModelLibrary.Material("Detail structural steel",new Color(.17f,.24f,.28f));
  Material DetailConcrete=>ModelLibrary.Material("Detail weathered concrete",new Color(.43f,.46f,.45f));
  void DetailBox(Transform root,string name,Vector3 p,Vector3 size,Material mat)=>CarParts.Part(root,name,PrimitiveType.Cube,p,size,mat);
  void DetailBeam(Transform root,string name,Vector3 a,Vector3 b,float width,Material mat){var o=CarParts.Part(root,name,PrimitiveType.Cube,(a+b)*.5f,new Vector3(width,width,Vector3.Distance(a,b)),mat);o.transform.localRotation=Quaternion.LookRotation(b-a);}
  public void BuildMapDetails(){
   if(IsAirah)return;
   if(IsAirah)return;
   Physics.SyncTransforms();rng=new System.Random(isStunts?7721:isRush?7330+rushTheme:7920+map);
   if(isStunts)StuntDetails();else if(isRush)RushDetails();else CircuitDetails();
   Debug.Log("MAP DETAILS "+(isStunts?"Heartstopper":isRush?"Rush "+rushTheme:Names[map])+" props="+detailPropCount+" types="+detailTypes.Count);
  }
  void RaceFacilities(Transform t,bool full){
   // Two rows of identifiable facilities, facing the course, with a fenced pedestrian apron.
   DetailBox(t,"Pit apron",new Vector3(0,-.15f,0),new Vector3(23,.3f,100),DetailConcrete);
   string[] buildings={"pitsGarage","pitsOffice","grandStandCovered","grandStandAwning","grandStand"};
   for(int i=0;i<5;i++)Detail(t,"racing",buildings[i],new Vector3(5,0,-39+i*19),15,-90);
   for(int i=0;i<12;i++){Detail(t,"racing",i%2==0?"barrierRed":"barrierWhite",new Vector3(-11,0,-46+i*8),5,90);if(i%3==0)Detail(t,"racing","fenceStraight",new Vector3(11,0,-44+i*8),7,90);}
   Detail(t,"racing","bannerTowerGreen",new Vector3(-7,0,-44),9);Detail(t,"racing","bannerTowerRed",new Vector3(-7,0,44),9);
   Detail(t,"racing","flagCheckers",new Vector3(-8,0,0),7);Detail(t,"racing","flagGreen",new Vector3(-8,0,-25),6);Detail(t,"racing","flagRed",new Vector3(-8,0,25),6);
   Detail(t,"racing","billboard",new Vector3(6,0,51),12,180);Detail(t,"racing","billboardLow",new Vector3(-3,0,-54),7);
   Detail(t,"racing","lightPostLarge",new Vector3(10,0,0),12);Detail(t,"racing","lightPostModern",new Vector3(-8,0,40),8);
   for(int i=0;i<6;i++)Detail(t,"racing","pylon",new Vector3(-6,0,-8+i*3),.9f);
   Detail(t,"racing","radarEquipment",new Vector3(8,4,-20),3);Detail(t,"racing","railDouble",new Vector3(-9,0,50),8);
   if(full){Detail(t,"racing","tent",new Vector3(23,0,-30),9);Detail(t,"racing","tentClosed",new Vector3(23,0,-15),9);Detail(t,"racing","tentRoofDouble",new Vector3(23,0,4),13);Detail(t,"racing","treeLarge",new Vector3(24,0,25),10);Detail(t,"racing","treeSmall",new Vector3(24,0,40),7);}
  }
  void IndustrialYard(Transform t){
   DetailBox(t,"Service yard paving",new Vector3(0,-.25f,0),new Vector3(88,.5f,100),DetailConcrete);
   for(int i=0;i<3;i++){Detail(t,"industrial","building-"+new[]{"a","c","f"}[i],new Vector3(23,0,-30+i*30),24,90);Detail(t,"industrial","shipping-container-"+new[]{"a","b","c"}[i],new Vector3(-10,0,-34+i*13),11);}
   Detail(t,"industrial","shipping-container-b",new Vector3(-10,3.9f,-34),11);
   Detail(t,"industrial","chimney-large",new Vector3(36,0,44),29);Detail(t,"industrial","detail-tank-large",new Vector3(-26,0,28),10);Detail(t,"industrial","detail-tank",new Vector3(-26,0,12),7);
   Detail(t,"industrial","water-tower",new Vector3(-32,0,-30),18);
   Detail(t,"industrial","solar-panel-landscape-group",new Vector3(0,0,32),13);Detail(t,"industrial","solar-panel-portrait-group",new Vector3(0,0,46),12);
   Detail(t,"industrial","windmill",new Vector3(-30,0,50),38);
   for(int j=0;j<12;j++)Detail(t,"racing","fenceStraight",new Vector3(-44,0,-44+j*8),8,90);
  }
  void PlantPatch(Transform t,Vector3 p,int seed,bool dry=false){
   Detail(t,"nature",stones[seed%stones.Length],p,1.2f+seed%4,seed*73);
   for(int j=0;j<5;j++){Vector3 a=p+new Vector3(Mathf.Sin(seed*4+j*2.3f)*(3+j),0,Mathf.Cos(seed*3+j*2.3f)*(3+j));
    string name=dry?(j%2==0?"cactus_short":"cactus_tall"):plants[(seed+j)%plants.Length];Detail(t,"nature",name,a,dry?1.6f+j*.35f:.7f+j*.5f,seed*51+j*37);
   }
   if(!dry){Detail(t,"nature",flowers[seed%3],p+new Vector3(1,0,2),1.1f,seed*17);Detail(t,"nature",seed%2==0?"mushroom_redGroup":"mushroom_tanGroup",p+new Vector3(-1,0,-2),.8f);if(seed%4==0)Detail(t,"nature","log_stack",p+Vector3.right*4,3.5f,31);}
  }
  void CircuitDetails(){
   // Read the actual ground and avoid both the driven road and existing architecture.
   for(int n=0;n<Count;n+=8){var t=DetailSector("Circuit detail "+n);for(int s=-1;s<=1;s+=2){Vector3 p=points[n]+Right(n)*s*(map==2?28:19);float baseY=map==2?Height(p.x,p.z):0;
     if(map!=2){if(!Physics.Raycast(new Vector3(p.x,points[n].y+35,p.z),Vector3.down,out var hit,80,1,QueryTriggerInteraction.Ignore)||hit.point.y>points[n].y+2)continue;baseY=hit.point.y;}
     p.y=baseY;int k=n/8;
     if(map==2){PlantPatch(t,p,k);if(k%3==0)Detail(t,"racing","treeLarge",p+Right(n)*s*10,9+k%5,k*37);}
     else {Detail(t,"racing",k%3==0?"lightPostModern":k%3==1?"flagCheckers":"billboardLow",p,k%3==2?4:6,Quaternion.LookRotation(Forward(n)).eulerAngles.y+s*90);if(k%2==0)Detail(t,"nature","plant_bushDetailed",p+Forward(n)*5,2.2f,k*19);}
    }CombineRushSector(t);}
   if(map==2){var t=DetailSector("Gardens race village",points[8]+Right(8)*65,Quaternion.LookRotation(Forward(8)));RaceFacilities(t,true);CombineRushSector(t);}
   // Vacant roadside sites only; yard footprints must clear the circuit and existing buildings.
   if(map!=2){for(int n=12;n<Count;n+=36){Vector3 p=points[n]+Right(n)*90;p.y=0;if(FlatDistance(p,points[Nearest(p)])<65)continue;if(Physics.CheckBox(p+Vector3.up*8,new Vector3(43,6,49),Quaternion.identity,1,QueryTriggerInteraction.Ignore))continue;var t=DetailSector("City service facilities",p);IndustrialYard(t);CombineRushSector(t);break;}}
  }
  void RushDetails(){
   for(int i=0;i<Mathf.CeilToInt(length/140);i++){float z=i*140+50;var t=DetailSector("Rush roadside details "+i);
    if(rushTheme<2){for(int j=0;j<5;j++)PlantPatch(t,new Vector3(-22-j%2*11,3.2f,z+j*23),i*5+j,rushTheme==1);
     if(i%6==0){var facility=DetailSector("Rush service stop "+i,new Vector3(-75,3.2f,z));if(i%12==0)RaceFacilities(facility,true);else IndustrialYard(facility);CombineRushSector(facility);}
    }else {if(i%3==0){var platform=DetailSector("Bridge maintenance platform "+i,new Vector3(-58,4.7f,z));DetailBox(platform,"Maintenance deck",new Vector3(0,-2,0),new Vector3(78,4,110),DetailMetal);IndustrialYard(platform);CombineRushSector(platform);}
     Detail(t,"racing","lightPostLarge",new Vector3(14,5,z),13);Detail(t,"racing","radarEquipment",new Vector3(-15,7,z+25),3);Detail(t,"racing","flagRed",new Vector3(14,5,z+50),7);
    }CombineRushSector(t);
   }
  }
  void StuntCliff(Transform root,Vector3 center,float rx,float rz,float top,int seed){
   // Terraced flat-topped islands: broad rock shelves, striated cliffs and a planted summit.
   const int sides=48;float[] radii={1.25f,1.16f,1.05f,1.09f,.96f,1,.88f,.84f};float[] heights={-15,.08f*top,.3f*top,.32f*top,.57f*top,.59f*top,.91f*top,top};
   for(int ring=0;ring<radii.Length-1;ring++){var vs=new List<Vector3>();var triangles=new List<int>();for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides;float rough=1+.06f*Mathf.Sin(a*7+seed)+.045f*Mathf.Cos(a*13+seed);for(int k=0;k<2;k++)vs.Add(center+new Vector3(Mathf.Cos(a)*rx*radii[ring+k]*rough,heights[ring+k],Mathf.Sin(a)*rz*radii[ring+k]*rough));if(j<sides){int v=j*2;triangles.AddRange(new[]{v,v+1,v+2,v+2,v+1,v+3});}}
    var mat=ModelLibrary.Material("Stunt cliff layer "+ring,Color.Lerp(new Color(.23f,.29f,.3f),new Color(.51f,.49f,.38f),ring/7f));mat.SetFloat("_Surface",2);mat.SetFloat("_Smoothness",.08f);MeshObject("Terraced rock stratum",vs.ToArray(),triangles.ToArray(),mat,false,root);}
   var topVs=new List<Vector3>{center+Vector3.up*top};var topTs=new List<int>();for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides;float rough=1+.06f*Mathf.Sin(a*7+seed)+.045f*Mathf.Cos(a*13+seed);topVs.Add(center+new Vector3(Mathf.Cos(a)*rx*.84f*rough,top,Mathf.Sin(a)*rz*.84f*rough));if(j<sides)topTs.AddRange(new[]{0,j+2,j+1});}var turf=ModelLibrary.Material("Stunt island grass",new Color(.25f,.36f,.2f));turf.SetFloat("_Surface",3);MeshObject("Island summit",topVs.ToArray(),topTs.ToArray(),turf,false,root);
  }
  void StuntDetails(){
   float[] centers={20,2250,4250};float[] widths={155,210,210};float[] depths={100,260,350};
   for(int island=0;island<3;island++){float z=centers[island],y=StuntHeight(z)-.7f;var t=DetailSector("Heartstopper island scenery "+island);StuntCliff(t,new Vector3(0,0,z),widths[island],depths[island],y,island+31);
    for(int i=0;i<42;i++){float angle=i*2.39996f,r=.6f+Mathf.Sin(i*9)*.13f;Vector3 p=new Vector3(Mathf.Cos(angle)*widths[island]*r,y,z+Mathf.Sin(angle)*depths[island]*r);if(Mathf.Abs(p.x-StuntX(p.z))<StuntWidth(p.z)*.5f+14)continue;PlantPatch(t,p,i);if(i%3==0)Detail(t,"racing",i%2==0?"treeLarge":"treeSmall",p+Vector3.right*5,8+i%7,i*47);}
    CombineRushSector(t);
    var village=DetailSector("Heartstopper spectator village "+island,new Vector3(island==0?-60:115,y,z));RaceFacilities(village,true);CombineRushSector(village);
    if(island==1){var yard=DetailSector("Heartstopper utilities",new Vector3(-125,y,z+40));IndustrialYard(yard);CombineRushSector(yard);}
   }
   var paint=ModelLibrary.Material("Stunt runway paint",new Color(.68f,.77f,.75f));var shoulder=ModelLibrary.Material("Stunt landing amber",new Color(.87f,.49f,.12f));
   foreach(float start in new[]{2070f,3950f}){var deck=DetailSector("Landing runway markings "+start);for(float z=start;z<start+280;z+=18){float x=StuntX(z),y=StuntHeight(z)+.028f;for(int s=-1;s<=1;s+=2){DetailBox(deck,"Landing guidance dash",new Vector3(x+s*12,y,z),new Vector3(.3f,.025f,9),paint);DetailBox(deck,"Landing shoulder block",new Vector3(x+s*(StuntWidth(z)*.5f-6),y,z),new Vector3(7,.025f,3),shoulder);}if(z<start+55)for(int k=-3;k<=3;k++)DetailBox(deck,"Touchdown marker",new Vector3(x+k*5,y,z),new Vector3(2,.025f,7),paint);}CombineRushSector(deck);}
   var accent=ModelLibrary.Material("Stunt amber safety",new Color(1,.55f,.10f),.3f);var cyan=ModelLibrary.Material("Stunt structural lights",new Color(.1f,.72f,.83f),.4f);
   for(float z=120;z<4730;z+=70){if(StuntGap(z)||z>2050&&z<2450||z>3930&&z<4350)continue;Vector3 p=StuntPosition(z);var q=Quaternion.LookRotation(StuntPosition(z+2)-StuntPosition(z-2));var t=DetailSector("Heartstopper bridge detail "+z,p,q);float w=StuntWidth(z)*.5f+1.2f;
    // Repeated under-road box trusses and diagonal bracing make the elevated roadway legible.
    for(int s=-1;s<=1;s+=2){DetailBeam(t,"Longitudinal beam",new Vector3(s*w,-2,-34),new Vector3(s*w,-2,34),1.1f,DetailMetal);DetailBeam(t,"Lower truss chord",new Vector3(s*w,-9,-34),new Vector3(s*w,-9,34),.7f,DetailMetal);
     for(int j=0;j<4;j++){float zz=-34+j*17;DetailBeam(t,"Cross bracing",new Vector3(s*w,-9,zz),new Vector3(s*w,-2,zz+17),.45f,DetailMetal);DetailBeam(t,"Truss upright",new Vector3(s*w,-9,zz),new Vector3(s*w,-2,zz),.45f,DetailMetal);DetailBox(t,"Reflective road marker",new Vector3(s*(w-.8f),.9f,zz),new Vector3(.15f,.6f,.35f),accent);}
     DetailBeam(t,"Low guide light",new Vector3(s*w,-1.2f,-34),new Vector3(s*w,-1.2f,34),.12f,cyan);
    }DetailBeam(t,"Transverse under-deck girder",new Vector3(-w,-3,0),new Vector3(w,-3,0),1.4f,DetailMetal);
    if((int)z%210==120){for(int s=-1;s<=1;s+=2){DetailBox(t,"Observation balcony",new Vector3(s*(w+6),-1,0),new Vector3(11,1.4f,19),DetailMetal);Detail(t,"racing","radarEquipment",new Vector3(s*(w+7),0,0),3);Detail(t,"racing","flagCheckers",new Vector3(s*(w+10),0,6),7);Detail(t,"racing","lightPostModern",new Vector3(s*(w+10),0,-6),9);}}
    CombineRushSector(t);
   }
   // Depth cues beside the flight paths; leave both jump corridors completely open.
   for(int i=0;i<16;i++){float z=850+i*245;var t=DetailSector("Distant coastal island "+i);float x=(i%2==0?-1:1)*(310+i%3*115);RushIsland(t,new Vector3(x,-10,z),100+i%4*20,150,75+i%5*28,i+140,false);Detail(t,"industrial","windmill",new Vector3(x,65+i%5*28,z),60);CombineRushSector(t);}
  }
 }
}
