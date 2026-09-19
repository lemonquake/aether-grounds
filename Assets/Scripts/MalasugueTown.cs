using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aether {
 [Serializable] public class NeighborhoodRoad {public string name;public Vector3[] points;public float width;public bool asphalt;}
 [Serializable] public class NeighborhoodBuilding {public string id,name;public Vector3[] points;public float height;public bool surveyHeight;}
 [Serializable] public class NeighborhoodLandmark {public string name;public Vector3 position;}
 [Serializable] public class NeighborhoodData {public double originLatitude,originLongitude;public NeighborhoodRoad[] roads;public NeighborhoodBuilding[] buildings;public NeighborhoodLandmark[] landmarks;public Vector3[] route;public Vector3 residence,destination;}
 public partial class TrackWorld {
  public NeighborhoodData neighborhood;
  public bool NeighborhoodStreet(Vector3 p){
   if(neighborhood==null||p.y<.5f||p.y>10)return false;
   foreach(var road in neighborhood.roads)for(int i=1;i<road.points.Length;i++)if(SegmentDistance(p,road.points[i-1],road.points[i])<road.width*.5f+1)return true;
   return false;
  }
  public static float SegmentDistance(Vector3 p,Vector3 a,Vector3 b){p.y=a.y=b.y=0;var d=b-a;return Vector3.Distance(p,a+d*Mathf.Clamp01(Vector3.Dot(p-a,d)/Mathf.Max(.01f,d.sqrMagnitude)));}
 }
 public sealed partial class PhilippineCity {
  NeighborhoodData neighborhood;readonly List<Bounds> lots=new List<Bounds>();
  public static void BuildMalasugue(TrackWorld t){new PhilippineCity{track=t}.GenerateMalasugue();}
  Material plaster=>Mat("Weathered plaster",new Color(.71f,.68f,.59f),6);
  Material streetConcrete=>Mat("Slab concrete road",new Color(.49f,.49f,.45f),7,.14f);
  void GenerateMalasugue(){
   neighborhood=JsonUtility.FromJson<NeighborhoodData>(Resources.Load<TextAsset>("Malasugue").text);track.neighborhood=neighborhood;
   root=track.transform;architecture=Node(root,"Mapped streets and buildings",Vector3.zero);props=Node(root,"Neighborhood breakable objects",Vector3.zero);
   Array.Copy(neighborhood.route,track.points,TrackWorld.Count);for(int i=0;i<TrackWorld.Count;i++)track.length+=Vector3.Distance(track.points[i],track.points[(i+1)%TrackWorld.Count]);
   Box(architecture,"Neighborhood ground",new Vector3(0,1.45f,0),new Vector3(1450,2.9f,1500),Mat("Town ground",new Color(.38f,.40f,.30f),6),true);
   foreach(var road in neighborhood.roads)TownRoad(road);
   // The racing surface follows the imported centerline. Shoulders are modestly widened for passing.
   TownRoad(new NeighborhoodRoad{name="Race route",points=Loop(track.points),width=8.4f});
   foreach(var building in neighborhood.buildings)FootprintBuilding(building);
   // OSM has incomplete coverage. Infill is explicitly reconstructed, never presented as surveyed.
   int infill=0;
   foreach(var road in neighborhood.roads){
    for(int i=1;i<road.points.Length;i++){
     var a=road.points[i-1];var b=road.points[i];var f=(b-a).normalized;var side=Vector3.Cross(Vector3.up,f);
     float len=Vector3.Distance(a,b);
     for(float d=7;d<len;d+=12.5f)for(int s=-1;s<=1;s+=2){
      var p=Vector3.Lerp(a,b,d/len)+side*s*(road.width*.5f+6.7f);p.y=3.1f;
      if(Mathf.Abs(p.x)>405||Mathf.Abs(p.z)>470||TrackWorld.FlatDistance(p,track.points[track.Nearest(p)])>145||NearStreet(p,4.4f)||NearLot(p,7)||Vector3.Distance(p,neighborhood.residence)<15)continue;
      ReconstructedHouse(p,Quaternion.LookRotation(side*s),infill++);if(infill>=520)break;
     }
    }
   }
   // User-provided residence position, kept separate from claims about the unknown facade.
   var home=Node(architecture,"Leodones-Olivar Residence",neighborhood.residence,Quaternion.Euler(0,26,0));
   Box(home,"Residence gate wall",new Vector3(0,1.0f,0),new Vector3(7.2f,2,.22f),plaster,true);
   Box(home,"Residence metal gate",new Vector3(0,1, -.14f),new Vector3(3.5f,1.9f,.12f),green);
   for(int j=-5;j<=5;j++)Box(home,"Gate bars",new Vector3(j*.29f,1,-.23f),new Vector3(.035f,1.85f,.06f),metal);
   Board(home,"Leodones-Olivar\nResidence",new Vector3(0,2.8f,-.15f),new Vector2(7.5f,1.35f),.20f);
   // Names come from mapped public POIs; their commercial status may have changed since mapping.
   foreach(var landmark in neighborhood.landmarks){
    if(TrackWorld.FlatDistance(landmark.position,track.points[track.Nearest(landmark.position)])>90)continue;
    var nearest=NearestStreetPoint(landmark.position);var direction=landmark.position-nearest;direction.y=0;if(direction.sqrMagnitude<1)continue;
    var sign=Node(architecture,landmark.name+" location",landmark.position,Quaternion.LookRotation(direction));
    Board(sign,WrapName(landmark.name),new Vector3(0,3.9f,0),new Vector2(7,1.4f),.14f);
   }
   for(int n=7;n<TrackWorld.Count;n+=9){
    var p=track.points[n];var f=track.Forward(n);var r=track.Right(n);int s=n%2==0?1:-1;
    var edge=p+r*s*5.1f;edge.y=3.16f;
    if(!NearLot(edge,1)){
     if(n%3==1)Crate(edge,Quaternion.LookRotation(f));
     else LooseProp("K_barrel",edge,1.05f,24,CityPropKind.Metal);
    }
    if(n%27==7&&!NearLot(p+r*s*7,1.4f))Stall(p+r*s*7,Quaternion.LookRotation(r*s),n);
    if(n%18==7&&!NearLot(p-r*s*6,1))UtilityPole(p-r*s*6,track.points[(n+18)%TrackWorld.Count]-track.Right((n+18)%TrackWorld.Count)*s*6);
   }
   for(int n=0;n<TrackWorld.Count;n+=7){int side=n%2==0?1:-1;StreetLamp(track.points[n]+track.Right(n)*side*5.7f,Quaternion.LookRotation(track.Right(n)*side),n);}
   var trafficSystem=root.gameObject.AddComponent<TownTrafficSystem>();trafficSystem.Initialize(track,MakeTownTricycle);
   for(int n=24;n<TrackWorld.Count;n+=18){
    var p=track.points[n]+track.Right(n)*(n%36==6?1.65f:-1.65f);p.y=3.3f;
    var trike=MakeTownTricycle(p,Quaternion.LookRotation(track.Forward(n)),n);var driver=trike.gameObject.AddComponent<TricycleTraffic>();driver.track=track;driver.point=n;driver.lane=n%36==6?1.65f:-1.65f;driver.travelSign=n%54==24?-1:1;trafficSystem.Register(driver,n);
   }
   track.MakeCityRaceFeatures();
   for(int n=18;n<TrackWorld.Count;n+=48)track.pickups.Add(track.points[n]+Vector3.up*1.25f);
   for(int x=0;x<8;x++)for(int z=0;z<2;z++)Box(architecture,"Finish checker",track.points[0]+track.Right(0)*(x-3.5f)+track.Forward(0)*z*.8f+Vector3.up*.045f,new Vector3(1,.02f,.8f),(x+z)%2==0?white:dark,false,Quaternion.LookRotation(track.Forward(0)).eulerAngles);
   var finish=Node(architecture,"Malasugue Town start",track.points[0],Quaternion.LookRotation(track.Forward(0)));
   for(int s=-1;s<=1;s+=2)Box(finish,"Start support",new Vector3(s*5,3.8f,0),new Vector3(.18f,7.6f,.18f),metal,true);
   Board(finish,"Malasugue Town",new Vector3(0,7.1f,0),new Vector2(10.2f,1.1f),.29f);
   Batch();root.gameObject.AddComponent<CityLife>().Initialize(track,props);
   Presentation.Lighting(0);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00105f;RenderSettings.fogColor=new Color(.68f,.76f,.78f);
   var sky=new Material(Shader.Find("Aether/Sky"));sky.SetColor("_Top",new Color(.20f,.46f,.69f));sky.SetColor("_Horizon",new Color(.83f,.87f,.85f));sky.SetFloat("_Night",0);RenderSettings.skybox=sky;PremiumLighting.Environment(track);Presentation.Probe(root,track.points[0]+Vector3.up*5);
   Debug.Log("MALASUGUE GENERATED route="+track.length+" mappedFootprints="+neighborhood.buildings.Length+" reconstructedInfill="+infill+" tricycles="+root.GetComponentsInChildren<TricycleTraffic>().Length);
  }
  static Vector3[] Loop(Vector3[] points){var result=new Vector3[points.Length+1];Array.Copy(points,result,points.Length);result[points.Length]=points[0];return result;}
  bool NearLot(Vector3 p,float margin){foreach(var lot in lots)if(p.x>lot.min.x-margin&&p.x<lot.max.x+margin&&p.z>lot.min.z-margin&&p.z<lot.max.z+margin)return true;return false;}
  bool NearStreet(Vector3 p,float margin){foreach(var road in neighborhood.roads)for(int i=1;i<road.points.Length;i++)if(TrackWorld.SegmentDistance(p,road.points[i-1],road.points[i])<road.width*.5f+margin)return true;return false;}
  Vector3 NearestStreetPoint(Vector3 p){float best=float.MaxValue;Vector3 result=p;foreach(var road in neighborhood.roads)for(int i=1;i<road.points.Length;i++){var a=road.points[i-1];var d=road.points[i]-a;var q=a+d*Mathf.Clamp01(Vector3.Dot(p-a,d)/Mathf.Max(.01f,d.sqrMagnitude));float v=(p-q).sqrMagnitude;if(v<best){best=v;result=q;}}return result;}
  void TownRoad(NeighborhoodRoad road){
   for(int i=1;i<road.points.Length;i++){
    var a=road.points[i-1];var b=road.points[i];if(Mathf.Abs(a.x)>700||Mathf.Abs(a.z)>710||Mathf.Abs(b.x)>700||Mathf.Abs(b.z)>710)continue;
    var d=b-a;var pos=(a+b)*.5f;pos.y=road.name=="Race route"?3.13f:3.035f;
    if(d.sqrMagnitude<.01f)continue;
    Box(architecture,road.name,pos,new Vector3(road.width,.12f,d.magnitude+.18f),road.asphalt?asphalt:streetConcrete,true,Quaternion.LookRotation(d).eulerAngles);
    if(road.name=="Race route")continue;
    var side=Vector3.Cross(Vector3.up,d.normalized);
    for(int s=-1;s<=1;s+=2){Box(architecture,"Open drainage",pos+side*s*(road.width*.5f+.25f)-Vector3.up*.025f,new Vector3(.45f,.05f,d.magnitude),dark,false,Quaternion.LookRotation(d).eulerAngles);}
   }
  }
  void FootprintBuilding(NeighborhoodBuilding building){
   var points=building.points;var center=Vector3.zero;foreach(var p in points)center+=p;center/=points.Length;
   var bound=new Bounds(points[0],Vector3.zero);foreach(var p in points)bound.Encapsulate(p);lots.Add(bound);
   int seed=int.Parse(building.id.Substring(Math.Max(0,building.id.Length-6)));float h=building.height;
   if(!building.surveyHeight)h=seed%5==0?6.4f:seed%3==0?4.2f:3.4f;
   var wall=Mat("Town plaster "+seed%8,new[]{new Color(.69f,.67f,.58f),new Color(.72f,.72f,.66f),new Color(.60f,.68f,.57f),new Color(.71f,.57f,.42f),new Color(.67f,.71f,.72f),new Color(.52f,.59f,.56f),new Color(.79f,.70f,.55f),new Color(.53f,.52f,.48f)}[seed%8],6);
   var roof=Mat("Town roof "+seed%5,new[]{new Color(.42f,.44f,.44f),new Color(.46f,.19f,.12f),new Color(.31f,.40f,.42f),new Color(.60f,.57f,.49f),new Color(.24f,.37f,.30f)}[seed%5],5,.4f);
   var node=Node(architecture,"OSM footprint "+building.id+" "+building.name,Vector3.zero);
   float area=0;for(int k=0;k<points.Length;k++)area+=points[k].x*points[(k+1)%points.Length].z-points[(k+1)%points.Length].x*points[k].z;
   var vs=new List<Vector3>();var ts=new List<int>();
   for(int i=0;i<points.Length;i++){
    var a=points[i];var b=points[(i+1)%points.Length];int n=vs.Count;
    vs.Add(a);vs.Add(b);vs.Add(a+Vector3.up*h);vs.Add(b+Vector3.up*h);ts.AddRange(area>0?new[]{n,n+2,n+1,n+1,n+2,n+3}:new[]{n,n+1,n+2,n+1,n+3,n+2});
    var edge=b-a;float len=edge.magnitude;var outward=Vector3.Cross(Vector3.up,edge.normalized);if(Vector3.Dot(outward,(a+b)*.5f-center)<0)outward=-outward;
    var q=Quaternion.LookRotation(-outward.normalized); // local -Z faces the street side
    if(len>3&&TrackWorld.FlatDistance((a+b)*.5f,track.points[track.Nearest((a+b)*.5f)])<85){
     var facade=Node(node,"Reconstructed facade",(a+b)*.5f,q);
     int windows=Mathf.Min(8,Mathf.FloorToInt(len/2.8f));
     for(int j=0;j<windows;j++)for(float level=2;level<h-.4f;level+=3){float x=(j-(windows-1)*.5f)*2.4f;Box(facade,"Window frame",new Vector3(x,level,-.08f),new Vector3(1.25f,1.15f,.09f),white);Box(facade,"Tinted window",new Vector3(x,level,-.14f),new Vector3(1.08f,.98f,.05f),Mat("Town window",new Color(.12f,.20f,.20f),0,.6f));for(int k=-1;k<=1;k++)Box(facade,"Window grille",new Vector3(x+k*.31f,level,-.19f),new Vector3(.035f,1.07f,.04f),metal);}
    }
   }
   TrackWorld.MeshObject("Mapped building walls",vs.ToArray(),ts.ToArray(),wall,true,node);
   // Ear-clipped flat roof preserves concave outlines. Corrugation comes from the material.
   var roofPoints=new Vector3[points.Length];for(int i=0;i<points.Length;i++)roofPoints[i]=points[i]+Vector3.up*(h+.10f);
   TrackWorld.MeshObject("Corrugated footprint roof",roofPoints,Triangulate(roofPoints),roof,false,node);
   if(TrackWorld.FlatDistance(center,track.points[track.Nearest(center)])<70){
    Round(node,"Roof water tank",center+new Vector3(0,h+.65f,0),new Vector3(1,.6f,1),Mat("Tank",new Color(.13f,.32f,.48f)));
    var nearest=NearestStreetPoint(center);var toward=(nearest-center).normalized;
    var yard=center+toward*Mathf.Min(bound.size.x,bound.size.z)*.55f;
    if(!NearStreet(yard,1))Asset("planter",node,yard,.75f);
   }
  }
  static int[] Triangulate(Vector3[] p){
   float Cross(Vector3 a,Vector3 b,Vector3 c)=>(b.x-a.x)*(c.z-a.z)-(b.z-a.z)*(c.x-a.x);
   float area=0;for(int i=0;i<p.Length;i++)area+=p[i].x*p[(i+1)%p.Length].z-p[(i+1)%p.Length].x*p[i].z;
   var ids=new List<int>();for(int i=0;i<p.Length;i++)ids.Add(area>0?i:p.Length-1-i);var result=new List<int>();int guard=p.Length*p.Length;
   while(ids.Count>2&&guard-->0){bool clipped=false;for(int i=0;i<ids.Count;i++){int a=ids[(i+ids.Count-1)%ids.Count],b=ids[i],c=ids[(i+1)%ids.Count];if(Cross(p[a],p[b],p[c])<=.0001f)continue;bool inside=false;foreach(int j in ids)if(j!=a&&j!=b&&j!=c&&Cross(p[a],p[b],p[j])>=0&&Cross(p[b],p[c],p[j])>=0&&Cross(p[c],p[a],p[j])>=0){inside=true;break;}if(inside)continue;result.AddRange(new[]{a,c,b});ids.RemoveAt(i);clipped=true;break;}if(!clipped)break;}
   return result.ToArray();
  }
  void ReconstructedHouse(Vector3 p,Quaternion q,int seed){
   var o=Node(architecture,"Reconstructed unmapped home",p,q);float h=seed%5==0?6:3.5f;float w=8,d=8;
   Box(o,"Plastered hollow block walls",new Vector3(0,h*.5f,0),new Vector3(w,h,d),Mat("Infill plaster "+seed%4,new[]{new Color(.68f,.67f,.59f),new Color(.56f,.64f,.57f),new Color(.74f,.66f,.51f),new Color(.58f,.61f,.61f)}[seed%4],6),true);
   var roof=Mat("Infill metal roof "+seed%3,seed%3==0?new Color(.48f,.23f,.16f):seed%3==1?new Color(.44f,.46f,.44f):new Color(.27f,.38f,.32f),5,.4f);
   for(int s=-1;s<=1;s+=2)Box(o,"Pitched corrugated roof",new Vector3(s*2.2f,h+.6f,0),new Vector3(4.7f,.12f,9),roof,false,new Vector3(0,0,s*-15));
   Box(o,"Wooden door",new Vector3(-1.7f,1.15f,-4.04f),new Vector3(1.3f,2.3f,.09f),wood);
   for(int j=0;j<2;j++){Box(o,"Window frame",new Vector3(j*2.1f+.2f,1.9f,-4.06f),new Vector3(1.5f,1.3f,.09f),white);Box(o,"Window glass",new Vector3(j*2.1f+.2f,1.9f,-4.12f),new Vector3(1.3f,1.1f,.05f),dark);for(int k=-2;k<=2;k++)Box(o,"Window security grille",new Vector3(j*2.1f+.2f+k*.25f,1.9f,-4.18f),new Vector3(.035f,1.2f,.06f),metal);}
   Box(o,"Front canopy",new Vector3(0,2.75f,-4.7f),new Vector3(8.6f,.12f,2.2f),roof,false,new Vector3(-8,0,0));
   if(seed%6==0){Board(o,"Sari-sari store",new Vector3(0,2.8f,-5.8f),new Vector2(3.8f,.65f),.12f);}
   if(seed%8==0)Asset("fence-low",o,new Vector3(3,0,-5.2f),.8f);
   if(seed%11==0)Asset("tree-small",o,new Vector3(-4.6f,0,0),4);
   lots.Add(new Bounds(p,new Vector3(10,10,10)));
  }
  void Board(Transform parent,string title,Vector3 p,Vector2 size,float font){Box(parent,"Sign board",p,new Vector3(size.x,size.y,.12f),green);Text(parent,title,p+Vector3.back*.10f,font,Color.white,new Vector3(0,180,0));parent.GetChild(parent.childCount-1).GetComponent<StructureSign>().fit=size*.88f;}
  string WrapName(string s){if(s.Length<28)return s;int n=s.LastIndexOf(' ',Mathf.Min(25,s.Length-1));return n>0?s.Substring(0,n)+"\n"+s.Substring(n+1):s;}

 }
}
