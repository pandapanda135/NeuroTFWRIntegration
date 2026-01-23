using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NeuroTFWRIntegration.Unity;

public static class AssetBundleHelper
{
	private static readonly Dictionary<string, AssetBundle> LoadedBundles = new(); 
	
	public static AssetBundle GetAssetBundle(string path)
	{
		Utilities.Logger.Info($"amount of loaded: {AssetBundle.GetAllLoadedAssetBundles_Native().Length}      {AssetBundle.GetAllLoadedAssetBundles().Count()}");
		if (!File.Exists(path))
		{
			throw new FileNotFoundException("You are missing the toast asset bundle.");
		}

		try
		{
			if (LoadedBundles.TryGetValue(path, out var bundle))
			{
				return bundle;
			}
			
			bundle = AssetBundle.LoadFromFile(path);
			LoadedBundles.Add(path, bundle);
			
			return bundle;
		}
		catch (Exception e)
		{
			Utilities.Logger.Error($"error loading bundle: {e}");
			return new();
		}
	}

	public static GameObject? LoadBundle(string path, string name)
	{
		return LoadedBundles.TryGetValue(path, out var bundle) ? bundle.LoadAsset<GameObject>(name) : null;
	}
	
	public static Material? LoadBundle(string path, string name, bool material)
	{
		return LoadedBundles.TryGetValue(path, out var bundle) ? bundle.LoadAsset<Material>(name) : null;
	}

	public static void UnloadBundle(string path, bool unloadAllLoadedObjects = false)
	{
		if (!LoadedBundles.TryGetValue(path, out var bundle))
		{
			Utilities.Logger.Warning($"Tried to unload bundle that is not stored in LoadedBundles. {path}");
			return;
		}

		bundle.Unload(unloadAllLoadedObjects);
		LoadedBundles.Remove(path);
	}
}