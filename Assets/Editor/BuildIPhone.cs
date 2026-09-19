using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Exports the native project; Apple signing and the final device build happen in Xcode.
public static class BuildIPhone {
 const string ScenePath = "Assets/Scenes/AetherGrounds.unity";
 public const string ExportPath = "Build/iPhone/Aether-Grounds-Xcode";

 [MenuItem("Aether Grounds/Export iPhone Xcode Project")]
 public static void Export() {
  if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
   throw new BuildFailedException("Install iOS Build Support for Unity 6000.6.0f1 in Unity Hub, then export again.");
  if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
   throw new BuildFailedException("Switch to iOS in File > Build Profiles before exporting. For batch mode, include -buildTarget iOS.");

  EditorSceneManager.OpenScene(ScenePath);
  PlayerSettings.companyName = "Aljay Leodones";
  PlayerSettings.productName = "Aether Grounds";
  PlayerSettings.bundleVersion = "1.5.0";
  PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.aljayleodones.aethergrounds");
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
  PlayerSettings.iOS.buildNumber = "6";
  PlayerSettings.iOS.targetOSVersionString = "15.0";
  PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
  PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
  PlayerSettings.iOS.appleEnableAutomaticSigning = true;
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
  PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
  PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
  PlayerSettings.allowedAutorotateToLandscapeLeft = true;
  PlayerSettings.allowedAutorotateToLandscapeRight = true;
  PlayerSettings.allowedAutorotateToPortrait = false;
  PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
  PlayerSettings.colorSpace = ColorSpace.Linear;
  PlayerSettings.SplashScreen.show = false;
  EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;
  var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Brand/AetherGroundsLogo.png");
  if (icon) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
  EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
  Directory.CreateDirectory("Build/iPhone");
  Directory.CreateDirectory("Evidence/iPhone");
  AssetDatabase.SaveAssets();
  var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
   scenes = new[] { ScenePath }, locationPathName = ExportPath,
   target = BuildTarget.iOS, options = BuildOptions.CompressWithLz4
  });
  File.WriteAllText("Evidence/iPhone/export-result.txt", report.summary.result +
   " | errors=" + report.summary.totalErrors + " | bytes=" + report.summary.totalSize +
   "\nXcode export only. Native compilation, signing and physical iPhone testing still required.");
  if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("iPhone Xcode export failed; see the Unity log.");
  Debug.Log("iPhone Xcode project exported to " + Path.GetFullPath(ExportPath));
 }

 [PostProcessBuild(100)]
 public static void ConfigureIPhonePlist(BuildTarget target, string path) {
  if (target != BuildTarget.iOS) return;
  EnableHighRefresh(Path.Combine(path, "Info.plist"));
 }

 static void EnableHighRefresh(string path) {
  var doc = XDocument.Load(path);
  var dict = doc.Root.Element("dict");
  if (dict == null) throw new BuildFailedException("iPhone Info.plist has no root dictionary.");
  const string keyName = "CADisableMinimumFrameDurationOnPhone";
  var key = dict.Elements("key").FirstOrDefault(e => e.Value == keyName);
  if (key == null) dict.Add(new XElement("key", keyName), new XElement("true"));
  else {
   var value = key.ElementsAfterSelf().FirstOrDefault();
   if (value == null || value.Name == "key") key.AddAfterSelf(new XElement("true"));
   else value.ReplaceWith(new XElement("true"));
  }
  doc.Save(path);
 }

 // Run on Windows too: validates the export hook without needing Apple credentials.
 public static void ValidatePreparation() {
  if (!File.Exists(ScenePath)) throw new BuildFailedException("Game scene is missing.");
  Directory.CreateDirectory("Evidence/iPhone");
  string path = "Evidence/iPhone/test-Info.plist";
  File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><plist version=\"1.0\"><dict><key>CFBundleDisplayName</key><string>Aether Grounds</string><key>CADisableMinimumFrameDurationOnPhone</key><false/></dict></plist>");
  EnableHighRefresh(path);
  EnableHighRefresh(path);
  var dict = XDocument.Load(path).Root.Element("dict");
  var keys = dict.Elements("key").Where(e => e.Value == "CADisableMinimumFrameDurationOnPhone").ToArray();
  if (keys.Length != 1 || keys[0].ElementsAfterSelf().First().Name != "true" ||
      !dict.Elements("string").Any(e => e.Value == "Aether Grounds"))
   throw new BuildFailedException("iPhone high refresh plist validation failed.");
  File.WriteAllText("Evidence/iPhone/preparation-result.txt", "PASS: Editor scripts compiled; ProMotion plist hook replaces false, is idempotent and preserves unrelated entries.\nThis is not a native iOS build or a device performance test.");
 }
}
