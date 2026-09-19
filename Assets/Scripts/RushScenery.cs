using System.Collections.Generic;
using UnityEngine;
namespace Aether {
 public partial class TrackWorld {
  void RushIsland(Transform parent,Vector3 center,float rx,float rz,float height,int seed,bool canyon){
   const int rings=18,sides=48;var vs=new Vector3[(rings+1)*(sides+1)];var ts=new List<int>();
   for(int j=0;j<=rings;j++)for(int i=0;i<=sides;i++){float a=i/(float)sides*Mathf.PI*2,t=j/(float)rings;float rough=.82f+.12f*Mathf.Sin(a*5+seed)+.09f*Mathf.Sin(a*9-seed*.3f)+.055f*Mathf.Cos(a*17+seed);float radius=Mathf.Pow(1-t,.55f)*rough;float jag=1+.055f*Mathf.Sin(t*53+a*5)+.04f*Mathf.Sin(t*27+a*7);vs[j*(sides+1)+i]=center+new Vector3(Mathf.Cos(a)*rx*radius*jag,height*t+Mathf.Sin(a*3+seed)*height*.06f*Mathf.Sin(t*Mathf.PI),Mathf.Sin(a)*rz*radius*jag);if(j<rings&&i<sides){int n=j*(sides+1)+i;ts.AddRange(new[]{n,n+sides+1,n+1,n+1,n+sides+1,n+sides+2});}}
   var mat=ModelLibrary.Material(canyon?"Rush canyon stone":"Rush island stone",canyon?new Color(.65f,.37f,.23f):new Color(.38f,.42f,.38f));mat.SetFloat("_Surface",2);mat.SetFloat("_Smoothness",.12f);MeshObject(canyon?"Layered canyon cliff":"Coastal island",vs,ts.ToArray(),mat,false,parent);
  }
  void RushBeach(Transform parent,float z){var mat=ModelLibrary.Material("Rush sand dunes",new Color(.62f,.52f,.34f));mat.SetFloat("_Surface",2);mat.SetFloat("_Smoothness",.08f);var vs=new Vector3[17*9];var ts=new List<int>();for(int j=0;j<9;j++)for(int i=0;i<17;i++){float x=-220+i*12.5f,zz=z+j*15;float y=3.3f+(1-Mathf.InverseLerp(-200,-20,x))*(Mathf.Sin(x*.039f+zz*.013f)*1.4f+Mathf.Sin(zz*.04f)*.55f);vs[j*17+i]=new Vector3(x,y,zz);if(j<8&&i<16){int n=j*17+i;ts.AddRange(new[]{n,n+17,n+1,n+1,n+17,n+18});}}MeshObject("Beach dunes",vs,ts.ToArray(),mat,false,parent);}
 }
}
