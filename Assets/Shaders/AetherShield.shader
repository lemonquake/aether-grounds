Shader "Aether/Shield" {
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct v2f {float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 view:TEXCOORD1;};
 v2f vert(float4 p:POSITION,float3 n:NORMAL){v2f o;o.pos=UnityObjectToClipPos(p);o.n=UnityObjectToWorldNormal(n);o.view=_WorldSpaceCameraPos-mul(unity_ObjectToWorld,p).xyz;return o;}
 fixed4 frag(v2f i):SV_Target {float rim=pow(1-saturate(dot(normalize(i.n),normalize(i.view))),2);return float4(.18,.95,1,rim*.6+.025);}
 ENDCG }
 }
}
