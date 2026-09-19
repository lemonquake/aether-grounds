from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=root/'Assets/Scripts/CarParts.cs';s=p.read_text(encoding='utf-8')
s=s.replace('float y=1.15f+(c.spoiler%4)*.18f','float y=new[]{0f,.96f,1.3f,1.43f,1.85f,1.16f,1.56f,1.67f,1.48f,1.4f,1.22f}[c.spoiler]')
s=s.replace('Box(root,"Selected wing",new Vector3(0,y+.1f,z),new Vector3(width,.08f,.4f),accent,new Vector3(c.spoiler*2,0,0));','if(c.spoiler==6){for(int side=-1;side<=1;side+=2)Box(root,"Split wing",new Vector3(side*width*.28f,y+.1f,z),new Vector3(width*.44f,.08f,.4f),accent);}else Box(root,"Selected wing",new Vector3(0,y+.1f,z),new Vector3(width,.08f,c.spoiler==10?.65f:.4f),accent,new Vector3(c.spoiler==1?22:c.spoiler*2,0,0));')
s=s.replace('if(c.spoiler==3||c.spoiler==6)','if(c.spoiler==3)')
start=s.index('   if(c.exhaust>0){');end=s.index('   if(c.livery>0){',start)
s=s[:start]+'''   if(c.exhaust>0){
    int count=c.exhaust==3?4:c.exhaust==9?3:c.exhaust==1||c.exhaust==5?1:2;
    for(int i=0;i<count;i++){
     float x=c.exhaust==2||c.exhaust==4||c.exhaust==10?(i==0?-.65f:.65f):c.exhaust==3?(i<2?-.65f:.65f)+(i%2==0?-.12f:.12f):(i-(count-1)*.5f)*.25f;
     bool side=c.exhaust==6,stacks=c.exhaust==7;var pos=side?new Vector3(i==0?-1.09f:1.09f,.5f,-.6f):new Vector3(x,stacks?1.3f:.44f,-2.21f);var rot=side?new Vector3(0,0,90):stacks?Vector3.zero:new Vector3(90,0,0);
     if(c.exhaust==4){var tube=RingMesh(root,"Hex exhaust",.064f,.105f,.46f,metal,6);tube.transform.localPosition=pos;tube.transform.localRotation=Quaternion.Euler(0,90,0);}
     else {Cylinder(root,"Exhaust barrel",pos,new Vector3(c.exhaust==5?.43f:c.exhaust==10?.23f:.17f,c.exhaust==10?.3f:.23f,.17f),metal,rot);Cylinder(root,"Exhaust opening",pos+(stacks?Vector3.up*.24f:side?new Vector3(i==0?-.24f:.24f,0,0):Vector3.back*(c.exhaust==10?.31f:.24f)),new Vector3(c.exhaust==5?.36f:.125f,.008f,.125f),dark,rot);}
     if(c.exhaust==10){var tip=RingMesh(root,"Heated titanium rim",.064f,.115f,.075f,ModelLibrary.Material("Titanium blue",new Color(.14f,.29f,.65f)));tip.transform.localPosition=pos+Vector3.back*.29f;tip.transform.localRotation=Quaternion.Euler(0,90,0);}
    }
    if(c.exhaust==8)Box(root,"Center exhaust shroud",new Vector3(0,.44f,-2.12f),new Vector3(.65f,.28f,.26f),dark);
   }
'''+s[end:]
start=s.index('   if(c.livery>0){');end=s.index('   Combine(root);',start)
s=s[:start]+'''   if(c.livery>0){
    float h=new[]{.88f,1.015f,.87f,1.11f,.67f,1.12f,1.02f,1.05f,1.44f,.68f}[c.body];int count=c.livery==1||c.livery==3||c.livery==9?1:c.livery==4?3:2;
    if(c.livery==10){Box(root,"Number one",new Vector3(-.19f,h,1.1f),new Vector3(.05f,.015f,.45f),accent);for(int side=-1;side<=1;side+=2){Box(root,"Number zero sides",new Vector3(.14f+side*.115f,h,1.1f),new Vector3(.045f,.015f,.45f),accent);Box(root,"Number zero ends",new Vector3(.14f,h,1.1f+side*.20f),new Vector3(.23f,.015f,.045f),accent);}}
    else for(int i=0;i<count;i++){float x=c.livery==3?.48f:(i-(count-1)*.5f)*.31f;
     if(c.livery==5)Box(root,"Hood band",new Vector3(0,h,.9f+i*.35f),new Vector3(.9f,.015f,.12f),accent);
     else if(c.livery<=4||c.livery>=8)Box(root,"Hood graphic",new Vector3(x,h,1.1f),new Vector3(c.livery==9?1.1f:.14f,.015f,.73f),accent,new Vector3(0,c.livery==8?(i==0?30:-30):0,0));
     else for(int side=-1;side<=1;side+=2)Box(root,"Side graphic",new Vector3(side*1.035f,.65f,i*.4f),new Vector3(.015f,.14f,c.livery==7?.23f:.8f),accent);
    }
   }
'''+s[end:]
s=s.replace('Cylinder(root,"Tire",Vector3.zero,new Vector3(r*2,.17f,r*2),rubber,new Vector3(0,0,90));','RingMesh(root,"Tire sidewall",r*.68f,r,.34f,rubber);RingMesh(root,"Alloy outer lip",r*.70f,r*.80f,.38f,rim);')
s=s.replace('float x=side*.175f;','float x=side*(kind==7?.105f:.175f);')
s=s.replace('Box(root,"Wheel spoke",','if(kind!=9)Box(root,"Wheel spoke",')
s=s.replace('kind==8?24:kind==1?10:0','kind==8?24:kind==1?10:kind==3?(j%2==0?22:-22):0')
marker='    if(kind==4)Cylinder'
s=s.replace(marker,'''    if(kind==9)for(int cell=0;cell<6;cell++){float ca=cell*Mathf.PI/3;Vector3 center=new Vector3(x+side*.038f,Mathf.Cos(ca)*r*.43f,Mathf.Sin(ca)*r*.43f);for(int edge=0;edge<6;edge++){float a=edge*Mathf.PI/3;Box(root,"Hexagonal wheel cell",center+new Vector3(0,Mathf.Cos(a)*r*.17f,Mathf.Sin(a)*r*.17f),new Vector3(.04f,r*.2f,.025f),rim,new Vector3(edge*60+90,0,0));}}
'''+marker)
p.write_text(s,encoding='utf-8')
