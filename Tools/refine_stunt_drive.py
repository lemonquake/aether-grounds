from pathlib import Path
root=Path(__file__).resolve().parents[1]
def change(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8');assert old in s,(file,old);p.write_text(s.replace(old,new),encoding='utf-8')
change('Assets/Scripts/StuntTest.cs','v.throttle=v.grounded>=2?1:0;','v.throttle=v.grounded>=2?1:Mathf.Clamp(Mathf.Asin(Mathf.Clamp(v.transform.forward.y,-1,1))*2,-1,1);')
change('Assets/Scripts/StuntTest.cs','v.steer=v.grounded>=2?Mathf.Clamp((aim-v.transform.position.x)*.08f-Vector3.SignedAngle(Vector3.forward,v.transform.forward,Vector3.up)*.035f,-.6f,.6f):0;','v.steer=v.grounded>=2&&v.transform.position.z>4300?Mathf.Clamp((aim-v.transform.position.x)*.015f-v.body.linearVelocity.x*.035f-Vector3.SignedAngle(Vector3.forward,v.transform.forward,Vector3.up)*.02f,-.35f,.35f):0;')
change('Assets/Scripts/Vehicle.cs','rollGrace=.75f;stun=1.0f;','rollGrace=1.5f;rolloverLock=Mathf.Max(rolloverLock,1.5f);righting=0;stun=Mathf.Max(stun,1.5f);')
change('Assets/Scripts/StuntWorld.cs','if(!TrackWorld.StuntGap(courseZ)&&rolloverLock<=0','if(speed>10&&!TrackWorld.StuntGap(courseZ)&&rolloverLock<=0')
