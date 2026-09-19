using UnityEngine;
using UnityEngine.Rendering;
namespace Aether {
 public static class PremiumLighting {
  public static void Environment(TrackWorld track){
   var sun=GameObject.Find("Sun").GetComponent<Light>();bool night=track.map==1;
   if(track.map==0){sun.transform.rotation=Quaternion.Euler(18,-68,0);sun.color=new Color(1,.73f,.44f);sun.intensity=1.5f;
    RenderSettings.ambientSkyColor=new Color(.34f,.43f,.58f);RenderSettings.ambientEquatorColor=new Color(.28f,.25f,.24f);RenderSettings.ambientGroundColor=new Color(.15f,.14f,.14f);RenderSettings.fogColor=new Color(.65f,.56f,.46f);RenderSettings.fogDensity=.0009f;
   }
   if(night){sun.intensity=0;RenderSettings.ambientSkyColor=new Color(.012f,.02f,.037f);RenderSettings.ambientEquatorColor=new Color(.009f,.015f,.026f);RenderSettings.ambientGroundColor=new Color(.005f,.008f,.014f);RenderSettings.reflectionIntensity=.12f;
    RenderSettings.fogColor=new Color(.008f,.014f,.026f);RenderSettings.fogDensity=.008f;
   }
   var sky=RenderSettings.skybox;if(sky){sky.SetColor("_Top",night?new Color(.002f,.004f,.01f):new Color(.18f,.34f,.5f));sky.SetColor("_Horizon",night?new Color(.008f,.014f,.027f):new Color(.88f,.61f,.36f));sky.SetFloat("_Night",night?1:0);}
   if(night){var mist=RaceEffects.Emitter(track.transform,"Low night mist",new Color(.26f,.36f,.48f,.085f),7,8,.3f,90);var shape=mist.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(75,1,90);var emission=mist.emission;emission.rateOverTime=9;mist.gameObject.AddComponent<FollowMist>();}
   DynamicGI.UpdateEnvironment();
  }
  public static void Headlights(Transform car,bool player,int map){if(map!=1)return;
   for(int s=-1;s<=1;s+=2){var o=new GameObject("Car headlight");o.transform.SetParent(car,false);o.transform.localPosition=new Vector3(s*.65f,.84f,1.9f);o.transform.localRotation=Quaternion.Euler(6,0,0);var light=o.AddComponent<Light>();light.type=LightType.Spot;light.range=player?48:20;light.spotAngle=64;light.innerSpotAngle=29;light.color=new Color(.78f,.86f,1);light.intensity=player?2.1f:.4f;light.shadows=player?LightShadows.Soft:LightShadows.None;light.shadowBias=.03f;
    if(player)Beam(o.transform,38,18,new Color(.43f,.63f,1,.012f));
   }
  }
  public static void Streetlight(Transform post,int map){if(map!=1)return;
   var o=new GameObject("Streetlamp pool");o.transform.SetParent(post,false);o.transform.localPosition=new Vector3(0,8.7f,-2.2f);o.transform.localRotation=Quaternion.Euler(90,0,0);
   var light=o.AddComponent<Light>();light.type=LightType.Spot;light.range=20;light.spotAngle=112;light.innerSpotAngle=62;light.color=new Color(1,.74f,.43f);light.intensity=2.4f;light.shadows=LightShadows.None;
   Beam(o.transform,13,12,new Color(1,.66f,.3f,.018f));o.AddComponent<NearbyLamp>();
  }
  static Mesh cone;
  public static void Beam(Transform parent,float length,float radius,Color color){
   if(!cone){cone=new Mesh{name="Volumetric cone"};var v=new Vector3[26];var t=new int[24*6];v[0]=Vector3.zero;v[25]=Vector3.forward;for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24;v[i+1]=new Vector3(Mathf.Cos(a),Mathf.Sin(a),1);int next=(i+1)%24+1;t[i*6]=0;t[i*6+1]=next;t[i*6+2]=i+1;t[i*6+3]=25;t[i*6+4]=i+1;t[i*6+5]=next;}cone.vertices=v;cone.triangles=t;cone.RecalculateNormals();}
   var o=new GameObject("Volumetric light beam");o.transform.SetParent(parent,false);o.transform.localScale=new Vector3(radius,radius,length);o.AddComponent<MeshFilter>().sharedMesh=cone;var renderer=o.AddComponent<MeshRenderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;var mat=new Material(Shader.Find("Aether/LightVolume"));mat.SetColor("_Color",color);renderer.sharedMaterial=mat;o.AddComponent<OwnedBeamMaterial>();
  }
 }
 public class OwnedBeamMaterial:MonoBehaviour {void OnDestroy(){Destroy(GetComponent<Renderer>().sharedMaterial);}}
 public class NearbyLamp:MonoBehaviour {Light bulb;Renderer beam;RespawningStreetLamp post;float timer;void Start(){bulb=GetComponent<Light>();beam=GetComponentInChildren<Renderer>();post=GetComponentInParent<RespawningStreetLamp>();}void Update(){timer-=Time.deltaTime;if(timer>0)return;timer=.25f;bool on=Camera.main&&(Camera.main.transform.position-transform.position).sqrMagnitude<85*85&&post.remaining<=0;bulb.enabled=on;if(beam)beam.enabled=on;}}
 public class FollowMist:MonoBehaviour {void Update(){if(Camera.main)transform.position=Camera.main.transform.position+Camera.main.transform.forward*25-Vector3.up*3;}}
}
