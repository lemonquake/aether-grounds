Shader "Aether/StructureText" {
 Properties { _MainTex("Font",2D)="white" {} _Color("Color",Color)=(1,1,1,1) }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Lighting Off Cull Back ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct Output {float4 position:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   sampler2D _MainTex;fixed4 _Color;
   Output vert(Input v){Output o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(Output i):SV_Target {fixed4 c=i.color;c.a*=tex2D(_MainTex,i.uv).a;return c;}
   ENDCG
  }
 }
}
