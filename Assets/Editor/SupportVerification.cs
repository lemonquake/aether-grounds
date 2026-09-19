using System;
using System.IO;
using Aether;
public static class SupportVerification {
 public static void VerifyAndBuildAndroid(){Verify();BuildGame.BuildAndroid();}
 public static void Verify(){
  string token=File.ReadAllText("Evidence/Premium/test-license.txt").Trim();
  if(!SupportStore.Verify(token,"0000000000000000",out var sku)||sku!="pip-city")throw new Exception("Valid signature rejected");
  if(SupportStore.Verify(token,"1111111111111111",out sku))throw new Exception("Wrong player accepted");
  var sections=token.Split('.');var payload=Convert.FromBase64String(sections[0]);payload[payload.Length-1]^=1;string tampered=Convert.ToBase64String(payload)+"."+sections[1];
  if(SupportStore.Verify(tampered,"0000000000000000",out sku))throw new Exception("Tampered license accepted");
  File.WriteAllText("Evidence/Premium/license-verification.txt","PASS valid signature; PASS wrong player rejected; PASS tampered payload rejected");
 }
}
