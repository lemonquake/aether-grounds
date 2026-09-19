Shader "Aether/SpeedGhost" {
 Properties{_Color("Color",Color)=(.1,.8,1,.3)}
 SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 fixed4 _Color;
 struct v2f{float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 view:TEXCOORD1;};
 v2f vert(float4 p:POSITION,float3 n:NORMAL){v2f o;o.pos=UnityObjectToClipPos(p);float3 world=mul(unity_ObjectToWorld,p).xyz;o.n=UnityObjectToWorldNormal(n);o.view=_WorldSpaceCameraPos-world;return o;}
 fixed4 frag(v2f i):SV_Target{float rim=pow(1-abs(dot(normalize(i.n),normalize(i.view))),2);float cameraFade=saturate((length(i.view)-2)/7);return fixed4(_Color.rgb*(.8+rim),_Color.a*(.22+rim*.78)*cameraFade);}
 ENDCG}
 }
}
