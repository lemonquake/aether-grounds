using UnityEngine;
namespace Aether {
 public static class AirahTraffic {
  public const int Count=18;
  public static void Build(TrackWorld track){
   var system=track.gameObject.AddComponent<TownTrafficSystem>();
   Transform Make(Vector3 p,Quaternion q,int seed){
    var o=ModelLibrary.Create("AirahCivilian"+seed%2,track.transform);o.name="Airah civilian car "+seed;o.transform.SetPositionAndRotation(p,q);
    var c=o.AddComponent<BoxCollider>();c.center=new Vector3(0,.82f,0);c.size=new Vector3(1.85f,1.55f,4.1f);
    foreach(var r in o.GetComponentsInChildren<Renderer>())if(r.name=="Airah Car Paint")r.sharedMaterial=ModelLibrary.Material("Airah civilian paint "+seed%6,new[]{new Color(.67f,.29f,.12f),new Color(.76f,.77f,.67f),new Color(.19f,.35f,.44f),new Color(.32f,.40f,.23f),new Color(.61f,.16f,.12f),new Color(.37f,.37f,.40f)}[seed%6]);
    var prop=o.AddComponent<CityProp>();prop.Configure(CityPropKind.Metal,seed%2==0?240:300,false,26);prop.body.centerOfMass=new Vector3(0,.5f,0);return o.transform;
   }
   system.Initialize(track,Make);
   for(int i=0;i<Count;i++){int n=26+i*24;int sign=i%3==0?-1:1;float lane=sign==1?8.3f:-8.3f;var root=Make(track.points[n]+track.Right(n)*lane+Vector3.up*.15f,Quaternion.LookRotation(track.Forward(n)*sign),i);var driver=root.gameObject.AddComponent<TricycleTraffic>();driver.track=track;driver.point=n;driver.travelSign=sign;driver.lane=lane;system.Register(driver,i);}
  }
 }
}
