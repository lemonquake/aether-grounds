using UnityEngine;
namespace Aether {
 public partial class Game {
  void CircuitCards(float y,float height){
   string[] descriptions={"City traffic · coastal roads","Davao village · night race","Forest turns · night gardens","Mountain forest · sunny afternoon"};
   for(int i=0;i<TrackWorld.Names.Length;i++){float x=34+i*390;Panel(x,y,378,height,.95f);DrawRoute(i,new Rect(x+15,y+18,90,90));Label(TrackWorld.Names[i],x+118,y+17,244,65,25,mapIndex==i?cyan:paper,true);Label(descriptions[i],x+118,y+82,240,52,19,muted);if(Button(mapIndex==i?"Selected":"Select circuit",x+17,y+height-57,344,42,mapIndex==i))mapIndex=i;}
  }
 }
}
