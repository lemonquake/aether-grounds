using UnityEngine;
namespace Aether {
 public partial class Game {
  void ApplyInventoryVisual(GameObject model){if(inventory==null)return;var root=new GameObject("Equipped Rush gear").transform;root.SetParent(model.transform,false);var metal=ModelLibrary.Material("Rush attachment steel",new Color(.11f,.15f,.2f));metal.SetFloat("_Metallic",.75f);var copper=ModelLibrary.Material("Solar turbo copper",new Color(.9f,.38f,.12f));copper.SetFloat("_Metallic",.65f);
   if(inventory.engine=="sun-turbo"){for(int s=-1;s<=1;s+=2){CarParts.Part(root,"Solar turbo housing",PrimitiveType.Cylinder,new Vector3(s*.42f,1.17f,.85f),new Vector3(.34f,.20f,.34f),copper,new Vector3(90,0,0));CarParts.Part(root,"Turbo intake",PrimitiveType.Cylinder,new Vector3(s*.42f,1.17f,1.07f),new Vector3(.23f,.015f,.23f),metal,new Vector3(90,0,0));CarParts.Part(root,"Turbo pipe",PrimitiveType.Cube,new Vector3(s*.42f,1.04f,.53f),new Vector3(.13f,.13f,.5f),metal);}}
   if(inventory.engine=="storm-coil"){var glow=ModelLibrary.Material("Storm coil glow",new Color(.14f,.59f,1),1.6f);CarParts.Part(root,"Storm capacitor",PrimitiveType.Cylinder,new Vector3(0,1.14f,-.82f),new Vector3(.3f,.47f,.3f),metal,new Vector3(0,0,90));for(int i=-2;i<=2;i++)CarParts.Part(root,"Coil band",PrimitiveType.Cylinder,new Vector3(i*.17f,1.14f,-.82f),new Vector3(.34f,.022f,.34f),glow,new Vector3(0,0,90));}
   if(inventory.chassis=="rock-plate")for(int s=-1;s<=1;s+=2){CarParts.Part(root,"Redrock side plate",PrimitiveType.Cube,new Vector3(s*1.04f,.55f,0),new Vector3(.16f,.32f,1.7f),metal);for(int i=-1;i<=1;i++)CarParts.Part(root,"Plating bolt",PrimitiveType.Sphere,new Vector3(s*1.14f,.55f,i*.6f),Vector3.one*.075f,copper);}
   if(root.childCount>0)CarParts.Combine(root);
  }
  void RushLoadingGUI(){DrawRushBackdrop(rushMode?rushIndex:0);Panel(448,344,704,204,.96f);Label(rushMode?Challenge.name:TrackWorld.Names[mapIndex],481,363,638,57,37,paper,true);Label("Preparing your race",485,439,620,33,23,muted);for(int i=0;i<12;i++){GUI.color=i<3+(int)(Time.unscaledTime*5)%10?orange:new Color(.18f,.23f,.29f);GUI.DrawTexture(new Rect(484+i*52,503,42,6),Texture2D.whiteTexture);}GUI.color=Color.white;}
 }
}
