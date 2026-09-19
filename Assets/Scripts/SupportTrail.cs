using UnityEngine;
namespace Aether {
 public class SupportTrail:MonoBehaviour {Vehicle car;ParticleSystem particles;void Start(){car=GetComponentInParent<Vehicle>();particles=GetComponent<ParticleSystem>();}void Update(){var e=particles.emission;e.rateOverTime=car&&car.boost>0?42:0;}}
}
