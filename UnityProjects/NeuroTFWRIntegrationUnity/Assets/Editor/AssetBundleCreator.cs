using UnityEditor;
using System.IO;

namespace Editor
{
  public static class BuildAssetBundles
  {
	[MenuItem("Assets/Build AssetBundles")]
	public static void BuildAllAssetBundles()
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
}
