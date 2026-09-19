Shader "Aether/RushOcean" {
 Properties { _Color("Water",Color)=(.02,.25,.30,1) _Storm("Storm",Float)=0 }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 #include "Lighting.cginc"
 float4 _Color;float _Storm;
 struct v2f{float4 pos:SV_POSITION;float3 world:TEXCOORD0;UNITY_FOG_COORDS(1)};
 v2f vert(float4 p:POSITION){v2f o;o.world=mul(unity_ObjectToWorld,p).xyz;o.pos=UnityWorldToClipPos(o.world);UNITY_TRANSFER_FOG(o,o.pos);return o;}
 fixed4 frag(v2f i):SV_Target{
 float2 p=i.world.xz;float t=_Time.y;float2 slopes=float2(sin(p.x*.16+p.y*.11+t*.8)*.10+sin(p.x*.42-p.y*.26-t*1.4)*.045,cos(p.y*.14-p.x*.08-t*.7)*.09+sin(p.y*.52+p.x*.23+t*1.2)*.04)*(1+_Storm*.65);
 float dist=distance(_WorldSpaceCameraPos.xz,p);float filter=1-saturate(dist/2200);slopes*=filter;
 float3 n=normalize(float3(slopes.x,1,slopes.y));float3 v=normalize(_WorldSpaceCameraPos-i.world);float fres=pow(1-saturate(dot(n,v)),4);
 float3 reflected=DecodeHDR(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0,reflect(-v,n)),unity_SpecCube0_HDR);
 float3 col=lerp(_Color.rgb*1.1,reflected*.65,fres*.7+.12);float3 halfDir=normalize(normalize(_WorldSpaceLightPos0.xyz)+v);float spec=pow(saturate(dot(n,halfDir)),100)*1.8;
 col+=spec*_LightColor0.rgb;float foam=pow(saturate(sin(p.y*.27+p.x*.08-t*.7)),18)*exp(-abs(i.world.x-23)*.12);col+=foam*float3(.5,.75,.73)*filter;
 fixed4 c=fixed4(col,.97);UNITY_APPLY_FOG(i.fogCoord,c);return c;}
 ENDCG}
 }
}
