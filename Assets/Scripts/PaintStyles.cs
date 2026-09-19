using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public static class PaintStyles {
  public static readonly string[] Names={"Solid","Carbon","Glitter","Jelly","Forged Carbon","Pearl","Brushed Metal","Marble","Lava","Honeycomb","Liquid Chrome"};
  public static readonly string[] Files={"","Carbon","Glitter","Jelly","ForgedCarbon","Pearl","BrushedMetal","Marble","Lava","Honeycomb","LiquidChrome"};
  static readonly Dictionary<int,Material> materials=new Dictionary<int,Material>();
  public static Texture2D Texture(int style)=>Resources.Load<Texture2D>("PaintStyles/"+Files[Mathf.Clamp(style,1,10)]);
  public static void Apply(GameObject model,SavedCar c){
   if(c.paintStyle==0||c.glassChassis&&SupportStore.Owned("glass-chassis"))return;
   int key=c.paintStyle*32+c.paint;
   if(!materials.TryGetValue(key,out var mat)){
    mat=new Material(Shader.Find("Aether/Paint")){name="Paint Style "+Names[c.paintStyle],color=Game.Paints[c.paint]};
    var texture=Texture(c.paintStyle);texture.wrapMode=TextureWrapMode.Repeat;texture.anisoLevel=8;mat.SetTexture("_MainTex",texture);mat.SetFloat("_Style",c.paintStyle);
    mat.SetFloat("_Scale",new[]{1,1.5f,2.2f,.75f,1.1f,.7f,1.5f,.65f,.6f,1.8f,.6f}[c.paintStyle]);
    mat.SetFloat("_Metallic",c.paintStyle==3?.2f:c.paintStyle==7?.15f:c.paintStyle==1?.55f:.82f);
    mat.SetFloat("_Smoothness",c.paintStyle==6?.64f:c.paintStyle==1?.78f:.94f);mat.enableInstancing=true;materials[key]=mat;
   }
   foreach(var r in model.GetComponentsInChildren<MeshRenderer>())if(r.name=="Paint"||r.sharedMaterial.name.StartsWith("Paint "))r.sharedMaterial=mat;
  }
 }
}
