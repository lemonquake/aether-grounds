Shader "Aether/Sky" {
 Properties { _Night("Night",Float)=0  _Top("Top",Color)=(.12,.13,.38,1) _Horizon("Horizon",Color)=(1,.53,.48,1) }
 SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Top,_Horizon;float _Night;
 struct v2f{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(float4 p:POSITION){v2f o;o.pos=UnityObjectToClipPos(p);o.dir=p.xyz;return o;}
 fixed4 frag(v2f i):SV_Target{float3 d=normalize(i.dir);float h=saturate(d.y);float3 c=lerp(_Horizon.rgb,_Top.rgb,pow(h,.55));float cloud=sin(d.x*18+d.z*7+sin(d.z*19)*.7+d.y*12);c+=smoothstep(.82,1,cloud)*.055*smoothstep(0,.2,h);float star=frac(sin(dot(floor(d.xz/max(.1,d.y)*450),float2(127.1,311.7)))*43758.5453);c+=step(.998,star)*smoothstep(.2,.8,h)*.5*_Night;return float4(c,1);}
 ENDCG }
 }
}
