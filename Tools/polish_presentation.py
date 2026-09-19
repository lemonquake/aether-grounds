from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=root/'Assets/Scripts/GarageUI.cs';s=p.read_text(encoding='utf-8');start=s.index('void Card(');end=s.index('void NewMenuGUI()',start)
s=s[:start]+'''void Card(float x,float y,float w,float h,Color? color=null){GUI.color=color??new Color(.045f,.065f,.09f,.97f);GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=Color.white;Line(x,y,w,new Color(.19f,.28f,.35f,.8f));}
  '''+s[end:]
s=s.replace('Label("Your garage",32,116','Panel(0,98,1600,130,.94f);Panel(0,754,1600,146,.96f);Label("Your garage",32,116')
s=s.replace('Label("Drag to rotate · scroll to zoom",461,640','Panel(450,637,543,37,.88f);Label("Drag to rotate · scroll to zoom",461,640')
s=s.replace('public void LoadGarage(){','public void LoadGarage(){')
s=s.replace('garage.selected=Mathf.Clamp','if(!PlayerPrefs.HasKey("garage-v2"))garage.cars[0].name="My "+CarSpec.All[garage.cars[0].body].name;\n   garage.selected=Mathf.Clamp')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/Game.cs';s=p.read_text(encoding='utf-8');s=s.replace('muted=new Color(.6f,.69f,.79f)','muted=new Color(.82f,.87f,.92f)')
s=s.replace('sky.SetColor("_Top",new Color(.12f,.16f,.34f));sky.SetColor("_Horizon",new Color(.53f,.45f,.64f));','sky.SetColor("_Top",new Color(.018f,.028f,.045f));sky.SetColor("_Horizon",new Color(.045f,.065f,.095f));')
s=s.replace('ModelLibrary.Material("garagefloor",new Color(.15f,.20f,.29f)','ModelLibrary.Material("garagefloor",new Color(.035f,.05f,.075f)')
s=s.replace('ModelLibrary.Material("plinth",new Color(.21f,.28f,.37f))','ModelLibrary.Material("plinth",new Color(.055f,.075f,.095f))')
start=s.index('for(int i=0;i<18;i++){float a=i*Mathf.PI*2/18;');end=s.index('var floorMat=',start)
s=s[:start]+'''for(int i=0;i<16;i++){float a=i*Mathf.PI*2/16;var wall=CarParts.Part(showroom.transform,"Studio wall",PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*16,5,Mathf.Cos(a)*16),new Vector3(6.4f,10,.5f),ModelLibrary.Material("Studio wall",new Color(.027f,.043f,.062f)),new Vector3(0,a*Mathf.Rad2Deg,0));CarParts.Part(showroom.transform,"Studio light strip",PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*15.7f,4,Mathf.Cos(a)*15.7f),new Vector3(.06f,6,.06f),ModelLibrary.Material("Studio strip",new Color(.25f,.58f,.67f),1.1f));}
   for(int i=-1;i<=1;i++)CarParts.Part(showroom.transform,"Softbox",PrimitiveType.Cube,new Vector3(i*4,7,0),new Vector3(2,.06f,9),ModelLibrary.Material("Studio softbox",new Color(.7f,.8f,.86f),1.4f));
   '''+s[end:]
s=s.replace('light.intensity=i==0?4:2.4f','light.intensity=i==0?1.8f:1.1f')
s=s.replace('cam.transform.position=Vector3.SmoothDamp(cam.transform.position,desired,ref cameraVel,.14f,200,Time.unscaledDeltaTime);','if(Vector3.Distance(cam.transform.position,desired)>65){cam.transform.position=desired;cameraVel=Vector3.zero;}cam.transform.position=Vector3.SmoothDamp(cam.transform.position,desired,ref cameraVel,.14f,200,Time.unscaledDeltaTime);')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/Presentation.cs';s=p.read_text(encoding='utf-8');s=s.replace('sun.intensity=theme==2?1.1f:1.65f','sun.intensity=theme<0?.8f:theme==2?.85f:1.35f');s=s.replace('RenderSettings.ambientSkyColor=theme==2?','RenderSettings.ambientSkyColor=theme<0?new Color(.24f,.29f,.35f):theme==2?');p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/TrackWorld.cs';s=p.read_text(encoding='utf-8');s=s.replace('new Vector3(15,7.5f,20)','new Vector3(15,2.5f,20)').replace('lake.transform.localScale=new Vector3(240,.3f,210)','lake.transform.localScale=new Vector3(264,.3f,250)')
s=s.replace('float baseH=-3+Mathf.PerlinNoise(x*.007f+17,z*.007f+39)*14;','float baseH=-3+Mathf.PerlinNoise(x*.007f+17,z*.007f+39)*14;if(map==0){float basin=new Vector2(x-15,z-20).magnitude;baseH=Mathf.Lerp(-2,baseH,Mathf.SmoothStep(0,1,Mathf.InverseLerp(107,147,basin)));}')
s=s.replace('new Color(.16f,.20f,.45f),new Color(.34f,.19f,.4f),new Color(.065f,.10f,.26f)','new Color(.085f,.28f,.52f),new Color(.28f,.15f,.22f),new Color(.018f,.035f,.095f)')
s=s.replace('new Color(.47f,.49f,.65f),new Color(.73f,.44f,.40f),new Color(.28f,.30f,.49f)','new Color(.46f,.66f,.75f),new Color(.70f,.43f,.28f),new Color(.12f,.15f,.26f)')
s=s.replace('sky.SetColor("_Horizon",RenderSettings.fogColor*1.35f);','sky.SetColor("_Horizon",RenderSettings.fogColor*1.15f);sky.SetFloat("_Night",index==2?1:0);')
s=s.replace('ModelLibrary.Material("ground"+map,ground,0,0)','GroundMaterial()')
s=s.replace('void MakeTerrain(){','Material GroundMaterial(){var m=ModelLibrary.Material("ground"+map,ground,0,0);m.SetFloat("_Surface",map==1?2:3);m.SetFloat("_Smoothness",.12f);return m;}\n  void MakeTerrain(){')
p.write_text(s,encoding='utf-8')
p=root/'Assets/Shaders/AetherSky.shader';s=p.read_text(encoding='utf-8');s=s.replace('Properties {','Properties { _Night("Night",Float)=0 ');s=s.replace('float4 _Top,_Horizon;','float4 _Top,_Horizon;float _Night;');s=s.replace('*.5;return float4(c,1);','*.5*_Night;return float4(c,1);');p.write_text(s,encoding='utf-8')
p=root/'Assets/Scripts/OverhaulTest.cs';s=p.read_text(encoding='utf-8');s=s.replace('Physics.SyncTransforms();}\n  IEnumerator Start()', 'Physics.SyncTransforms();if(v==game.player){game.cam.transform.position=v.body.position-game.track.Forward(n)*9+Vector3.up*4;game.cam.transform.LookAt(v.body.position+game.track.Forward(n)*4+Vector3.up);}}\n  IEnumerator Start()');p.write_text(s,encoding='utf-8')
