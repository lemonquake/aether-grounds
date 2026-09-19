using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
namespace Aether {
 public sealed class SupportProduct {
  public string id,name,description,price;public int body;public Color color;
  public SupportProduct(string i,string n,string d,string p,int b,Color c){id=i;name=n;description=d;price=p;body=b;color=c;}
 }
 public static partial class SupportStore {
  public const string Recipient="lemonquake@gmail.com";
  public static readonly SupportProduct[] Products={
   new SupportProduct("pip-city","Pip City Edition","Bubble compact with fender trim and a roof rack.","0.99",6,new Color(.9f,.59f,.22f)),
   new SupportProduct("comet-chrome","Comet Chrome Edition","Muscle coupe with chrome scoops and side pipes.","2.99",1,new Color(.75f,.83f,.9f)),
   new SupportProduct("nomad-night","Nomad Night Edition","Rally buggy with a roof light bar and brush guard.","5.99",3,new Color(.23f,.7f,.58f)),
   new SupportProduct("manta-gold","Manta Gold Edition","Prototype with gold aero fins and splitter.","9.99",9,new Color(.95f,.67f,.24f)),
   new SupportProduct("neon-wheels","Neon wheel kit","Cyan illuminated hub rings for your saved cars.","0.99",-1,Color.cyan),
   new SupportProduct("star-trail","Starlight trail","Violet boost particles with an acceleration upgrade.","1.99",-1,new Color(.7f,.4f,1)),
   new SupportProduct("vanta-gold","Golden Vanta","Polished gold body, aero kit and the strongest Vanta engine.","9.99",0,new Color(1,.7f,.22f)),
   new SupportProduct("vanta-chrome","Chrome Vanta","Mirror-finish body with upgraded brakes and high-speed grip.","7.99",0,new Color(.72f,.88f,1)),
   new SupportProduct("glass-wheels","Glass wheels","Transparent cyan alloy rims with improved grip and braking.","2.99",-1,new Color(.3f,.9f,1)),
   new SupportProduct("glass-chassis","Glass chassis","See-through body panels, visible frame and battery, plus a faster powertrain. Fits every car.","4.99",-1,new Color(.4f,1,.88f))};
  public static SupportProduct Product(string id)=>Array.Find(Products,p=>p.id==id);
  public static string PlayerId {get {var id=PlayerPrefs.GetString("support-player-id","");if(id.Length!=16){id=Guid.NewGuid().ToString("N").Substring(0,16);PlayerPrefs.SetString("support-player-id",id);PlayerPrefs.Save();}return id;}}
  public static string Checkout(SupportProduct p,string id)=>"https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business="+Uri.EscapeDataString(Recipient)+"&item_name="+Uri.EscapeDataString("Aether Grounds - "+p.name)+"&item_number="+p.id+"&amount="+p.price+"&currency_code=USD&no_shipping=1&custom="+Uri.EscapeDataString(id)+"&charset=utf-8";
  public static bool Verify(string token,string player,out string sku){sku="";try{var pieces=token.Trim().Split('.');if(pieces.Length!=2)return false;var data=Convert.FromBase64String(pieces[0]);var fields=Encoding.UTF8.GetString(data).Split('|');if(fields.Length!=4||fields[0]!="AG1"||fields[1]!=player||Product(fields[2])==null||fields[3].Length<4)return false;
    var key=Resources.Load<TextAsset>("Commerce/SupportPublicKey");if(!key)return false;using(var rsa=new RSACryptoServiceProvider()){rsa.FromXmlString(key.text);if(!rsa.VerifyData(data,CryptoConfig.MapNameToOID("SHA256"),Convert.FromBase64String(pieces[1])))return false;}sku=fields[2];return true;
   }catch{return false;}}
  static readonly System.Collections.Generic.Dictionary<string,bool> owned=new System.Collections.Generic.Dictionary<string,bool>();
  public static bool Owned(string sku){if(owned.TryGetValue(sku,out bool result))return result;return owned[sku]=CheckOwned(sku);}
  static bool CheckOwned(string sku){var token=PlayerPrefs.GetString("support-license-"+sku,"");return token.Length>0&&Verify(token,PlayerId,out var verified)&&verified==sku;}
  public static bool Redeem(string code){if(!Verify(code,PlayerId,out var sku))return false;PlayerPrefs.SetString("support-license-"+sku,code.Trim());PlayerPrefs.Save();owned.Remove(sku);return true;}
  public static void Apply(GameObject model,SavedCar car,bool preview=false){
   if(!string.IsNullOrEmpty(car.supportEdition)&&(preview||Owned(car.supportEdition))){
    var product=Product(car.supportEdition);if(product!=null&&product.body==car.body){var trim=ModelLibrary.Material("Support edition "+product.id,product.color,.08f);var kit=new GameObject(product.name+" body kit");kit.transform.SetParent(model.transform,false);
     if(product.id=="pip-city"||product.id=="nomad-night"){for(int s=-1;s<=1;s+=2)CarParts.Part(kit.transform,"Roof rail",PrimitiveType.Cube,new Vector3(s*.62f,car.body==3?1.7f:1.5f,-.2f),new Vector3(.08f,.09f,1.6f),trim);for(int i=-2;i<=2;i++)CarParts.Part(kit.transform,"Roof bar",PrimitiveType.Cube,new Vector3(0,car.body==3?1.72f:1.52f,i*.35f),new Vector3(1.4f,.07f,.07f),trim);}
     if(product.id=="nomad-night")for(int i=-2;i<=2;i++)CarParts.Part(kit.transform,"Rally lamp",PrimitiveType.Sphere,new Vector3(i*.25f,1.84f,.65f),Vector3.one*.21f,ModelLibrary.Material("Rally lamp",Color.white,1.4f));
     for(int s=-1;s<=1;s+=2){CarParts.Part(kit.transform,"Side sill",PrimitiveType.Cube,new Vector3(s*.98f,.36f,0),new Vector3(.17f,.15f,2.5f),trim);if(product.id=="comet-chrome")CarParts.Part(kit.transform,"Chrome side pipe",PrimitiveType.Cylinder,new Vector3(s*1.02f,.45f,-.15f),new Vector3(.18f,1.1f,.18f),trim,new Vector3(90,0,0));if(product.id=="manta-gold")CarParts.Part(kit.transform,"Aero fin",PrimitiveType.Cube,new Vector3(s*.96f,.8f,-1.8f),new Vector3(.08f,.8f,.75f),trim,new Vector3(0,0,s*8));}
     CarParts.Part(kit.transform,"Front splitter",PrimitiveType.Cube,new Vector3(0,.29f,1.9f),new Vector3(2.12f,.08f,.38f),trim);
    }
   }
   if(car.neonWheels&&(preview||Owned("neon-wheels")))foreach(string name in new[]{"WheelFL","WheelFR","WheelRL","WheelRR"}){var hub=model.transform.Find(name);if(hub)CarParts.Part(hub,"Neon hub",PrimitiveType.Cylinder,new Vector3(name.EndsWith("L")?-.24f:.24f,0,0),new Vector3(.57f,.024f,.57f),ModelLibrary.Material("Neon hubs",Color.cyan,1.8f),new Vector3(0,0,90));}
   ApplyFinishes(model,car,preview);
  }
 }
 public partial class Game {
  int supportChoice;string unlockCode="";
  void SupportGUI(){StoreGUI();}
 }
}
