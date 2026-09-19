from pathlib import Path
root=Path(__file__).resolve().parents[1]
def change(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8');assert old in s,(file,old);p.write_text(s.replace(old,new),encoding='utf-8')
change('Assets/Scripts/StorePage.cs','new Vector3(0,3,9)','new Vector3(0,2.4f,7.2f)')
change('Assets/Scripts/StorePage.cs','cameraView.fieldOfView=34','cameraView.fieldOfView=30')
change('Assets/Scripts/StorePage.cs','ScaleMode.ScaleToFit','ScaleMode.ScaleAndCrop')
change('Assets/Scripts/StorePage.cs','USD · one-time unlock",38,737,645,34,25','USD · one-time",38,737,330,34,23')
change('Assets/Scripts/StuntTest.cs','tb.position=new Vector3(1700,.3f,12);','traffic.disabledUntil=Time.time+.7f;tb.position=new Vector3(1700,.3f,12);')
change('Assets/Scripts/StuntWorld.cs','RenderSettings.skybox=new Material(Shader.Find("Aether/RushSky"));','var sky=new Material(Shader.Find("Aether/RushSky"));sky.SetColor("_Top",new Color(.12f,.31f,.54f));sky.SetColor("_Horizon",new Color(.63f,.82f,.93f));RenderSettings.skybox=sky;')
