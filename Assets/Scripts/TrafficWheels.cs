using System.Collections.Generic;
using UnityEngine;

namespace Aether {
    public sealed class TrafficWheelSetup:MonoBehaviour {
        public int bodyIndex;
        void Start(){
            var body=GetComponentInParent<Rigidbody>();
            if(!body||body.GetComponent<Vehicle>()||body.GetComponent<NpcWheelPhysics>())return;
            var rig=body.gameObject.AddComponent<NpcWheelPhysics>();
            rig.templateBody=bodyIndex;
        }
    }

    public sealed class NpcWheelPhysics:MonoBehaviour {
        public int templateBody=2;
        public float engineForce=6000;
        public float Mass=>body?body.mass:0;
        public int Grounded {get;private set;}
        readonly List<WheelCollider> colliders=new List<WheelCollider>();
        readonly List<WheelVisual> visuals=new List<WheelVisual>();
        Rigidbody body;Game game;
        void Start(){
            body=GetComponent<Rigidbody>();game=FindAnyObjectByType<Game>();if(!body)return;
            bool tricycle=name.Contains("tricycle"),jeepney=name.Contains("jeepney");
            body.mass=tricycle?350:jeepney?2800:VehicleBuild.Calculate(new SavedCar{body=templateBody},null,false).mass;
            engineForce=tricycle?1600:jeepney?9200:6000;
            var mounts=GetComponentsInChildren<WheelVisual>();
            foreach(var visual in mounts){
                var o=new GameObject("Traffic suspension "+visual.name);o.transform.SetParent(transform,false);o.layer=10;
                Vector3 at=transform.InverseTransformPoint(visual.transform.position);
                o.transform.localPosition=at+Vector3.up*.14f;
                var wheel=o.AddComponent<WheelCollider>();wheel.radius=visual.Radius*visual.transform.lossyScale.y;
                wheel.mass=tricycle?9:jeepney?38:20;wheel.suspensionDistance=.28f;
                float share=body.mass/Mathf.Max(1,mounts.Length);
                wheel.sprungMass=share;wheel.suspensionSpring=new JointSpring{spring=share*145,damper=share*13,targetPosition=.5f};
                wheel.forceAppPointDistance=.12f;wheel.ConfigureVehicleSubsteps(10,3,2);
                var f=wheel.sidewaysFriction;f.stiffness=1.1f;wheel.sidewaysFriction=f;
                colliders.Add(wheel);visuals.Add(visual);visual.AttachSuspension(transform,at);
            }
            body.WakeUp();
        }
        public void DriveToward(Vector3 desiredVelocity,float response=3){
            if(!body)return;
            var current=Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up);
            body.AddForce(Vector3.ClampMagnitude((desiredVelocity-current)*body.mass*response,engineForce),ForceMode.Force);
        }
        void FixedUpdate(){
            Grounded=0;
            foreach(var wheel in colliders){
                wheel.brakeTorque=game&&game.RaceSimulationActive?0:800;
                if(wheel.GetGroundHit(out _))Grounded++;
            }
        }
        void LateUpdate(){
            if(!body)return;
            for(int i=0;i<colliders.Count;i++){
                var wheel=colliders[i];wheel.GetWorldPose(out var p,out var q);wheel.GetGroundHit(out var hit);
                visuals[i].SetPhysicalPose(p,q,body.rotation,hit.force,wheel.brakeTorque,Mathf.Abs(wheel.rpm),game&&game.RaceSimulationActive);
            }
        }
    }
}
