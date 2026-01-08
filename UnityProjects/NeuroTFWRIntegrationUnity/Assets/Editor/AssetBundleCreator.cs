using UnityEditor;
using System.IO;

public class BuildAssetBundles
{
  [MenuItem("Assets/Build AssetBundles")]
  static void BuildAllAssetBundles()
  {
    string path = "AssetBundles";
    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);

    BuildPipeline.BuildAssetBundles(
        path,
        BuildAssetBundleOptions.None,
        EditorUserBuildSettings.activeBuildTarget
    );
    
    
  }
}
