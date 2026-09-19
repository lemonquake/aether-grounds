using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Aether {
 // Surface-mounted chevrons follow each local road tangent. Their moving light
 // sequence always travels in race direction, including through corners.
 public class RoadGuideLights:MonoBehaviour {
  readonly List<Renderer> arrows=new List<Renderer>();readonly List<float> distances=new List<float>();
  MaterialPropertyBlock pulse;TrackWorld track;float length;
  public int ArrowCount=>arrows.Count;
  public static void Build(TrackWorld track){var root=new GameObject("Pulsing road direction lights");root.transform.SetParent(track.transform,false);root.AddComponent<RoadGuideLights>().Initialize(track);}
  void Initialize(TrackWorld world){
   track=world;pulse=new MaterialPropertyBlock();var material=ModelLibrary.Material("Road guide turquoise",new Color(.045f,.85f,.69f),2);
   float distance=0;
   for(int n=0;n<TrackWorld.Count;n++){
    if(n>0)distance+=Vector3.Distance(track.points[n-1],track.points[n]);
    if(n%2!=0)continue;
    // Keep the lights visible alongside the pickups and off raised jump tiles.
    float lane=track.Broken(n)?7:track.map==1?.65f:2.5f;
    var p=track.points[n]+track.Right(n)*lane+Vector3.up*.085f;
    var root=new GameObject("Forward road chevron "+n).transform;root.SetParent(transform,false);root.SetPositionAndRotation(p,Quaternion.LookRotation(track.Forward(n)));
    foreach(int side in new[]{-1,1})CarParts.Part(root,"Embedded light",PrimitiveType.Cube,new Vector3(side*.32f,0,0),new Vector3(.15f,.026f,.92f),material,new Vector3(0,side*-43,0));
    CarParts.Combine(root);
    foreach(var renderer in root.GetComponentsInChildren<Renderer>()){if(!renderer.enabled)continue;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;arrows.Add(renderer);distances.Add(distance);}
   }
   length=track.length;
  }
  void Update(){
   float cycles=Mathf.Max(1,Mathf.Round(length/34));var cam=Camera.main;
   for(int i=0;i<arrows.Count;i++){
    var renderer=arrows[i];if(!renderer)continue;
    bool visible=!cam||(renderer.transform.position-cam.transform.position).sqrMagnitude<200*200;renderer.enabled=visible;if(!visible)continue;
    // Increasing distance receives each pulse later: light chases forward.
    float phase=Time.time*1.3f-distances[i]/Mathf.Max(1,length)*cycles;
    float wave=Mathf.Pow(.5f+.5f*Mathf.Cos(phase*Mathf.PI*2),4);
    pulse.SetFloat("_Emission",.65f+wave*4.5f);renderer.SetPropertyBlock(pulse);
   }
  }
 }
}
