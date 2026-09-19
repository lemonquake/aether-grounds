using System;
using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public class TownTrafficSystem:MonoBehaviour {
  class Entry {public TricycleTraffic car;public int point,seed,sign;public float lane,timer,stationary;public Vector3 last;}
  readonly List<Entry> entries=new List<Entry>();TrackWorld track;Game game;Func<Vector3,Quaternion,int,Transform> factory;
  public int RespawnCount{get;private set;}
  public void Initialize(TrackWorld world,Func<Vector3,Quaternion,int,Transform> create){track=world;factory=create;}
  public void Register(TricycleTraffic car,int seed){entries.Add(new Entry{car=car,point=car.point,seed=seed,sign=car.travelSign,lane=car.lane,last=car.transform.position});}
  void Update(){
   if(!game)game=FindAnyObjectByType<Game>();if(!game||!game.RaceSimulationActive)return;
   foreach(var entry in entries){
    var car=entry.car;bool lost=!car;
    if(car){var p=car.transform.position;if(TrackWorld.FlatDistance(p,entry.last)>1){entry.last=p;entry.stationary=0;}else entry.stationary+=Time.deltaTime;var prop=car.GetComponent<CityProp>();lost=prop.broken||p.y<0||TrackWorld.FlatDistance(p,track.points[track.Nearest(p)])>(track.map==1?8:17)||entry.stationary>8||Vector3.Dot(car.transform.up,Vector3.up)<.35f;}
    if(!lost){entry.timer=0;continue;}
    entry.timer+=Time.deltaTime;if(entry.timer<20)continue;
    bool placed=false;
    for(int offset=0;offset<36;offset+=3){
     int n=(entry.point+offset)%TrackWorld.Count;Vector3 p=track.points[n]+track.Right(n)*entry.lane+Vector3.up*.16f;Quaternion q=Quaternion.LookRotation(track.Forward(n)*entry.sign);
     if(Physics.CheckBox(p+Vector3.up*.85f,new Vector3(1.1f,.6f,1.6f),q,(1<<8)|(1<<10)|1,QueryTriggerInteraction.Ignore))continue;
     if(car){car.gameObject.SetActive(false);Destroy(car.gameObject);}
     var root=factory(p,q,entry.seed);var next=root.gameObject.AddComponent<TricycleTraffic>();next.track=track;next.point=n;next.lane=entry.lane;next.travelSign=entry.sign;entry.car=next;entry.timer=entry.stationary=0;entry.last=p;RespawnCount++;placed=true;break;
    }
    if(!placed)entry.timer=20; // Wait for a free lane instead of appearing inside a racer.
   }
  }
 }
}
