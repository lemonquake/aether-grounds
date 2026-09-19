using UnityEngine;
namespace Aether {
 public static class RaceEffects {
  public static void Teleport(Vector3 p){Ring(p,new Color(.2f,1,1,.9f),4.2f);Burst(p+Vector3.up,new Color(.3f,.95f,1,.9f),48,6,.18f,.8f,-.5f);Burst(p+Vector3.up*.5f,new Color(.7f,1,1,.65f),14,2,.7f,.5f);var light=new GameObject("Teleport flash");light.transform.position=p+Vector3.up;var l=light.AddComponent<Light>();l.color=Color.cyan;l.range=9;l.intensity=4;light.AddComponent<ImpactFlash>();}
  public static ParticleSystem Emitter(Transform parent,string name,Color color,float life,float size,float speed,int max=120){
   var o=new GameObject(name);if(parent)o.transform.SetParent(parent,false);var ps=o.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var m=ps.main;m.startLifetime=new ParticleSystem.MinMaxCurve(life*.6f,life);m.startSize=new ParticleSystem.MinMaxCurve(size*.5f,size);m.startSpeed=new ParticleSystem.MinMaxCurve(speed*.35f,speed);m.startColor=color;m.simulationSpace=ParticleSystemSimulationSpace.World;m.maxParticles=max;
   var e=ps.emission;e.rateOverTime=0;var s=ps.shape;s.shapeType=ParticleSystemShapeType.Cone;s.angle=18;s.radius=.16f;var c=ps.colorOverLifetime;c.enabled=true;var grad=new Gradient();grad.SetKeys(new[]{new GradientColorKey(color,0),new GradientColorKey(color*.6f,1)},new[]{new GradientAlphaKey(color.a,0),new GradientAlphaKey(0,1)});c.color=grad;
   var sz=ps.sizeOverLifetime;sz.enabled=true;sz.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.5f,1,1.7f));ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Game.FXMaterial();ps.Play();return ps;
  }
  public static void Burst(Vector3 pos,Color color,int count,float speed,float size,float life,float gravity=0){
   var ps=Emitter(null,"Burst particles",color,life,size,speed,count);ps.transform.position=pos;ps.transform.rotation=Quaternion.Euler(-90,0,0);var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Hemisphere;shape.radius=.35f;var m=ps.main;m.gravityModifier=gravity;m.loop=false;ps.Emit(count);Object.Destroy(ps.gameObject,life+.3f);
  }
  public static void Splash(Vector3 pos,float speed){Burst(pos,new Color(.57f,.87f,.98f,.8f),16,Mathf.Clamp(speed*.18f,3,9),.23f,.75f,1.1f);Burst(pos,new Color(.83f,.96f,1,.45f),7,2,.8f,.45f);}
  public static void Explosion(Vector3 p,float scale=1,Vehicle source=null){
   CityPhysics.Blast(p,18*scale,24*scale,source);
   Burst(p,new Color(1,.4f,.075f),60,12*scale,1.3f*scale,.65f);Burst(p+Vector3.up*.3f,new Color(.15f,.16f,.19f,.8f),32,5*scale,2*scale,2.1f,-.15f);Burst(p,new Color(1,.83f,.36f),42,18*scale,.15f,.8f,1.1f);
   var o=new GameObject("Explosion flash");o.transform.position=p;var l=o.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.42f,.15f);l.range=18*scale;l.intensity=5;o.AddComponent<ImpactFlash>();
   Ring(p,new Color(1,.68f,.28f,.8f),12*scale);Debris(p,new Color(.18f,.21f,.27f),8,10*scale);
  }
  public static void Ring(Vector3 p,Color color,float radius){var o=new GameObject("Impact wave");o.transform.position=p+Vector3.up*.15f;var line=o.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=48;line.widthMultiplier=.14f;line.sharedMaterial=Game.FXMaterial();line.startColor=line.endColor=color;for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));}var ring=o.AddComponent<ImpactRing>();ring.radius=radius;ring.color=color;}
  static int debrisCount;
  public static void Debris(Vector3 pos,Color color,int count,float speed){for(int i=0;i<count&&debrisCount<240;i++){debrisCount++;var o=CarParts.Part(null,"Broken fragments",PrimitiveType.Cube,pos+Random.insideUnitSphere*.45f,new Vector3(Random.Range(.12f,.35f),.09f,Random.Range(.2f,.65f)),ModelLibrary.Material("Debris "+color,color),Random.insideUnitSphere*180);o.layer=9;var c=o.AddComponent<BoxCollider>();var b=o.AddComponent<Rigidbody>();b.mass=.3f;b.linearVelocity=Random.onUnitSphere*speed+Vector3.up*5;b.angularVelocity=Random.insideUnitSphere*18;o.AddComponent<Fragment>();Object.Destroy(o,2.5f);}}
  public static void Released(){debrisCount=Mathf.Max(0,debrisCount-1);}
  public static GameObject Crate(){var o=new GameObject("Power-up crate");var m=ModelLibrary.Material("Crate timber",new Color(.25f,.17f,.09f));var metal=ModelLibrary.Material("Crate frame",new Color(.2f,.27f,.34f));var glow=ModelLibrary.Material("Crate power",new Color(.15f,.9f,.72f),2);
   CarParts.Part(o.transform,"Core",PrimitiveType.Cube,Vector3.zero,Vector3.one*1.18f,m);
   for(int s=-1;s<=1;s+=2)for(int j=-1;j<=1;j+=2){CarParts.Part(o.transform,"Frame",PrimitiveType.Cube,new Vector3(s*.62f,j*.62f,0),new Vector3(.12f,.12f,1.38f),metal);CarParts.Part(o.transform,"Frame",PrimitiveType.Cube,new Vector3(s*.62f,0,j*.62f),new Vector3(.12f,1.38f,.12f),metal);CarParts.Part(o.transform,"Frame",PrimitiveType.Cube,new Vector3(0,s*.62f,j*.62f),new Vector3(1.38f,.12f,.12f),metal);}
   for(int s=-1;s<=1;s+=2){CarParts.Part(o.transform,"Power symbol",PrimitiveType.Cube,new Vector3(0,0,s*.607f),new Vector3(.47f,.47f,.035f),glow,new Vector3(0,0,45));CarParts.Part(o.transform,"Power symbol",PrimitiveType.Cube,new Vector3(s*.607f,0,0),new Vector3(.035f,.47f,.47f),glow,new Vector3(45,0,0));}CarParts.Combine(o.transform);return o;
  }
  public static GameObject Missile(){var o=new GameObject("Homing missile");var metal=ModelLibrary.Material("Missile shell",new Color(.66f,.72f,.78f));var red=ModelLibrary.Material("Missile fins",new Color(.85f,.18f,.065f));var dark=ModelLibrary.Material("Carbon",new Color(.04f,.05f,.065f));
   CarParts.Part(o.transform,"Fuselage",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.3f,.65f,.3f),metal,new Vector3(90,0,0));CarParts.Part(o.transform,"Nose",PrimitiveType.Sphere,new Vector3(0,0,.68f),new Vector3(.3f,.3f,.48f),red);
   for(int i=0;i<4;i++){var fin=CarParts.Part(o.transform,"Tail fin",PrimitiveType.Cube,Vector3.zero,new Vector3(.08f,.62f,.44f),red,new Vector3(0,0,i*90));fin.transform.localPosition=new Vector3(0,0,-.4f);}CarParts.Part(o.transform,"Nozzle",PrimitiveType.Cylinder,new Vector3(0,0,-.69f),new Vector3(.26f,.08f,.26f),dark,new Vector3(90,0,0));CarParts.Combine(o.transform);
   var fire=Emitter(o.transform,"Missile fire",new Color(1,.46f,.1f),.22f,.45f,4);fire.transform.localPosition=Vector3.back*.8f;fire.transform.localRotation=Quaternion.Euler(0,180,0);var e=fire.emission;e.rateOverTime=65;
   var smoke=Emitter(o.transform,"Missile smoke",new Color(.33f,.36f,.4f,.6f),1.1f,.62f,1);smoke.transform.localPosition=Vector3.back*.85f;e=smoke.emission;e.rateOverTime=28;return o;
  }
 }
 public class Fragment:MonoBehaviour {void OnDestroy(){RaceEffects.Released();}}
 public class ImpactFlash:MonoBehaviour {float age;void Update(){age+=Time.deltaTime;GetComponent<Light>().intensity=Mathf.Max(0,5-age*22);if(age>.3f)Destroy(gameObject);}}
 public class ImpactRing:MonoBehaviour {public float radius;public Color color;float age;void Update(){age+=Time.deltaTime;transform.localScale=Vector3.one*Mathf.Lerp(.5f,radius,age/.5f);color.a=Mathf.Max(0,1-age/.5f);GetComponent<LineRenderer>().startColor=GetComponent<LineRenderer>().endColor=color;if(age>.5f)Destroy(gameObject);}}
}
