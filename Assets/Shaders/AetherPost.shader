Shader "Hidden/Aether/Post" {
 Properties {_MainTex("Scene",2D)="white"{} _Bloom("Bloom",2D)="black"{}}
 SubShader {Cull Off ZWrite Off ZTest Always
 CGINCLUDE
 #include "UnityCG.cginc"
 float4 _Sun;sampler2D _MainTex,_Bloom;float4 _MainTex_TexelSize;
 half4 blur(v2f_img i){float2 d=_MainTex_TexelSize.xy*1.8;return (tex2D(_MainTex,i.uv+d)+tex2D(_MainTex,i.uv-d)+tex2D(_MainTex,i.uv+float2(d.x,-d.y))+tex2D(_MainTex,i.uv+float2(-d.x,d.y)))*.25;}
 ENDCG
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 half4 frag(v2f_img i):SV_Target {half4 c=blur(i);c.rgb=max(0,c.rgb-1.15);return c;}
 ENDCG}
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 half4 frag(v2f_img i):SV_Target{return blur(i);}
 ENDCG}
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 half4 frag(v2f_img i):SV_Target{float3 c=tex2D(_MainTex,i.uv).rgb+tex2D(_Bloom,i.uv).rgb*.3;float2 delta=(i.uv-_Sun.xy)*float2(_Sun.w,1);float glow=exp(-dot(delta,delta)*14)*_Sun.z;c+=float3(1,.69,.35)*glow;c=saturate((c*(2.51*c+.03))/(c*(2.43*c+.59)+.14));float v=1-dot(i.uv-.5,i.uv-.5)*.20;return float4(c*v,1);}
 ENDCG}
 }
}
