using UnityEngine;

namespace Aether {
    public partial class Vehicle {
        public VehicleBuild build;
        public SavedCar buildCar;
        public WheelCollider[] physicalWheels=new WheelCollider[4];
        public float engineRPM, enginePowerKW, suspensionTravel, peakWheelLoad;
        public int gear=1;
        float shiftTimer, lastSuspensionMass;
        readonly float[] gearRatios={3.2f,2.15f,1.57f,1.21f,.95f,.75f};
        readonly float[] wheelLoads=new float[4];
        readonly float[] wheelCompressions=new float[4];
        WheelVisual[] wheelVisuals=new WheelVisual[4];

        public void ConfigureBuild(SavedCar configuration) {
            buildCar=JsonUtility.FromJson<SavedCar>(JsonUtility.ToJson(configuration));
            build=VehicleBuild.Calculate(buildCar,player?game.inventory:null,player);
            radius=build.radius;
            body.mass=build.mass;
            body.centerOfMass=build.centerOfMass;
            body.linearDamping=0;
            body.angularDamping=.8f;
            enginePowerKW=build.powerKW;
            for(int i=0;i<4;i++) {
                wheels[i]=model.transform.Find(new[]{"WheelFL","WheelFR","WheelRL","WheelRR"}[i]);
                if(!wheels[i])continue;
                // Hubs are model-space mounts. Never derive radius from their Y
                // position: suspension motion and body designs can change it.
                hubs[i]=wheels[i].localPosition;
                wheelVisuals[i]=wheels[i].GetComponent<WheelVisual>();
                if(!physicalWheels[i]) {
                    var o=new GameObject("Physical "+wheels[i].name);
                    o.transform.SetParent(transform,false);o.layer=8;
                    physicalWheels[i]=o.AddComponent<WheelCollider>();
                }
                var wheel=physicalWheels[i];
                wheel.transform.localPosition=hubs[i]+Vector3.up*build.travel*.5f;
                wheel.radius=build.radius;wheel.mass=build.wheelMass;
                wheel.suspensionDistance=build.travel;
                wheel.forceAppPointDistance=.16f;
                wheel.wheelDampingRate=.45f;
                wheel.ConfigureVehicleSubsteps(12,5,3);
                var forward=wheel.forwardFriction;forward.extremumSlip=.35f;forward.extremumValue=1;
                forward.asymptoteSlip=.8f;forward.asymptoteValue=.72f;forward.stiffness=build.tireGrip;wheel.forwardFriction=forward;
                var side=wheel.sidewaysFriction;side.extremumSlip=.22f;side.extremumValue=1;
                side.asymptoteSlip=.6f;side.asymptoteValue=.72f;side.stiffness=build.tireGrip;wheel.sidewaysFriction=side;
                if(wheelVisuals[i])wheelVisuals[i].AttachSuspension(model.transform,hubs[i]);
            }
            RefreshWheelSprings();
            body.ResetInertiaTensor();
            // PhysX recomputes the box's tensor with the actual assembled mass.
            body.centerOfMass=build.centerOfMass;
        }

        void RefreshWheelSprings() {
            if(build==null)return;
            lastSuspensionMass=body.mass;
            for(int i=0;i<4;i++) {
                var wheel=physicalWheels[i];if(!wheel)continue;
                float share=(i<2?build.frontWeight:1-build.frontWeight)*.5f;
                wheel.sprungMass=body.mass*share;
                wheel.suspensionSpring=new JointSpring {
                    spring=build.springRate*share*4*(body.mass/build.mass),
                    damper=build.damperRate*share*4*(body.mass/build.mass),targetPosition=.5f
                };
            }
        }

        void UpdateWheelPhysics(bool active) {
            if(build==null)return;
            if(Mathf.Abs(lastSuspensionMass-body.mass)>.1f)RefreshWheelSprings();
            float dt=Time.fixedDeltaTime;
            shiftTimer=Mathf.Max(0,shiftTimer-dt);
            float longitudinal=Vector3.Dot(body.linearVelocity,body.rotation*Vector3.forward);
            float rpm=0;for(int i=0;i<4;i++)if(physicalWheels[i])rpm+=Mathf.Abs(physicalWheels[i].rpm)*.25f;
            float ratio=gearRatios[gear-1]*build.finalDrive;
            engineRPM=Mathf.Max(900,rpm*ratio);
            float limit=build.redline*(boost>0?1.14f:1);
            if(active&&shiftTimer<=0) {
                if(engineRPM>limit*.9f&&gear<6){gear++;shiftTimer=.18f;}
                else if(engineRPM<limit*.36f&&gear>1){gear--;shiftTimer=.12f;}
            }
            ratio=gearRatios[gear-1]*build.finalDrive;
            engineRPM=Mathf.Max(900,rpm*ratio);
            float torque=Mathf.Min(build.peakTorque,build.powerKW*1000/Mathf.Max(100,engineRPM*Mathf.PI/30));
            torque*=Mathf.Lerp(.72f,1,Mathf.Clamp01(engineRPM/(build.redline*.42f)));
            torque*=boost>0?1.6f:1;
            if(engineRPM>limit||!active||frozenFor>0)torque=0;
            if(shiftTimer>0)torque*=.35f;
            if(stun>0)torque*=.15f;
            bool reverse=throttle<0&&longitudinal<1.2f;
            if(reverse){gear=1;ratio=-3.2f*build.finalDrive;torque*=Mathf.Clamp01((14+longitudinal)/3);}
            float drive=active?Mathf.Max(0,reverse?-throttle:throttle)*torque*ratio*.88f:0;
            bool braking=active&&throttle<0&&!reverse;
            float steerLimit=Mathf.Lerp(31,9,Mathf.Clamp01(Mathf.Abs(longitudinal)/65));
            steerSmooth=Mathf.MoveTowards(steerSmooth,active?steer:0,dt*3);
            grounded=0;suspensionTravel=0;peakWheelLoad=0;
            for(int i=0;i<4;i++) {
                var wheel=physicalWheels[i];if(!wheel)continue;
                wheel.steerAngle=i<2?steerSmooth*steerLimit:0;
                // A 45/55 drive split helps mixed road surfaces; torque is finite
                // and is never multiplied by chassis mass to cancel added weight.
                wheel.motorTorque=drive*(i<2?.225f:.275f);
                // PhysX's low-speed brake constraint can hold a stopped wheel
                // even with engine torque present. Rolling resistance is a road
                // force below, never a permanently engaged service brake.
                wheel.brakeTorque=!active||frozenFor>0?build.brakeTorque*1.5f:braking?-throttle*build.brakeTorque*(i<2?1.2f:.8f):0;
                // Drift changes rear lateral grip below. A constant rear brake
                // also locks launches when Space is held, so it is not used as
                // a substitute for tire slip.
                var friction=wheel.sidewaysFriction;
                friction.stiffness=build.tireGrip*(oilFor>0?.2f:wet?.67f:1)*(drift&&i>=2?.55f:1);
                wheel.sidewaysFriction=friction;
                var traction=wheel.forwardFriction;traction.stiffness=build.tireGrip*(oilFor>0?.35f:wet?.7f:1);wheel.forwardFriction=traction;
                bool contact=wheel.GetGroundHit(out WheelHit hit)&&launchGrace<=0;
                if(contact) {
                    grounded++;wheelLoads[i]=hit.force;peakWheelLoad=Mathf.Max(peakWheelLoad,hit.force);
                    wheel.GetWorldPose(out Vector3 position,out _);
                    float extension=Vector3.Dot(wheel.transform.position-position,transform.up);
                    wheelCompressions[i]=Mathf.Clamp01(1-extension/build.travel);
                    suspensionTravel=Mathf.Max(suspensionTravel,wheelCompressions[i]);
                    if(i>=2){trails[i-2].transform.position=hit.point+hit.normal*.025f;trails[i-2].emitting=active&&speed>20&&(Mathf.Abs(hit.sidewaysSlip)>.22f||Mathf.Abs(hit.forwardSlip)>.35f);}
                }else {wheelLoads[i]=0;wheelCompressions[i]=0;if(i>=2)trails[i-2].emitting=false;}
                // A launch pad is an explicit game impulse; unload tire torque
                // briefly so a still-overlapping ray cannot pin a jump to the ramp.
                if(launchGrace>0)wheel.motorTorque=wheel.brakeTorque=0;
            }
            if(active) {
                Vector3 velocity=body.linearVelocity;
                body.AddForce(-velocity*velocity.magnitude*(.5f*1.225f*build.dragArea),ForceMode.Force);
                if(grounded>0&&velocity.sqrMagnitude>.0001f)body.AddForce(-velocity.normalized*Mathf.Min(build.rollingResistance*body.mass*9.81f,body.mass*velocity.magnitude/dt),ForceMode.Force);
                if(grounded>=2) {
                    float downforce=.5f*1.225f*(.14f+buildCar.spoiler*.026f)*longitudinal*longitudinal;
                    body.AddForce(-transform.up*downforce,ForceMode.Force);
                    AntiRoll(0,1);AntiRoll(2,3);
                }
            }
        }

        void AntiRoll(int left,int right) {
            float force=(wheelCompressions[left]-wheelCompressions[right])*build.springRate*.075f;
            if(wheelLoads[left]>0)body.AddForceAtPosition(-transform.up*force,physicalWheels[left].transform.position);
            if(wheelLoads[right]>0)body.AddForceAtPosition(transform.up*force,physicalWheels[right].transform.position);
        }

        void LateUpdate() {
            if(build==null||body==null)return;
            for(int i=0;i<4;i++)if(physicalWheels[i]&&wheelVisuals[i]) {
                var wheel=physicalWheels[i];wheel.GetWorldPose(out Vector3 p,out Quaternion q);
                wheelVisuals[i].SetPhysicalPose(p,q,body.rotation*Quaternion.Euler(0,wheel.steerAngle,0),wheelLoads[i],wheel.brakeTorque,Mathf.Abs(wheel.rpm),game.RaceSimulationActive);
            }
        }
    }
}
