using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;
public static class BuildGame {
 [MenuItem("Aether Grounds/Build Android APK")]
 public static void BuildAndroid(){
  EditorSceneManager.OpenScene("Assets/Scenes/AetherGrounds.unity");
  PlayerSettings.companyName="Aljay Leodones";PlayerSettings.productName="Aether Grounds";
  PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.aljayleodones.aethergrounds");
  PlayerSettings.bundleVersion="1.5.0";PlayerSettings.Android.bundleVersionCode=6;
  PlayerSettings.Android.minSdkVersion=(AndroidSdkVersions)26;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
  PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
  PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.Android.splitApplicationBinary=false;
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{UnityEngine.Rendering.GraphicsDeviceType.Vulkan,UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3});
  PlayerSettings.SplashScreen.show=false;PlayerSettings.colorSpace=ColorSpace.Linear;
  var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Brand/AetherGroundsLogo.png");if(icon)PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
  EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
  Directory.CreateDirectory("Build/Android");AssetDatabase.SaveAssets();
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/AetherGrounds.unity"},locationPathName="Build/Android/Aether-Grounds.apk",target=BuildTarget.Android,options=BuildOptions.CompressWithLz4});
  File.WriteAllText("Evidence/android-build-result.txt",report.summary.result+" | errors="+report.summary.totalErrors+" | bytes="+report.summary.totalSize);
  if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Android build failed");
 }
 [MenuItem("Aether Grounds/Open Game Scene")]
 public static void OpenScene(){EditorSceneManager.OpenScene("Assets/Scenes/AetherGrounds.unity");if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(new Vector3(0,.65f,0),Quaternion.Euler(18,-35,0),7);}
 [MenuItem("Aether Grounds/Export Editable Prefabs")]
 public static void BakeModels(){
  Directory.CreateDirectory("Assets/Art/Prefabs");Directory.CreateDirectory("Assets/Art/Meshes");Directory.CreateDirectory("Assets/Art/Materials");AssetDatabase.Refresh();
  foreach(string name in new[]{"Vanta","Comet","Miso","Nomad","Spectre","Atlas","Pip","Cinder","Relay","Manta","AlienTree","CrystalCluster","Rock","RaceArch","RoadModule","TrackStation"}){
   var root=Aether.ModelLibrary.Create(name);int i=0;
   foreach(var mf in root.GetComponentsInChildren<MeshFilter>()){
    var mesh=mf.sharedMesh;string path="Assets/Art/Meshes/"+name+"_"+i+++".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if(existing){EditorUtility.CopySerialized(mesh,existing);mf.sharedMesh=existing;}else if(!AssetDatabase.Contains(mesh))AssetDatabase.CreateAsset(mesh,path);
    var renderer=mf.GetComponent<MeshRenderer>();var material=renderer.sharedMaterial;string mp="Assets/Art/Materials/"+material.name+".mat";var em=AssetDatabase.LoadAssetAtPath<Material>(mp);if(em){EditorUtility.CopySerialized(material,em);renderer.sharedMaterial=em;}else if(!AssetDatabase.Contains(material))AssetDatabase.CreateAsset(material,mp);
   }
   PrefabUtility.SaveAsPrefabAsset(root,"Assets/Art/Prefabs/"+name+".prefab");Object.DestroyImmediate(root);
  }
  AssetDatabase.SaveAssets();
 }
 public static void Build(){
  BakeModels();PlayerSettings.SplashScreen.show=false;var gameIcon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Brand/AetherGroundsLogo.png");if(gameIcon)PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{gameIcon});
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("Aether Grounds");root.AddComponent<Aether.Game>();
  var preview=new GameObject("Editor Preview");preview.tag="EditorOnly";var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/Vanta.prefab"));model.transform.SetParent(preview.transform);
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/AetherGrounds.unity");
  PlayerSettings.companyName="Aljay Leodones";PlayerSettings.productName="Aether Grounds";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
  var shaders=new[]{Shader.Find("Aether/AirahTerrain"),Shader.Find("Aether/AirahSky"),Shader.Find("Aether/TireSmoke"),Shader.Find("Aether/Paint"),Shader.Find("Aether/SpeedGhost"),Shader.Find("Aether/StoreGlass"),Shader.Find("Aether/RushOcean"),Shader.Find("Aether/RushSky"),Shader.Find("Aether/Ice"),Shader.Find("Aether/LightVolume"),Shader.Find("Aether/StructureText"),Shader.Find("Aether/Toon"),Shader.Find("Aether/Sky"),Shader.Find("Aether/Shield"),Shader.Find("Aether/Water"),Shader.Find("Hidden/Aether/Post"),Shader.Find("Sprites/Default"),Shader.Find("Particles/Standard Unlit")};
  var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);var instancing=settings.FindProperty("m_InstancingStripping");int keepAll=System.Array.IndexOf(instancing.enumNames,"Keep All");if(keepAll<0)throw new System.Exception("KeepAll instancing setting unavailable: "+string.Join(",",instancing.enumNames));instancing.enumValueIndex=keepAll;var property=settings.FindProperty("m_AlwaysIncludedShaders");var included=new HashSet<Shader>();for(int i=0;i<property.arraySize;i++){var shader=property.GetArrayElementAtIndex(i).objectReferenceValue as Shader;if(shader)included.Add(shader);}foreach(var shader in shaders){if(!shader)throw new System.Exception("Required shader missing");included.Add(shader);}property.arraySize=included.Count;int shaderIndex=0;foreach(var shader in included)property.GetArrayElementAtIndex(shaderIndex++).objectReferenceValue=shader;settings.ApplyModifiedProperties();
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/AetherGrounds.unity",true)};AssetDatabase.SaveAssets();
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/AetherGrounds.unity"},locationPathName="Build/Aether Grounds.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  foreach(var shader in shaders)if(shader&&ShaderUtil.ShaderHasError(shader))throw new System.Exception("Shader compilation failed: "+shader.name);
  File.WriteAllText("Evidence/build-result.txt",report.summary.result+" | errors="+report.summary.totalErrors+" | bytes="+report.summary.totalSize);
  if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Build failed");
 }
}


