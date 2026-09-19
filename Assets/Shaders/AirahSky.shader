Shader "Aether/AirahSky" {
 Properties { _SunDirection("Sun direction",Vector)=(.7,.5,.4,0) }
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float3 _SunDirection;
 struct v2f{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(float4 p:POSITION){v2f o;o.pos=UnityObjectToClipPos(p);o.dir=p.xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 float4 frag(v2f i):SV_Target{float3 d=normalize(i.dir);float h=saturate(d.y);float3 color=lerp(float3(.65,.79,.85),float3(.115,.39,.68),pow(h,.55));
  float2 p=d.xz/max(.13,d.y)*2;float n=noise(p)*.6+noise(p*2)*.26+noise(p*4)*.14;float clouds=smoothstep(.60,.82,n)*smoothstep(.05,.3,d.y);color=lerp(color,float3(.96,.95,.87),clouds*.9);
  float sun=dot(d,normalize(_SunDirection));color+=float3(1,.78,.4)*pow(saturate(sun),140)*.38;float disk=smoothstep(.99988,.99996,sun);color+=disk*float3(4.5,3.8,2.6);return float4(color,1);}
 ENDCG }
 }
}
