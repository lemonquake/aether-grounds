using UnityEngine;
namespace Aether {
 public class RushWeather:MonoBehaviour {
  ParticleSystem rain;Game game;LineRenderer lightning;float nextFlash;float fade;
  void Start(){game=FindAnyObjectByType<Game>();rain=RaceEffects.Emitter(transform,"Bridge rain",new Color(.51f,.68f,.9f,.25f),1,.04f,24,1100);var main=rain.main;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=1600;var shape=rain.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(65,90,1);var renderer=rain.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=4;renderer.velocityScale=.05f;rain.transform.localRotation=Quaternion.Euler(90,0,8);
   lightning=new GameObject("Distant lightning").AddComponent<LineRenderer>();lightning.transform.SetParent(transform);lightning.sharedMaterial=Game.FXMaterial();lightning.positionCount=9;lightning.widthMultiplier=1.7f;lightning.useWorldSpace=true;lightning.enabled=false;nextFlash=8;
  }
  void Update(){if(!game||!game.cam||!rain)return;rain.transform.position=game.cam.transform.position+Vector3.up*28+Vector3.forward*30;if(game.state!=Game.State.Race)return;nextFlash-=Time.deltaTime;if(nextFlash<=0){nextFlash=11+Random.value*7;fade=.6f;Vector3 p=game.cam.transform.position+new Vector3(450,230,850);for(int i=0;i<9;i++)lightning.SetPosition(i,p+new Vector3(i==0?0:Mathf.Sin(i*17)*18,-i*27,i*5));lightning.enabled=true;}if(fade>0){fade-=Time.deltaTime;lightning.startColor=lightning.endColor=new Color(.48f,.65f,1,Mathf.Clamp01(fade*1.6f));if(fade<=0)lightning.enabled=false;}}
 }
}
