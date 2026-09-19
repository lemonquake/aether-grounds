using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class Vehicle {
  public const float FlipSupportDelay=1f;
  public float flipSupportedSeconds{get;private set;}public int flipRecoveries{get;private set;}
  readonly HashSet<Collider> flipSupports=new HashSet<Collider>();readonly ContactPoint[] flipContacts=new ContactPoint[16];
  void RecordFlipContact(Collision collision){
   bool supported=false;int count=collision.GetContacts(flipContacts);
   for(int i=0;i<count;i++)if(Vector3.Dot(flipContacts[i].normal,Vector3.up)>.35f){supported=true;break;}
   if(supported)flipSupports.Add(collision.collider);else flipSupports.Remove(collision.collider);
  }
  void OnCollisionStay(Collision collision){RecordFlipContact(collision);}
  void OnCollisionExit(Collision collision){flipSupports.Remove(collision.collider);}
  void ResetFlipRecovery(){flipSupports.Clear();flipSupportedSeconds=0;righting=0;}
  void UpdateFlipRecovery(bool active){
   if(!active)return;
   flipSupports.RemoveWhere(c=>!c||!c.enabled||!c.gameObject.activeInHierarchy);
   bool supported=flipSupports.Count>0;
   if(!supported||rolloverLock>0||frozenFor>0){flipSupportedSeconds=0;righting=0;return;}
   float upright=Vector3.Dot(transform.up,Vector3.up);
   if(upright>=.65f){flipSupportedSeconds=0;righting=0;return;}
   flipSupportedSeconds+=Time.fixedDeltaTime;
   if(flipSupportedSeconds<FlipSupportDelay)return;
   flipRecoveries++;
   var heading=Vector3.ProjectOnPlane(transform.forward,Vector3.up);if(heading.sqrMagnitude<.1f)heading=track.Forward(lastPoint);
   // Commit the correction while contact is present. Continuing a rotation over
   // subsequent airborne frames would violate the support requirement.
   var rotation=Quaternion.LookRotation(heading);body.rotation=rotation;transform.rotation=rotation;body.angularVelocity=Vector3.zero;flipSupportedSeconds=0;righting=0;rollGrace=.25f;flipSupports.Clear();
  }
 }
}
