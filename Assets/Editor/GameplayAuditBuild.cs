using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

// Rebuild existing content without regenerating prefabs or rewriting the scene.
public static class GameplayAuditBuild {
 public static void Build(){
  Directory.CreateDirectory("Evidence/GameplayAudit");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
   scenes=new[]{"Assets/Scenes/AetherGrounds.unity"},
   locationPathName=Environment.GetEnvironmentVariable("AETHER_BUILD_PATH")??"Build/Aether Grounds.exe",
   target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
  });
  File.WriteAllText("Evidence/GameplayAudit/build-result.txt",report.summary.result+" | errors="+report.summary.totalErrors+" | bytes="+report.summary.totalSize);
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Gameplay audit build failed");
 }
}
