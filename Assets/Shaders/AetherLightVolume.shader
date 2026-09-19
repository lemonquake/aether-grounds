Shader "Aether/LightVolume" {
 Properties {_Color("Scattering",Color)=(1,.7,.4,.035)}
 SubShader {Tags {"Queue"="Transparent+5" "RenderType"="Transparent"} Blend One One ZWrite Off ZTest Always Cull Front
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _CameraDepthTexture;float4 _Color;
 struct app {float4 vertex:POSITION;};struct vary {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float4 screen:TEXCOORD1;};
 vary vert(app v){vary o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.screen=ComputeScreenPos(o.pos);return o;}
 float4 frag(vary i):SV_Target {
  float3 ray=normalize(i.world-_WorldSpaceCameraPos);float3 origin=mul(unity_ObjectToWorld,float4(0,0,.5,1)).xyz;
  float span=length(mul((float3x3)unity_ObjectToWorld,float3(0,0,1)))*1.3;
  float middle=dot(origin-_WorldSpaceCameraPos,ray);float start=max(0,middle-span),end=middle+span;
  float scene=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screen)));
  float3 viewray=mul((float3x3)UNITY_MATRIX_V,ray);end=min(end,scene/max(.05,-viewray.z));
  float stepSize=max(0,end-start)/16;float total=0;
  [unroll]for(int j=0;j<16;j++){float3 p=mul(unity_WorldToObject,float4(_WorldSpaceCameraPos+ray*(start+(j+.5)*stepSize),1)).xyz;
   float radial=length(p.xy)/max(.01,p.z);float density=saturate(1-radial*radial)*saturate(p.z*8)*saturate(1-p.z);if(p.z>0&&p.z<1)total+=density*stepSize;}
  return float4(_Color.rgb*min(total*_Color.a,.19),0);
 }
 ENDCG}
 }
}
