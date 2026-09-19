using UnityEditor;
public class AirahTextureImport:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Resources/Airah/"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.sRGBTexture=true;t.mipmapEnabled=true;t.wrapMode=UnityEngine.TextureWrapMode.Repeat;t.filterMode=UnityEngine.FilterMode.Trilinear;t.anisoLevel=8;t.maxTextureSize=1024;t.textureCompression=TextureImporterCompression.Compressed;t.compressionQuality=80;}
}
