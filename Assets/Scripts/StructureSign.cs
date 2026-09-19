using UnityEngine;
namespace Aether {
 // World signs render only their readable face. This avoids mirrored letters
 // showing through or above the back of a building/gantry sign.
 public class StructureSign:MonoBehaviour {
  public Vector2 fit;Material material;
  void Start(){
   var text=GetComponent<TextMesh>();var renderer=GetComponent<MeshRenderer>();
   material=new Material(Shader.Find("Aether/StructureText"));material.mainTexture=renderer.sharedMaterial.mainTexture;renderer.sharedMaterial=material;
   if(fit.x>0){var bounds=renderer.localBounds;float scale=Mathf.Min(1,Mathf.Min(fit.x/Mathf.Max(.001f,bounds.size.x),fit.y/Mathf.Max(.001f,bounds.size.y)));transform.localScale=Vector3.one*scale;}
  }
  void OnDestroy(){if(material)Destroy(material);}
 }
}
