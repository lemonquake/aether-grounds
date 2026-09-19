Shader "Aether/Ice" {
 Properties {_Color("Ice",Color)=(.32,.72,.95,.45)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float3 normal:NORMAL;};
 struct vary {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;};
 float4 _Color;
 vary vert(app v){vary o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);return o;}
 fixed4 frag(vary i):SV_Target {float rim=pow(1-saturate(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world))),2);float crack=pow(saturate(sin(i.world.y*16+i.world.x*10+sin(i.world.z*9)*3)),28);return float4(_Color.rgb+rim*.55+crack*.65,.24+rim*.46+crack*.25);}
 ENDCG}
 }
}
