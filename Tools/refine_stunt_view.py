from pathlib import Path
root=Path(__file__).resolve().parents[1]
def change(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8');assert old in s,(file,old);p.write_text(s.replace(old,new),encoding='utf-8')
change('Assets/Scripts/Game.cs','Vector3 desired=player.transform.position-forward*8.6f+Vector3.up*3.9f;','Vector3 desired=player.transform.position-forward*8.6f+Vector3.up*3.9f;if(stuntMode)desired+=player.body.linearVelocity*.09f;')
change('Assets/Scripts/StuntWorld.cs','island.transform.position=new Vector3(0,-110,z);island.transform.localScale=new Vector3(z==45?230:420,StuntHeight(z)*1.6f,500);','var bounds=new Bounds();bool first=true;foreach(var r in island.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}var scale=new Vector3((z==45?280:430)/bounds.size.x,(StuntHeight(z)+102)/bounds.size.y,650/bounds.size.z);island.transform.localScale=scale;island.transform.position=new Vector3(-bounds.center.x*scale.x,-110-bounds.min.y*scale.y,z-bounds.center.z*scale.z);')
change('Assets/Scripts/StuntWorld.cs','public StuntTileKind kind;readonly','Material trapMaterial;public StuntTileKind kind;readonly')
change('Assets/Scripts/StuntWorld.cs','if(kind==StuntTileKind.Trap)for(int j=0;j<4;j++)','if(kind==StuntTileKind.Trap)trapMaterial=mat;if(kind==StuntTileKind.Trap)for(int j=0;j<4;j++)')
change('Assets/Scripts/StuntWorld.cs','public void Apply(Vehicle v){if(!Contains','void Update(){if(trapMaterial)trapMaterial.color=Mathf.Repeat(Time.time,4)>1.4f?new Color(1,.08f,.035f):new Color(.2f,.09f,.055f);}\n  public void Apply(Vehicle v){if(!Contains')
