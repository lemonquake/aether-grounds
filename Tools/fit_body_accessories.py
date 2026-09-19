from pathlib import Path
p=Path(__file__).resolve().parents[1]/'Assets/Scripts/CarParts.cs'
s=p.read_text(encoding='utf-8')
s=s.replace('[c.spoiler],z=c.body==6?', '[c.spoiler]+(c.body==8?.85f:c.body==5?.25f:0),z=c.body==6?')
s=s.replace('new Vector3(x,stacks?1.3f:.44f,-2.21f)', 'new Vector3(x,stacks?1.3f:.44f,c.body==6?-1.8f:c.body==5?-2.55f:c.body==2?-1.96f:-2.21f)')
s=s.replace('new Vector3(0,.44f,-2.12f)', 'new Vector3(0,.44f,c.body==6?-1.71f:c.body==5?-2.46f:c.body==2?-1.87f:-2.12f)')
s=s.replace('.67f,1.12f,1.02f,1.05f,1.44f,.68f', '.67f,1.66f,1.06f,1.05f,1.89f,.68f')
p.write_text(s,encoding='utf-8')
