Shader "Aether/LegacyToon" {
 Properties { _Color("Color",Color)=(1,1,1,1) _Emission("Glow",Float)=0 _Outline("Outline",Float)=0.012 }
 SubShader {
 Tags { "RenderType"="Opaque" }
 Pass {
 Tags { "LightMode"="ForwardBase" }
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fwdbase
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 #include "Lighting.cginc"
 #include "AutoLight.cginc"
 fixed4 _Color; float _Emission;
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;};
 struct v2f {float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 world:TEXCOORD1;SHADOW_COORDS(2) UNITY_FOG_COORDS(3)};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.n=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;TRANSFER_SHADOW(o);UNITY_TRANSFER_FOG(o,o.pos);return o;}
 fixed4 frag(v2f i):SV_Target {float3 n=normalize(i.n);float d=dot(n,normalize(_WorldSpaceLightPos0.xyz));float band=d>.55?1.12:(d>.05?.88:.61);float sh=SHADOW_ATTENUATION(i);float3 col=_Color.rgb*(band*lerp(.7,1,sh));float rim=pow(1-saturate(dot(n,normalize(_WorldSpaceCameraPos-i.world))),4);col+=rim*.10*float3(.4,.75,1)+_Color.rgb*_Emission;fixed4 c=fixed4(col,1);UNITY_APPLY_FOG(i.fogCoord,c);return c;}
 ENDCG
 }
 Pass {
 Cull Front
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Outline;
 float4 vert(float4 v:POSITION,float3 n:NORMAL):SV_POSITION {v.xyz+=n*_Outline;return UnityObjectToClipPos(v);}
 fixed4 frag():SV_Target{return fixed4(.025,.035,.065,1);}
 ENDCG
 }
 UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 }
 Fallback "Diffuse"
}
