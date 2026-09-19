using UnityEngine;
namespace Aether {
 public partial class TrackWorld {
  void AirahBrokenBridge(){
   foreach(int n in new[]{29,31,40}){AddTile(n,TileKind.Booster);var tile=tiles[tiles.Count-1];tile.halfWidth=4;tile.transform.localScale=new Vector3(2.7f,1,1);tile.minForwardSpeed=46;}
   AddTile(AirahGapStart,TileKind.Jump);var launch=tiles[tiles.Count-1];launch.transform.position=points[AirahGapStart]-Forward(AirahGapStart)*3+Vector3.up*.06f;launch.length=5;launch.halfWidth=4;launch.transform.localScale=new Vector3(2.7f,1,.4f);launch.launchForce=16.5f;launch.minForwardSpeed=46;launch.maxForwardSpeed=57;
   Sign(26,"Broken bridge - use boosters",new Color(1,.73f,.28f));Sign(39,"Landing deck",new Color(.58f,1,.72f));
   var root=DetailSector("Broken bridge - two decks");var steel=AirahMat("fractured bridge steel",new Color(.34f,.20f,.12f));var concrete=AirahMat("broken deck stone",new Color(.48f,.47f,.40f));
   foreach(int n in new[]{AirahGapStart,AirahGapEnd}){
    var p=points[n];var forward=Forward(n);var right=Right(n);
    for(int side=-1;side<=1;side+=2){
     DetailBeam(root,"Broken truss end",p+right*side*12-Vector3.up*8,p+right*side*12+forward*(n==AirahGapStart?4:-4)-Vector3.up*2,.5f,steel);
     for(int j=0;j<5;j++){var o=Cube("Fractured bridge edge",p+right*side*(7+j)-Vector3.up*R(.4f,1.2f),new Vector3(R(.7f,1.6f),R(.4f,1.3f),R(1,3)),concrete,false);o.transform.SetParent(root);o.transform.rotation=Quaternion.LookRotation(forward)*Quaternion.Euler(R(-15,15),R(-12,12),R(-12,12));}
    }
   }
   // Collapsed pieces remain far below the open flight path.
   for(int i=0;i<12;i++){var p=Vector3.Lerp(points[AirahGapStart],points[AirahGapEnd],i/11f)+Right(35)*R(-16,16);p.y=AirahHeight(p.x,p.z)+1;var o=ModelLibrary.Create("AirahRock"+i%4,root);o.transform.position=p;o.transform.localScale=new Vector3(2,1,2);}
   AirahCombine(root);
  }
 }
}
