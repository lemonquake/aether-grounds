Shader "Aether/AirahTerrain" {
 Properties { _MainTex("Generated granite and moss",2D)="white"{} _Meadow("Generated meadow",2D)="white"{} }
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Lambert noforwardadd
 #pragma target 3.0
 sampler2D _MainTex,_Meadow;
 struct Input {float3 worldPos;float3 worldNormal;};
 void surf(Input IN,inout SurfaceOutput o){
  float3 p=IN.worldPos;float3 w=pow(abs(IN.worldNormal),4);w/=max(.001,w.x+w.y+w.z);
  float3 rock=tex2D(_MainTex,p.yz*.045).rgb*w.x+tex2D(_MainTex,p.xz*.045).rgb*w.y+tex2D(_MainTex,p.xy*.045).rgb*w.z;
  float broad=tex2D(_MainTex,p.xz*.0015).g;float detail=tex2D(_MainTex,p.xz*.19).g;
  float3 meadow=lerp(float3(.045,.10,.028),float3(.18,.23,.065),saturate(broad*2));meadow*=.85+detail*.55;meadow*=lerp(float3(.8,.9,.7),tex2D(_Meadow,p.xz*.12).rgb*2.3,.65);
  float slope=1-saturate(IN.worldNormal.y);float alpine=smoothstep(590,790,p.y)*.45;float blend=saturate(smoothstep(.12,.52,slope)+alpine);
  o.Albedo=lerp(meadow,rock*.85,blend);o.Alpha=1;
 }
 ENDCG
 } Fallback "Diffuse"
}
