using UnityEngine;

namespace Aether {
    // All visible geometry is Blender-authored. Rotation, spring length, brake
    // heat and damper/control-arm poses come from each physical wheel's state.
    public sealed class WheelVisual:MonoBehaviour {
        Transform spinner,caliper,upperArm,lowerArm,spring,damper,upright,mountParent;
        Vector3 mount;float radius,heat,previousLoad;
        public float Radius=>radius;
        MaterialPropertyBlock properties;
        Renderer[] brakeRenderers;
        public static WheelVisual Build(Transform hub,int kind,Color color,float radius) {
            var visual=hub.GetComponent<WheelVisual>();if(!visual)visual=hub.gameObject.AddComponent<WheelVisual>();visual.radius=radius;
            var mesh=ModelLibrary.Create("WheelStyle"+kind,hub);mesh.name="Rotating wheel";mesh.transform.localScale=Vector3.one*(radius/.5f);visual.spinner=mesh.transform;
            foreach(var r in mesh.GetComponentsInChildren<Renderer>())if(r.name=="Wheel")r.sharedMaterial=ModelLibrary.Material("Wheel alloy "+color,color);
            visual.caliper=ModelLibrary.Create("BrakeCaliper",hub).transform;visual.caliper.localScale=Vector3.one*(radius/.5f);
            if(hub.name.EndsWith("L"))visual.caliper.localRotation=Quaternion.Euler(0,180,0);
            visual.upright=ModelLibrary.Create("WheelUpright",hub).transform;
            visual.brakeRenderers=System.Array.FindAll(mesh.GetComponentsInChildren<Renderer>(),r=>r.name=="BrakeRotor");
            visual.properties=new MaterialPropertyBlock();return visual;
        }
        public void AttachSuspension(Transform parent,Vector3 at) {
            mountParent=parent;mount=at;
            float side=at.x<0?-1:1;
            mount+=new Vector3(-side*.43f,.47f,0);
            if(spring)return;
            var root=new GameObject("Suspension linkage "+name).transform;root.SetParent(parent,false);
            spring=ModelLibrary.Create("SuspensionSpring",root).transform;
            damper=ModelLibrary.Create("SuspensionDamper",root).transform;
            upperArm=ModelLibrary.Create("SuspensionArm",root).transform;
            lowerArm=ModelLibrary.Create("SuspensionArm",root).transform;
        }
        static void Span(Transform part,Vector3 from,Vector3 to) {
            part.position=from;part.rotation=Quaternion.FromToRotation(Vector3.up,to-from);part.localScale=new Vector3(1,Vector3.Distance(from,to),1);
        }
        public void SetPhysicalPose(Vector3 position,Quaternion rollingRotation,Quaternion steeringRotation,float load,float brakes,float rpm,bool active) {
            transform.SetPositionAndRotation(position,steeringRotation);spinner.rotation=rollingRotation*(name.EndsWith("L")?Quaternion.Euler(0,180,0):Quaternion.identity);
            if(mountParent&&spring) {
                Vector3 top=mountParent.TransformPoint(mount);
                Vector3 inner=position-transform.right*Mathf.Sign(mount.x)*.13f;
                Span(spring,inner,top);Span(damper,inner,Vector3.Lerp(inner,top,.7f));
                Span(upperArm,top-mountParent.up*.12f,inner+transform.up*.10f);
                Span(lowerArm,top-mountParent.up*.42f,inner-transform.up*.12f);
            }
            if(active)heat=Mathf.Clamp01(heat+Time.deltaTime*(brakes>700&&rpm>150?.20f:-.10f));
            properties.SetColor("_Color",Color.Lerp(new Color(.30f,.33f,.36f),new Color(.95f,.22f,.045f),heat));properties.SetFloat("_Emission",heat*1.1f);
            foreach(var renderer in brakeRenderers)renderer.SetPropertyBlock(properties);
            if(active&&load>18000&&previousLoad<3000){RaceEffects.Burst(position-transform.up*radius,new Color(.65f,.61f,.53f,.4f),8,1.8f,.18f,.45f);}
            previousLoad=load;
        }
    }
}
