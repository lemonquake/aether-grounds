from pathlib import Path
root=Path(__file__).resolve().parents[1]
def change(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8');assert old in s,(file,old);p.write_text(s.replace(old,new),encoding='utf-8')
change('Assets/Scripts/SupportShop.cs','name.EndsWith("L")?-.16f:.16f','name.EndsWith("L")?-.24f:.24f')
change('Assets/Scripts/StorePage.cs','Camera cameraView;Game game;','Camera cameraView;Game game;float yaw=35,zoom=7.2f;')
change('Assets/Scripts/StorePage.cs','new Vector3(0,2.4f,7.2f)','new Vector3(0,2.4f,zoom)')
change('Assets/Scripts/StorePage.cs','Render(game.StoreSelection,30+Mathf.Sin(Time.unscaledTime*.2f)*55);','{var m=game.ScreenToUI(Input.mousePosition);bool hover=new Rect(729,232,834,292).Contains(m);if(hover&&Input.GetMouseButton(0)){yaw-=Input.GetAxis("Mouse X")*4;if(game.phoneMode&&Input.touchCount>0)yaw-=Input.GetTouch(0).deltaPosition.x*.2f;}else yaw+=Time.unscaledDeltaTime*9;if(hover)zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*.4f,5.8f,9.5f);Render(game.StoreSelection,yaw);}')
change('Assets/Scripts/StorePage.cs','Label(selected.name,753,531','Label("Drag to rotate · scroll to zoom",752,239,780,28,18,muted);Label(selected.name,753,531')
