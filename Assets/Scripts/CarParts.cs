using System;
using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 [Serializable] public class SavedCar {
  public string supportEdition="";public bool neonWheels,starTrail,glassWheels,glassChassis;public string name="My Vanta"; public int paintStyle; public int body,paint=1,accent=3,wheel,wheelColor=4,glass,engine,spoiler,exhaust,livery,engineLevel,tireLevel,brakeLevel;
  public void Validate(){paintStyle=Mathf.Clamp(paintStyle,0,PaintStyles.Names.Length-1);body=Mathf.Clamp(body,0,9);paint=Mathf.Clamp(paint,0,31);accent=Mathf.Clamp(accent,0,31);wheelColor=Mathf.Clamp(wheelColor,0,31);wheel=Mathf.Clamp(wheel,0,9);glass=Mathf.Clamp(glass,0,12);engine=Mathf.Clamp(engine,0,10);spoiler=Mathf.Clamp(spoiler,0,10);exhaust=Mathf.Clamp(exhaust,0,10);livery=Mathf.Clamp(livery,0,10);engineLevel=Mathf.Clamp(engineLevel,0,20);tireLevel=Mathf.Clamp(tireLevel,0,20);brakeLevel=Mathf.Clamp(brakeLevel,0,20);}
 }
 [Serializable] public class GarageSave {public int version=2,selected;public List<SavedCar> cars=new List<SavedCar>();}
 public static class CarParts {
  public static readonly string[] Wheels={"Five spoke","Split spoke","Turbofan","Wire mesh","Rally disc","Beadlock","Six spoke","Deep dish","Aero blade","Honeycomb"};
  public static readonly string[] Engines={"Standard","Inline four","V6 twin intake","V8 stacks","Flat six","Rotary","Turbine","Electric drive","Twin turbo","Supercharged V8","V12"};
  public static readonly string[] Spoilers={"Standard","Ducktail","Club wing","Double wing","Tall GT","Low GT","Split wing","Rally wing","Endplate wing","Center fin","Wide blade"};
  public static readonly string[] Exhausts={"Standard","Single round","Twin round","Quad tips","Hex tips","Wide oval","Side pipes","High stacks","Twin center","Triple center","Titanium tips"};
  public static readonly string[] Liveries={"None","Center stripe","Twin stripes","Offset stripe","Three stripes","Hood bands","Side stripe","Side blocks","Hood chevrons","Two tone hood","Number blocks"};
  public static readonly string[] GlassNames={"Smoke","Cyan","Violet","Clear blue","Amber","Emerald","Rose","Silver","Midnight","Ocean","Plum","Bronze","Ice"};
  public static readonly Color[] Glass={new Color(.04f,.09f,.12f),new Color(.1f,.38f,.48f),new Color(.3f,.18f,.4f),new Color(.35f,.49f,.57f),new Color(.45f,.29f,.09f),new Color(.07f,.28f,.21f),new Color(.4f,.16f,.24f),new Color(.37f,.41f,.45f),new Color(.025f,.035f,.06f),new Color(.04f,.21f,.36f),new Color(.18f,.065f,.25f),new Color(.3f,.19f,.12f),new Color(.5f,.7f,.73f)};
  public static GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material material,Vector3 rotation=default){var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=scale;o.transform.localRotation=Quaternion.Euler(rotation);var c=o.GetComponent<Collider>();c.enabled=false;UnityEngine.Object.Destroy(c);o.GetComponent<Renderer>().sharedMaterial=material;return o;}
  static void Box(Transform p,string n,Vector3 v,Vector3 s,Material m,Vector3 r=default)=>Part(p,n,PrimitiveType.Cube,v,s,m,r);
  static void Cylinder(Transform p,string n,Vector3 v,Vector3 s,Material m,Vector3 r=default)=>Part(p,n,PrimitiveType.Cylinder,v,s,m,r);
  static GameObject RingMesh(Transform parent,string name,float inner,float outer,float width,Material mat,int sides=40){
   var vertices=new Vector3[sides*4];var triangles=new List<int>();
   for(int ring=0;ring<4;ring++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;float radius=ring==0||ring==3?inner:outer;vertices[ring*sides+i]=new Vector3(ring<2?-width*.5f:width*.5f,Mathf.Cos(a)*radius,Mathf.Sin(a)*radius);}
   for(int ring=0;ring<4;ring++)for(int i=0;i<sides;i++){int a=ring*sides+i,b=ring*sides+(i+1)%sides,c=((ring+1)%4)*sides+i,d=((ring+1)%4)*sides+(i+1)%sides;triangles.AddRange(new[]{a,b,c,b,d,c});}
   var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;o.AddComponent<OwnedMesh>();return o;
  }
  public static void Combine(Transform root){var groups=new Dictionary<Material,List<CombineInstance>>();var old=new List<GameObject>();foreach(var mf in root.GetComponentsInChildren<MeshFilter>()){var m=mf.GetComponent<Renderer>().sharedMaterial;if(!groups.ContainsKey(m))groups[m]=new List<CombineInstance>();groups[m].Add(new CombineInstance{mesh=mf.sharedMesh,transform=root.worldToLocalMatrix*mf.transform.localToWorldMatrix});old.Add(mf.gameObject);mf.GetComponent<Renderer>().enabled=false;}foreach(var g in groups){var m=new Mesh(){indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.CombineMeshes(g.Value.ToArray());var o=new GameObject(g.Key.name);o.transform.SetParent(root,false);o.AddComponent<MeshFilter>().sharedMesh=m;o.AddComponent<MeshRenderer>().sharedMaterial=g.Key;o.AddComponent<OwnedMesh>();}foreach(var o in old)UnityEngine.Object.Destroy(o);}
  public static void Apply(GameObject model,SavedCar c){
   c.Validate();foreach(string part in new[]{"StockWing","StockExhaust","StockScoop"}){var old=model.transform.Find(part);if(old)old.gameObject.SetActive(part=="StockWing"?c.spoiler==0:part=="StockExhaust"?c.exhaust==0:c.engine==0);}
   ModelLibrary.Customize(model,Game.Paints[c.paint],Game.Paints[c.accent],c.wheelColor,c.glass);
   foreach(string name in new[]{"WheelFL","WheelFR","WheelRL","WheelRR"}){var hub=model.transform.Find(name);if(!hub)continue;foreach(Transform child in hub){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}BuildWheel(hub,c.wheel,Game.Paints[c.wheelColor],hub.localPosition.y);}
   var root=new GameObject("Selected accessories").transform;root.SetParent(model.transform,false);
   if(c.engine>0)BuildEngine(root,c.engine,c.body,Game.Paints[c.accent]);
   var accent=ModelLibrary.Material("Accessory paint "+c.accent,Game.Paints[c.accent]);var metal=ModelLibrary.Material("Metal",new Color(.5f,.56f,.62f));var dark=ModelLibrary.Material("Carbon",new Color(.025f,.033f,.044f));
   if(c.spoiler>0){float y=new[]{0f,.96f,1.3f,1.43f,1.85f,1.16f,1.56f,1.67f,1.48f,1.4f,1.22f}[c.spoiler]+(c.body==8?.85f:c.body==5?.25f:0),z=c.body==6?-1.35f:-1.85f;float width=1.6f+(c.spoiler%3)*.24f;for(int s=-1;s<=1;s+=2)Box(root,"Wing supports",new Vector3(s*.62f,y-.2f,z),new Vector3(.06f,.52f,.15f),metal);if(c.spoiler==6){for(int side=-1;side<=1;side+=2)Box(root,"Split wing",new Vector3(side*width*.28f,y+.1f,z),new Vector3(width*.44f,.08f,.4f),accent);}else Box(root,"Selected wing",new Vector3(0,y+.1f,z),new Vector3(width,.08f,c.spoiler==10?.65f:.4f),accent,new Vector3(c.spoiler==1?22:c.spoiler*2,0,0));if(c.spoiler==3)Box(root,"Second wing",new Vector3(0,y+.34f,z),new Vector3(width,.07f,.3f),dark);if(c.spoiler>=4)for(int s=-1;s<=1;s+=2)Box(root,"Wing endplates",new Vector3(s*width/2,y+.14f,z),new Vector3(.045f,.27f,.46f),accent);if(c.spoiler==9)Box(root,"Center fin",new Vector3(0,y+.26f,z+.35f),new Vector3(.05f,.43f,.8f),accent);}
   if(c.exhaust>0){
    int count=c.exhaust==3?4:c.exhaust==9?3:c.exhaust==1||c.exhaust==5?1:2;
    for(int i=0;i<count;i++){
     float x=c.exhaust==2||c.exhaust==4||c.exhaust==10?(i==0?-.65f:.65f):c.exhaust==3?(i<2?-.65f:.65f)+(i%2==0?-.12f:.12f):(i-(count-1)*.5f)*.25f;
     bool side=c.exhaust==6,stacks=c.exhaust==7;var pos=side?new Vector3(i==0?-1.09f:1.09f,.5f,-.6f):new Vector3(x,stacks?1.3f:.44f,c.body==6?-1.8f:c.body==5?-2.55f:c.body==2?-1.96f:-2.21f);var rot=side?new Vector3(0,0,90):stacks?Vector3.zero:new Vector3(90,0,0);
     if(c.exhaust==4){var tube=RingMesh(root,"Hex exhaust",.064f,.105f,.46f,metal,6);tube.transform.localPosition=pos;tube.transform.localRotation=Quaternion.Euler(0,90,0);}
     else {Cylinder(root,"Exhaust barrel",pos,new Vector3(c.exhaust==5?.43f:c.exhaust==10?.23f:.17f,c.exhaust==10?.3f:.23f,.17f),metal,rot);Cylinder(root,"Exhaust opening",pos+(stacks?Vector3.up*.24f:side?new Vector3(i==0?-.24f:.24f,0,0):Vector3.back*(c.exhaust==10?.31f:.24f)),new Vector3(c.exhaust==5?.36f:.125f,.008f,.125f),dark,rot);}
     if(c.exhaust==10){var tip=RingMesh(root,"Heated titanium rim",.064f,.115f,.075f,ModelLibrary.Material("Titanium blue",new Color(.14f,.29f,.65f)));tip.transform.localPosition=pos+Vector3.back*.29f;tip.transform.localRotation=Quaternion.Euler(0,90,0);}
    }
    if(c.exhaust==8)Box(root,"Center exhaust shroud",new Vector3(0,.44f,c.body==6?-1.71f:c.body==5?-2.46f:c.body==2?-1.87f:-2.12f),new Vector3(.65f,.28f,.26f),dark);
   }
   if(c.livery>0){
    float h=new[]{.88f,1.015f,.87f,1.11f,.67f,1.66f,1.06f,1.05f,1.89f,.68f}[c.body];int count=c.livery==1||c.livery==3||c.livery==9?1:c.livery==4?3:2;
    if(c.livery==10){Box(root,"Number one",new Vector3(-.19f,h,1.1f),new Vector3(.05f,.015f,.45f),accent);for(int side=-1;side<=1;side+=2){Box(root,"Number zero sides",new Vector3(.14f+side*.115f,h,1.1f),new Vector3(.045f,.015f,.45f),accent);Box(root,"Number zero ends",new Vector3(.14f,h,1.1f+side*.20f),new Vector3(.23f,.015f,.045f),accent);}}
    else for(int i=0;i<count;i++){float x=c.livery==3?.48f:(i-(count-1)*.5f)*.31f;
     if(c.livery==5)Box(root,"Hood band",new Vector3(0,h,.9f+i*.35f),new Vector3(.9f,.015f,.12f),accent);
     else if(c.livery<=4||c.livery>=8)Box(root,"Hood graphic",new Vector3(x,h,1.1f),new Vector3(c.livery==9?1.1f:.14f,.015f,.73f),accent,new Vector3(0,c.livery==8?(i==0?30:-30):0,0));
     else for(int side=-1;side<=1;side+=2)Box(root,"Side graphic",new Vector3(side*1.035f,.65f,i*.4f),new Vector3(.015f,.14f,c.livery==7?.23f:.8f),accent);
    }
   }
   Combine(root);
  SupportStore.Apply(model,c);PaintStyles.Apply(model,c);
  }
  static void BuildWheel(Transform root,int kind,Color color,float radius){
   float r=Mathf.Clamp(radius,.4f,.6f);var rubber=ModelLibrary.Material("Tire",new Color(.025f,.03f,.035f));var rim=ModelLibrary.Material("Rim "+color, color);rim.SetFloat("_Metallic",.75f);rim.SetFloat("_Smoothness",.7f);var dark=ModelLibrary.Material("Carbon",new Color(.02f,.026f,.034f));var metal=ModelLibrary.Material("Metal",new Color(.6f,.64f,.69f));
   RingMesh(root,"Tire sidewall",r*.68f,r,.34f,rubber);RingMesh(root,"Alloy outer lip",r*.70f,r*.80f,.38f,rim);
   for(int side=-1;side<=1;side+=2){float x=side*(kind==7?.105f:.175f);Cylinder(root,"Alloy barrel",new Vector3(x,0,0),new Vector3(r*1.60f,.015f,r*1.60f),rim,new Vector3(0,0,90));Cylinder(root,"Rim recess",new Vector3(x+side*.02f,0,0),new Vector3(r*1.34f,.009f,r*1.34f),dark,new Vector3(0,0,90));
    int spokes=new[]{5,10,12,20,8,8,6,5,7,12}[kind];
    for(int j=0;j<spokes;j++){float a=j*360f/spokes;float rad=a*Mathf.Deg2Rad;float mid=r*.40f;float width=kind==2||kind==8?.115f:kind==3?.025f:.052f;if(kind!=9)Box(root,"Wheel spoke",new Vector3(x+side*.038f,Mathf.Cos(rad)*mid,Mathf.Sin(rad)*mid),new Vector3(.045f,r*.65f,width),rim,new Vector3(a+(kind==8?24:kind==1?10:kind==3?(j%2==0?22:-22):0),0,0));if(kind==5||kind==9)Cylinder(root,"Rim bolt",new Vector3(x+side*.045f,Mathf.Cos(rad)*r*.69f,Mathf.Sin(rad)*r*.69f),new Vector3(.045f,.02f,.045f),metal,new Vector3(0,0,90));}
    if(kind==9)for(int cell=0;cell<6;cell++){float ca=cell*Mathf.PI/3;Vector3 center=new Vector3(x+side*.038f,Mathf.Cos(ca)*r*.43f,Mathf.Sin(ca)*r*.43f);for(int edge=0;edge<6;edge++){float a=edge*Mathf.PI/3;Box(root,"Hexagonal wheel cell",center+new Vector3(0,Mathf.Cos(a)*r*.17f,Mathf.Sin(a)*r*.17f),new Vector3(.04f,r*.2f,.025f),rim,new Vector3(edge*60+90,0,0));}}
    if(kind==4)Cylinder(root,"Rally cover",new Vector3(x+side*.045f,0,0),new Vector3(r*1.17f,.013f,r*1.17f),rim,new Vector3(0,0,90));
    Cylinder(root,"Hub",new Vector3(x+side*.06f,0,0),new Vector3(r*.32f,.025f,r*.32f),metal,new Vector3(0,0,90));
   }
   for(int j=0;j<28;j++){float a=j*Mathf.PI*2/28;Box(root,"Tread",new Vector3(0,Mathf.Cos(a)*r,Mathf.Sin(a)*r),new Vector3(.30f,.015f,kind==5?.075f:.032f),dark,new Vector3(j*360f/28,0,0));}Combine(root);
  }
  static void BuildEngine(Transform parent,int kind,int body,Color color){
   var root=new GameObject(Engines[kind]).transform;root.SetParent(parent,false);root.localPosition=new[]{new Vector3(0,.95f,1.32f),new Vector3(0,1.05f,1.35f),new Vector3(0,.95f,1.26f),new Vector3(0,1.15f,1.28f),new Vector3(0,.96f,-1.42f),new Vector3(0,1.1f,2.02f),new Vector3(0,.9f,1.38f),new Vector3(0,1.07f,1.25f),new Vector3(0,1.87f,-.9f),new Vector3(0,.8f,-1.55f)}[body];root.localScale=Vector3.one*.75f;
   var metal=ModelLibrary.Material("Engine metal",new Color(.4f,.47f,.54f));metal.SetFloat("_Metallic",.85f);var red=ModelLibrary.Material("Engine covers "+color,color);var dark=ModelLibrary.Material("Carbon",new Color(.03f,.04f,.05f));var copper=ModelLibrary.Material("Engine copper",new Color(.75f,.34f,.13f));
   Box(root,"Engine block",Vector3.zero,new Vector3(.63f,.28f,.77f),metal);
   if(kind==1){Box(root,"Cam cover",new Vector3(0,.19f,0),new Vector3(.35f,.14f,.85f),red);for(int j=0;j<4;j++)Cylinder(root,"Intake",new Vector3(.28f,.25f,-.3f+j*.2f),new Vector3(.15f,.15f,.15f),metal);}
   else if(kind==4){for(int s=-1;s<=1;s+=2)for(int j=0;j<3;j++)Box(root,"Opposed cylinder",new Vector3(s*.46f,.12f,-.28f+j*.27f),new Vector3(.4f,.18f,.20f),red);}
   else if(kind==5){for(int j=0;j<3;j++)Cylinder(root,"Rotary housing",new Vector3(0,.18f,-.25f+j*.23f),new Vector3(.58f,.08f,.58f),metal,new Vector3(90,0,0));Box(root,"Rotary intake",new Vector3(0,.48f,0),new Vector3(.35f,.14f,.67f),red);}
   else if(kind==6){Cylinder(root,"Turbine casing",new Vector3(0,.26f,0),new Vector3(.62f,.55f,.62f),metal,new Vector3(90,0,0));Cylinder(root,"Turbine mouth",new Vector3(0,.26f,.57f),new Vector3(.48f,.03f,.48f),dark,new Vector3(90,0,0));for(int j=0;j<12;j++)Box(root,"Compressor blade",new Vector3(0,.26f,.61f),new Vector3(.035f,.44f,.045f),copper,new Vector3(0,0,j*30));}
   else if(kind==7){Box(root,"Battery module",new Vector3(0,.16f,0),new Vector3(.85f,.22f,.8f),red);for(int j=0;j<7;j++)Box(root,"Cooling fins",new Vector3(-.34f+j*.11f,.3f,0),new Vector3(.03f,.1f,.76f),metal);for(int s=-1;s<=1;s+=2)Cylinder(root,"Electric motor",new Vector3(s*.52f,.1f,0),new Vector3(.3f,.25f,.3f),copper,new Vector3(90,0,0));}
   else {int cylinders=kind==2?3:kind==10?6:4;for(int s=-1;s<=1;s+=2){Box(root,"Valve cover",new Vector3(s*.25f,.24f,0),new Vector3(.26f,.18f,.9f),red,new Vector3(0,0,-s*23));for(int j=0;j<cylinders;j++)Cylinder(root,"Velocity stack",new Vector3(s*.17f,.4f,-.36f+j*.72f/(cylinders-1)),new Vector3(.12f,.12f,.12f),metal,new Vector3(0,0,-s*18));}if(kind==8)for(int s=-1;s<=1;s+=2){Cylinder(root,"Turbo compressor",new Vector3(s*.52f,.19f,.23f),new Vector3(.32f,.14f,.32f),metal,new Vector3(90,0,0));Cylinder(root,"Turbo inlet",new Vector3(s*.52f,.19f,.38f),new Vector3(.22f,.015f,.22f),dark,new Vector3(90,0,0));}if(kind==9){Box(root,"Supercharger",new Vector3(0,.52f,0),new Vector3(.55f,.28f,.68f),metal);for(int j=0;j<3;j++)Cylinder(root,"Blower intake",new Vector3(-.17f+j*.17f,.58f,.37f),new Vector3(.14f,.07f,.14f),dark,new Vector3(90,0,0));}}
  }
 }
 public class OwnedMesh:MonoBehaviour {void OnDestroy(){var f=GetComponent<MeshFilter>();if(f&&f.sharedMesh)Destroy(f.sharedMesh);}}
}
