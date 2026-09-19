from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=root/'Assets/Scripts/Vehicle.cs';s=p.read_text();old='(compression*(carIndex==3?30000:36000)-Vector3.Dot(velocity,transform.up)*4200)*(body.mass/spec.mass),0,26000*(body.mass/spec.mass)';new='(compression*(carIndex==3?30000:36000)*(track.isStunts?2.5f:1)-Vector3.Dot(velocity,transform.up)*4200*(track.isStunts?1.6f:1))*(body.mass/spec.mass),0,26000*(body.mass/spec.mass)*(track.isStunts?2.5f:1)';assert old in s;s=s.replace(old,new);p.write_text(s)
p=root/'Assets/Scripts/StuntTest.cs';s=p.read_text();s=s.replace('v.grounded>=2&&v.transform.position.z>4300?','v.grounded>=2?');p.write_text(s)
