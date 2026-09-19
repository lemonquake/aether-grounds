Shader "Aether/StoreGlass" {
 Properties { _Color("Glass tint", Color)=(0.18,0.8,0.94,0.23) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off
  Cull Back
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;
   struct v2f {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 view:TEXCOORD1;};
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);return o;}
   fixed4 frag(v2f i):SV_Target{float3 n=normalize(i.normal),v=normalize(i.view);float f=pow(1-saturate(abs(dot(n,v))),3);half4 probe=UNITY_SAMPLE_TEXCUBE(unity_SpecCube0,reflect(-v,n));float3 reflection=DecodeHDR(probe,unity_SpecCube0_HDR);return fixed4(lerp(_Color.rgb*.7,reflection+float3(.32,.65,.72),f*.8),_Color.a+f*.65);}
   ENDCG
  }
 }
}
