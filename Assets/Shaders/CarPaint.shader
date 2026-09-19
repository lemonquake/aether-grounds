Shader "Aether/Paint" {
 Properties { _Color("Body color",Color)=(1,1,1,1) _MainTex("Generated paint texture",2D)="white"{} _Scale("Pattern scale",Float)=1 _Style("Style",Float)=1 _Metallic("Metallic",Range(0,1))=.6 _Smoothness("Clear coat",Range(0,1))=.9 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
 #pragma target 3.0
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 sampler2D _MainTex; fixed4 _Color; float _Scale,_Style; half _Metallic,_Smoothness;
 struct Input {float3 localPos;float3 localNormal;float3 viewDir;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;o.localNormal=v.normal;}
 // Object-space triplanar wrapping follows moving cars; no UV seams or sliding world texture.
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.localPos*_Scale;float3 w=pow(abs(normalize(IN.localNormal)),6);w/=max(.001,w.x+w.y+w.z);
  float3 tex=tex2D(_MainTex,p.yz).rgb*w.x+tex2D(_MainTex,p.xz).rgb*w.y+tex2D(_MainTex,p.xy).rgb*w.z;
  float l=dot(tex,float3(.299,.587,.114));float fres=pow(1-saturate(dot(normalize(IN.viewDir),float3(0,0,1))),3);
  o.Albedo=_Color.rgb*lerp(.28,1.5,l);o.Metallic=_Metallic;o.Smoothness=saturate(_Smoothness+(l-.5)*.12);
  if(_Style==2){float sparkle=smoothstep(.68,.91,l);o.Smoothness=lerp(.65,1,sparkle);o.Emission=sparkle*fres*_Color.rgb*.5;}
  if(_Style==3){o.Albedo=lerp(_Color.rgb*.35,_Color.rgb*1.4,saturate(l+fres*.4));o.Emission=_Color.rgb*fres*.17;}
  if(_Style==5||_Style==10){float3 pearl=.5+.5*cos(fres*7+float3(0,2,4));o.Albedo=lerp(o.Albedo,pearl*_Color.rgb*1.5,fres*.45);}
  if(_Style==8)o.Emission=_Color.rgb*pow(l,4)*.65;
  o.Occlusion=1;o.Alpha=1;
 }
 ENDCG
 } Fallback "Standard"
}
