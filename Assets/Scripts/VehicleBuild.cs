using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aether {
    [Serializable]
    public sealed class PartWeight {
        public string name;
        public float kilograms;
        public Vector3 position;
        public PartWeight(string label, float kg, Vector3 at) { name=label; kilograms=kg; position=at; }
    }

    // Authored game specifications in SI units, shared by the garage and runtime.
    // These are fictional parts, not claims about real manufacturers' products.
    public sealed class VehicleBuild {
        public static readonly string[] ChassisNames={"Standard chassis","Light alloy chassis","Reinforced chassis","Tubular chassis"};
        public static readonly string[] SpringNames={"Road springs","Soft springs","Firm springs"};
        public static readonly string[] DamperNames={"Balanced rebound","Lively rebound","Settled rebound"};
        public static readonly float[] WheelMass={19.8f,20.6f,22.4f,18.7f,23.3f,28.6f,19.1f,25.2f,21.5f,20.1f};
        public static readonly float[] WheelRadiusScale={1f,1.01f,.99f,.98f,1.02f,1.07f,1f,1.015f,1.005f,.995f};
        public static readonly float[] BodyRadius={.44f,.46f,.47f,.59f,.45f,.55f,.43f,.49f,.49f,.46f};
        public static readonly float[] StockPower={240,260,180,205,350,230,170,290,165,330};
        public static readonly float[] StockEngineMass={205,240,148,170,180,215,125,225,185,195};
        public static readonly float[] EnginePower={0,190,245,315,255,280,370,265,380,410,430};
        public static readonly float[] EngineMass={0,142,186,252,178,132,205,335,224,292,278};
        public static readonly float[] WingMass={8,3.2f,7.4f,11.5f,12.8f,9.3f,8.1f,10.4f,11.7f,6.9f,13.6f};
        public static readonly float[] ExhaustMass={15,8.2f,14.6f,23.1f,16.8f,10.3f,19.7f,26.4f,17.5f,21.9f,12.8f};
        public readonly List<PartWeight> parts=new List<PartWeight>();
        public float mass, powerKW, peakTorque, wheelMass, radius, dragArea, rollingResistance;
        public float springRate, damperRate, travel, tireGrip, brakeTorque, redline, finalDrive;
        public float estimatedTopSpeed, estimatedAcceleration, frontWeight;
        public Vector3 centerOfMass;

        void Add(string name,float kg,Vector3 at) { parts.Add(new PartWeight(name,kg,at));mass+=kg;centerOfMass+=at*kg; }
        public static VehicleBuild Calculate(SavedCar car,RushSave gear=null,bool store=true) {
            car.Validate();var baseSpec=CarSpec.All[car.body];var b=new VehicleBuild();
            float baseEngine=StockEngineMass[car.body], panels=baseSpec.mass*.23f;
            float chassis=baseSpec.mass-panels-baseEngine-4*WheelMass[0]-8-15-75-35;
            float chassisFactor=new[]{1f,.82f,1.18f,.90f}[car.chassis];
            b.Add(CarSpec.All[car.body].name+" body panels",panels,new Vector3(0,.73f,0));
            b.Add(ChassisNames[car.chassis],chassis*chassisFactor,new Vector3(0,.28f,0));
            b.Add("Driver",75,new Vector3(-.28f,.78f,-.15f));
            b.Add("Fuel, cooling and electrics",35,new Vector3(.15f,.28f,-.35f));
            bool rearEngine=car.body==4||car.body==8||car.body==9;
            b.Add(CarParts.Engines[car.engine]+" powertrain",car.engine==0?baseEngine:EngineMass[car.engine],new Vector3(0,.44f,rearEngine?-1.05f:1.05f));
            b.wheelMass=WheelMass[car.wheel];b.radius=BodyRadius[car.body]*WheelRadiusScale[car.wheel];
            for(int i=0;i<4;i++)b.Add(CarParts.Wheels[car.wheel]+" · "+new[]{"front left","front right","rear left","rear right"}[i],b.wheelMass,new Vector3(i%2==0?-1:1,b.radius,i<2?1.35f:-1.35f));
            b.Add(CarParts.Spoilers[car.spoiler]+" wing",WingMass[car.spoiler],new Vector3(0,1.2f,-1.8f));
            b.Add(CarParts.Exhausts[car.exhaust]+" exhaust",ExhaustMass[car.exhaust],new Vector3(0,.4f,-1.7f));
            if(car.livery>0)b.Add(CarParts.Liveries[car.livery]+" wrap",.18f+car.livery*.037f,new Vector3(0,.75f,0));
            // Tint/paint variants are coatings, so their mass is small, not zero.
            b.Add(CarParts.GlassNames[car.glass]+" window film",.20f+car.glass*.013f,new Vector3(0,1.15f,0));
            b.Add(PaintStyles.Names[car.paintStyle]+" finish",1.6f+car.paintStyle*.19f,new Vector3(0,.7f,0));
            b.Add("Engine upgrade hardware",car.engineLevel*.85f,new Vector3(0,.44f,rearEngine?-1:1));
            b.Add("Tire reinforcement",car.tireLevel*.16f,new Vector3(0,b.radius,0));
            b.Add("Brake upgrade hardware",car.brakeLevel*.34f,new Vector3(0,b.radius,0));
            b.Add("Suspension upgrade hardware",car.suspensionLevel*.22f,new Vector3(0,.52f,0));
            if(gear!=null){
                if(gear.engine=="sun-turbo")b.Add("Solar Turbo",18.4f,new Vector3(0,1.12f,.85f));
                if(gear.engine=="storm-coil")b.Add("Storm Coil",24.7f,new Vector3(0,1.14f,-.82f));
                if(gear.chassis=="rock-plate")b.Add("Redrock Plating",78.5f,new Vector3(0,.55f,0));
                if(gear.chassis=="grip-kit")b.Add("Road Grip Kit",5.6f,new Vector3(0,b.radius,0));
            }
            if(store){
                if(car.neonWheels&&SupportStore.Owned("neon-wheels"))b.Add("Neon wheel hardware",3.2f,new Vector3(0,b.radius,0));
                if(car.glassWheels&&SupportStore.Owned("glass-wheels"))b.Add("Glass wheel reinforcement",12.8f,new Vector3(0,b.radius,0));
                if(car.glassChassis&&SupportStore.Owned("glass-chassis"))b.Add("Glass panels, frame and battery",66.2f,new Vector3(0,.45f,0));
                if(car.starTrail&&SupportStore.Owned("star-trail"))b.Add("Starlight emitter",4.1f,new Vector3(0,.4f,-1.8f));
                if(!string.IsNullOrEmpty(car.supportEdition)&&SupportStore.Owned(car.supportEdition))b.Add("Edition hardware",9.7f+Array.FindIndex(SupportStore.Products,p=>p.id==car.supportEdition)*1.13f,new Vector3(0,.5f,0));
            }
            b.centerOfMass/=b.mass;b.frontWeight=Mathf.Clamp(.5f+b.centerOfMass.z/2.7f,.36f,.64f);
            var bonus=store?SupportStore.Bonus(car):Vector4.zero;
            b.powerKW=(car.engine==0?StockPower[car.body]:EnginePower[car.engine])*(1+car.engineLevel*.025f);
            b.powerKW*=Mathf.Max(1+bonus.y/baseSpec.accel,Mathf.Pow((baseSpec.speed+bonus.x)/baseSpec.speed,3));
            if(gear!=null&&gear.engine=="sun-turbo")b.powerKW*=1.08f;
            b.redline=car.engine==7?10500:car.engine==5?8500:car.engine==10?7400:6800;
            b.peakTorque=b.powerKW*1000/(b.redline*.74f*Mathf.PI/30)*1.12f;
            b.finalDrive=b.redline*Mathf.PI/30*BodyRadius[car.body]/((baseSpec.speed+bonus.x)*.75f);
            b.dragArea=new[]{.86f,1.0f,.79f,1.12f,.82f,1.30f,.76f,.99f,1.35f,.74f}[car.body]+car.spoiler*.007f;
            b.rollingResistance=.011f+(car.wheel==5?.005f:car.wheel==4?.002f:0);
            float frequency=(car.body==3||car.body==5?1.7f:1.95f)+car.suspensionLevel*.025f;
            frequency*=new[]{1f,.82f,1.2f}[car.springTune];
            b.travel=(car.body==3||car.body==5?.46f:.36f)*new[]{1f,1.18f,.88f}[car.springTune];
            b.springRate=Mathf.Pow(2*Mathf.PI*frequency,2)*(b.mass/4);
            float damping=(.52f+car.suspensionLevel*.012f)*new[]{1f,.65f,1.3f}[car.damperTune];
            b.damperRate=2*Mathf.Sqrt(b.springRate*b.mass/4)*damping;
            b.tireGrip=Mathf.Clamp(1.15f+(baseSpec.grip-8.3f)*.08f+car.tireLevel*.016f+bonus.w*.06f+(gear!=null&&gear.chassis=="grip-kit"?.045f:0),1.05f,1.85f);
            b.brakeTorque=1550+car.brakeLevel*65+bonus.z*60;
            b.estimatedTopSpeed=b.EstimateTopSpeed();
            b.estimatedAcceleration=Mathf.Min(b.tireGrip*9.81f,b.peakTorque*3.2f*b.finalDrive*.88f/b.radius/(b.mass+4*b.wheelMass*.5f));
            return b;
        }
        public float Resistance(float metersPerSecond)=>.5f*1.225f*dragArea*metersPerSecond*metersPerSecond+rollingResistance*mass*9.81f;
        public float EstimateTopSpeed(){
            float gearing=redline*Mathf.PI/30*radius/(.75f*finalDrive),lo=0,hi=gearing;
            for(int i=0;i<40;i++){float mid=(lo+hi)*.5f;if(Resistance(mid)*mid<powerKW*880)lo=mid;else hi=mid;}
            return (lo+hi)*.5f;
        }
    }
}
