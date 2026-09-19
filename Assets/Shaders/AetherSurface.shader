Shader "Aether/Toon" {
 Properties {
  _Color("Color",Color)=(1,1,1,1) _Emission("Glow",Float)=0 _Outline("Outline",Float)=0
  _Metallic("Metallic",Range(0,1))=0 _Smoothness("Smoothness",Range(0,1))=.38
  _MainTex("Detail albedo",2D)="white"{} _TextureMix("Texture mix",Range(0,1))=0 _TextureScale("World tile scale",Float)=.22
  _Surface("Surface: plain, asphalt, sand, stone, floor",Float)=0
 }
 SubShader {
 Tags { "RenderType"="Opaque" } LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 #pragma multi_compile_instancing
 sampler2D _MainTex;float _TextureMix,_TextureScale;fixed4 _Color; half _Metallic,_Smoothness,_Emission; float _Surface;
 struct Input {float3 worldPos;float3 worldNormal;};
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.worldPos; float grain=hash(floor(p.xz*35)); float pattern=1;
  if(_Surface>.5 && _Surface<1.5)pattern=.80+grain*.28+sin(p.x*1.4+p.z*.3)*.025;
  if(_Surface>1.5 && _Surface<2.5)pattern=.84+grain*.15+.13*sin(p.x*.8+p.z*.35+sin(p.z*.17)*3);
  if(_Surface>2.5 && _Surface<3.5){float2 tile=abs(frac(p.xz*.32)-.5);pattern=lerp(.56,.96,1-smoothstep(.47,.495,max(tile.x,tile.y)))+grain*.07;}
  if(_Surface>3.5 && _Surface<4.5){float2 tile=abs(frac(p.xz*.4)-.5);pattern=lerp(.7,1,1-smoothstep(.483,.495,max(tile.x,tile.y)));}
  if(_Surface>4.5 && _Surface<5.5){float ridge=sin(p.x*47+p.z*9);float rust=hash(floor(p.xz*3));pattern=.78+ridge*.13+grain*.08;pattern*=lerp(.64,1,step(.15,rust));}
  if(_Surface>5.5 && _Surface<6.5){float stains=hash(floor(float2(p.x+p.z,p.y)*1.4));pattern=.83+grain*.09+stains*.12;pattern*=lerp(.7,1,saturate((p.y-3)*.9));}
  if(_Surface>6.5){float2 slab=abs(frac(p.xz/3)-.5);float joint=smoothstep(.485,.497,max(slab.x,slab.y));float crack=step(.995,hash(floor(p.xz*12)));pattern=.94+grain*.13-joint*.24-crack*.22;}
  float3 weights=pow(abs(IN.worldNormal),4);weights/=max(.001,weights.x+weights.y+weights.z);float3 albedo=tex2D(_MainTex,p.yz*_TextureScale).rgb*weights.x+tex2D(_MainTex,p.xz*_TextureScale).rgb*weights.y+tex2D(_MainTex,p.xy*_TextureScale).rgb*weights.z;o.Albedo=_Color.rgb*pattern*lerp(float3(1,1,1),albedo*2,_TextureMix);o.Metallic=_Metallic;o.Smoothness=_Smoothness;
  o.Emission=_Color.rgb*_Emission;o.Occlusion=1;o.Alpha=1;
 }
 ENDCG
 }
 Fallback "Standard"
}
