Shader "Aether/RushSky" {
 Properties { _Top("Top",Color)=(.14,.22,.38,1) _Horizon("Horizon",Color)=(1,.49,.22,1) _Storm("Storm",Float)=0 }
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Top,_Horizon;float _Storm;
 struct v2f{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(float4 p:POSITION){v2f o;o.pos=UnityObjectToClipPos(p);o.dir=p.xyz;return o;}
 fixed4 frag(v2f i):SV_Target{float3 d=normalize(i.dir);float h=saturate(d.y);float3 c=lerp(_Horizon.rgb,_Top.rgb,pow(h,.42));float sun=dot(d,normalize(float3(.53,.12,.84)));float disc=smoothstep(.99965,.99985,sun);float halo=pow(saturate(sun),80)*.28;c+=(disc*float3(3,1.8,.65)+halo*float3(1,.5,.18))*(1-_Storm);float cloud=sin(d.x*22+d.z*8+sin(d.z*29+d.y*13)*.8+d.y*36);c=lerp(c,c*.63+_Horizon.rgb*.1,smoothstep(.5,1,cloud)*smoothstep(.02,.18,h)*(.35+_Storm*.4));return float4(c,1);}
 ENDCG }
 }
}
