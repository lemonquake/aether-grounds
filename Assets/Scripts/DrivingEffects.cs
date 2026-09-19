using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class Vehicle {
  ParticleSystem[] contactSmoke;TrailRenderer[] aura;float lastFxSpeed,launchSmoke,ghostClock;int ghostIndex;
  readonly List<SpeedAfterimage> ghosts=new List<SpeedAfterimage>();
  public float smokeIntensity;public int boostGhostCount;
  static Material smokeMat;
  static Material SmokeMaterial(){
   if(smokeMat)return smokeMat;
   var tex=new Texture2D(96,96,TextureFormat.RGBA32,false);var pixels=new Color[96*96];
   for(int y=0;y<96;y++)for(int x=0;x<96;x++){float r=new Vector2(x-47.5f,y-47.5f).magnitude/47.5f;float n=Mathf.PerlinNoise(x*.065f,y*.065f)*.65f+Mathf.PerlinNoise(x*.16f+9,y*.16f)*.35f;float a=Mathf.Pow(Mathf.Clamp01(1-r),1.5f)*Mathf.SmoothStep(.2f,1,n+.2f);pixels[y*96+x]=new Color(.7f+n*.3f,.7f+n*.3f,.7f+n*.3f,a);}
   tex.SetPixels(pixels);tex.Apply();smokeMat=new Material(Shader.Find("Aether/TireSmoke"));smokeMat.SetTexture("_MainTex",tex);smokeMat.SetFloat("_Mode",0);smokeMat.SetInt("_SrcBlend",5);smokeMat.SetInt("_DstBlend",10);smokeMat.SetInt("_ZWrite",0);smokeMat.EnableKeyword("_ALPHABLEND_ON");smokeMat.renderQueue=3000;return smokeMat;
  }
  void BuildDrivingEffects(){
   contactSmoke=new ParticleSystem[4];
   for(int i=0;i<4;i++){
    var ps=RaceEffects.Emitter(transform,"Wheel contact smoke "+i,Color.white,1.7f,.55f,1.4f,0);contactSmoke[i]=ps;
    ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var m=ps.main;m.simulationSpace=ParticleSystemSimulationSpace.World;m.maxParticles=player?160:65;m.startLifetime=new ParticleSystem.MinMaxCurve(1.1f,2.1f);m.startRotation=new ParticleSystem.MinMaxCurve(0,6.28f);m.startSize=new ParticleSystem.MinMaxCurve(.45f,.8f);m.gravityModifier=-.09f;
    var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.16f;
    var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(.68f,.71f,.75f),0),new GradientColorKey(new Color(.82f,.85f,.89f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.65f,.09f),new GradientAlphaKey(.26f,.6f),new GradientAlphaKey(0,1)});color.color=gradient;
    var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.5f,1,4.8f));
    var noise=ps.noise;noise.enabled=true;noise.strength=.45f;noise.frequency=.4f;noise.scrollSpeed=.3f;noise.quality=ParticleSystemNoiseQuality.Low;
    ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=SmokeMaterial();ps.Play();
   }
   aura=new TrailRenderer[4];for(int i=0;i<4;i++){var o=new GameObject("Boost aura ribbon "+i);o.transform.SetParent(transform,false);o.transform.localPosition=new Vector3(i%2==0?-1.02f:1.02f,i<2?.48f:1.1f,i<2?-1.4f:-.4f);var t=o.AddComponent<TrailRenderer>();aura[i]=t;t.time=.48f;t.minVertexDistance=.2f;t.widthMultiplier=i<2?.45f:.15f;t.widthCurve=AnimationCurve.Linear(0,1,1,0);t.sharedMaterial=Game.FXMaterial();t.startColor=i<2?new Color(.12f,.85f,1,.65f):new Color(.65f,.35f,1,.7f);t.endColor=new Color(.1f,.4f,1,0);t.emitting=false;t.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
  }
  void UpdateDrivingEffects(bool active){
   if(contactSmoke==null)BuildDrivingEffects();
   float forward=Vector3.Dot(body.linearVelocity,transform.forward),slip=Mathf.Abs(Vector3.Dot(body.linearVelocity,transform.right));
   if(active&&throttle>.7f&&speed<8&&lastFxSpeed<8)launchSmoke=.75f;
   launchSmoke=Mathf.Max(0,launchSmoke-Time.fixedDeltaTime);
   float skid=drift&&speed>25?Mathf.Clamp01(speed/110):Mathf.InverseLerp(2.5f,11,slip);
   float brake=throttle<-.2f&&forward>15?Mathf.Clamp01((forward-15)/25):0;
   float burn=throttle>.7f&&speed<95?launchSmoke*Mathf.Clamp01(speed/8+.3f):0;
   bool near=player||(game.cam.transform.position-transform.position).sqrMagnitude<130*130;
   smokeIntensity=active&&!wet&&near?Mathf.Max(skid,brake,burn):0;
   for(int i=0;i<4;i++){
    Vector3 origin=transform.TransformPoint(hubs[i]+Vector3.up*.3f);bool contact=Physics.Raycast(origin,-transform.up,out var hit,radius+.75f,~((1<<8)|(1<<9)),QueryTriggerInteraction.Ignore);
    float strength=i<2?brake*.85f:smokeIntensity;
    var ps=contactSmoke[i];if(contact){ps.transform.position=hit.point+hit.normal*.12f;ps.transform.rotation=Quaternion.LookRotation(hit.normal);}
    var e=ps.emission;e.rateOverTime=active&&near&&!wet&&contact?strength*(player?58:28):0;
    if(i>=2&&contact&&active&&brake>.1f){trails[i-2].transform.position=hit.point+hit.normal*.025f;trails[i-2].emitting=true;}
   }
   lastFxSpeed=speed;bool boosting=active&&boost>0&&speed>35&&near;
   foreach(var t in aura){if(t.positionCount>0&&(t.GetPosition(t.positionCount-1)-t.transform.position).sqrMagnitude>2500)t.Clear();t.emitting=boosting;}
   ghostClock-=Time.fixedDeltaTime;
   if(boosting&&player&&speed>90&&ghostClock<=0){ghostClock=.085f;SpeedAfterimage ghost;if(ghosts.Count<6){ghost=new GameObject("Boost afterimage").AddComponent<SpeedAfterimage>();ghost.transform.SetParent(transform);ghost.Build(model);ghosts.Add(ghost);}else{ghost=ghosts[ghostIndex];ghostIndex=(ghostIndex+1)%6;}ghost.Show(model);boostGhostCount++;}
  }
 }
 public class SpeedAfterimage:MonoBehaviour {
  class Part {public Transform source,target;}
  readonly List<Part> parts=new List<Part>();Material material;float age=1;Vector3 frozenPosition;Quaternion frozenRotation;
  public void Build(GameObject model){material=new Material(Shader.Find("Aether/SpeedGhost"));foreach(var mf in model.GetComponentsInChildren<MeshFilter>()){if(!mf.gameObject.activeInHierarchy)continue;var g=new GameObject("Afterimage mesh");g.transform.SetParent(transform);g.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;parts.Add(new Part{source=mf.transform,target=g.transform});}}
  public void Show(GameObject model){age=0;frozenPosition=model.transform.position;frozenRotation=model.transform.rotation;transform.SetPositionAndRotation(frozenPosition,frozenRotation);foreach(var p in parts)if(p.source){p.target.SetPositionAndRotation(p.source.position,p.source.rotation);p.target.localScale=p.source.lossyScale;}gameObject.SetActive(true);}
  void LateUpdate(){if(age>=.45f)return;age+=Time.deltaTime;transform.SetPositionAndRotation(frozenPosition,frozenRotation);material.color=new Color(.12f,.75f,1,.3f*Mathf.Pow(Mathf.Clamp01(1-age/.45f),1.4f));if(age>=.45f)gameObject.SetActive(false);}
  void OnDestroy(){if(material)Destroy(material);}
 }
}

