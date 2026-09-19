from pathlib import Path
p=Path(__file__).resolve().parents[1]/'Assets/Scripts/StuntTest.cs';s=p.read_text();s=s.replace('Vector3.SignedAngle(Vector3.forward,v.transform.forward,Vector3.up)','Vector3.SignedAngle(Vector3.forward,Vector3.ProjectOnPlane(v.transform.forward,Vector3.up),Vector3.up)');p.write_text(s)
