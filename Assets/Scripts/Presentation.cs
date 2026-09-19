using UnityEngine;
using UnityEngine.Rendering;
namespace Aether {
 public class Presentation:MonoBehaviour {
  Material post;
  void OnEnable(){GetComponent<Camera>().depthTextureMode|=DepthTextureMode.Depth;post=new Material(Shader.Find("Hidden/Aether/Post"));}
  void OnRenderImage(RenderTexture source,RenderTexture destination){
   if(!post){Graphics.Blit(source,destination);return;}
   UpdateSunGlare();var a=RenderTexture.GetTemporary(Mathf.Max(1,source.width/4),Mathf.Max(1,source.height/4),0,source.format);
   var b=RenderTexture.GetTemporary(a.width,a.height,0,source.format);Graphics.Blit(source,a,post,0);
   Graphics.Blit(a,b,post,1);Graphics.Blit(b,a,post,1);post.SetTexture("_Bloom",a);Graphics.Blit(source,destination,post,2);
   RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);
  }
  void UpdateSunGlare(){var game=GetComponent<Camera>();var sun=GameObject.Find("Sun");float strength=0;Vector3 uv=Vector3.zero;if(sun&&sun.GetComponent<Light>().intensity>1.4f){Vector3 direction=-sun.transform.forward;uv=game.WorldToViewportPoint(game.transform.position+direction*1000);if(uv.z>0&&!Physics.Raycast(game.transform.position,direction,1200,~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore))strength=Mathf.Pow(Mathf.Clamp01(Vector3.Dot(game.transform.forward,direction)),8)*.18f;}post.SetVector("_Sun",new Vector4(uv.x,uv.y,strength,(float)game.pixelWidth/game.pixelHeight));}
  void OnDestroy(){if(post)Destroy(post);}
  public static void Lighting(int theme){
   var sun=GameObject.Find("Sun").GetComponent<Light>();
   sun.color=theme==1?new Color(1,.79f,.56f):theme==2?new Color(.61f,.72f,1):new Color(1,.94f,.84f);
   sun.intensity=theme<0?.8f:theme==2?.85f:1.35f;sun.transform.rotation=Quaternion.Euler(theme==1?24:48,theme==1?-70:-32,0);
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=theme<0?new Color(.24f,.29f,.35f):theme==2?new Color(.24f,.3f,.51f):new Color(.53f,.65f,.79f);
   RenderSettings.ambientEquatorColor=new Color(.3f,.36f,.44f);RenderSettings.ambientGroundColor=new Color(.16f,.18f,.22f);
   QualitySettings.shadowDistance=180;QualitySettings.shadowCascades=4;QualitySettings.shadowResolution=ShadowResolution.High;
   RenderSettings.reflectionIntensity=.8f;DynamicGI.UpdateEnvironment();
  }
  public static void Probe(Transform parent,Vector3 pos){var o=new GameObject("Environment reflections");o.transform.SetParent(parent);o.transform.position=pos;var p=o.AddComponent<ReflectionProbe>();p.mode=ReflectionProbeMode.Realtime;p.refreshMode=ReflectionProbeRefreshMode.ViaScripting;p.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;p.resolution=128;p.size=Vector3.one*5000;p.farClipPlane=2000;p.cullingMask=~((1<<8)|(1<<9));p.RenderProbe();}
 }
}
