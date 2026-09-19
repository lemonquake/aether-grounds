Shader "Aether/Water" {
 Properties {_Color("Water",Color)=(.025,.28,.35,.85) _Foam("Foam",Color)=(.65,.96,.94,1)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 #include "Lighting.cginc"
 sampler2D_float _CameraDepthTexture;float4 _Color,_Foam;
 struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float4 screen:TEXCOORD1;UNITY_FOG_COORDS(2)};
 v2f vert(float4 p:POSITION){v2f o;float3 w=mul(unity_ObjectToWorld,p).xyz;w.y+=sin(w.x*.55+_Time.y*1.8)*.025+sin(w.z*.7-_Time.y*1.4)*.018;o.world=w;o.pos=UnityWorldToClipPos(w);o.screen=ComputeScreenPos(o.pos);o.screen.z=-mul(UNITY_MATRIX_V,float4(w,1)).z;UNITY_TRANSFER_FOG(o,o.pos);return o;}
 fixed4 frag(v2f i):SV_Target {
  float t=_Time.y;float3 n=normalize(float3(-cos(i.world.x*.55+t*1.8)*.18,1,-cos(i.world.z*.7-t*1.4)*.16));
  n.x+=sin(i.world.z*2+i.world.x*1.6+t*2.8)*.055;
  float3 v=normalize(_WorldSpaceCameraPos-i.world);float fres=pow(1-saturate(dot(n,v)),4);
  float depth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screen)))-i.screen.z;
  float wave=sin(i.world.x*2.2+t*2+sin(i.world.z*2-t))*sin(i.world.z*3-t*1.6);
  float foam=(1-saturate(depth*2.2))*smoothstep(-.35,.55,wave);
  float3 reflected=DecodeHDR(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0,reflect(-v,n)),unity_SpecCube0_HDR);
  float sparkle=pow(saturate(dot(reflect(-normalize(_WorldSpaceLightPos0.xyz),n),v)),180);
  float3 col=lerp(_Color.rgb*.8,_Color.rgb*1.5,saturate(depth*.6));col=lerp(col,reflected,fres*.75+.18);col+=sparkle*_LightColor0.rgb*2+foam*_Foam.rgb*.8;
  fixed4 c=fixed4(col,lerp(.64,.96,saturate(depth*.7))+foam*.1);UNITY_APPLY_FOG(i.fogCoord,c);return c;
 }
 ENDCG}
 }
}
