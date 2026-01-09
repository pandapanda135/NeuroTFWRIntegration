using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public class ToastsManager : MonoBehaviour
{
	public void AddToast(GameObject toastPrefab)
	{
		var toastGo = Instantiate(toastPrefab, transform);
		var toast = toastGo.GetComponent<BaseToast>();

		if (!toast)
			throw new Exception("Toast prefab must contain a BaseToast component");

		toastGo.transform.SetAsLastSibling();
	}

	private static readonly string ValidationToastPath =  Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "validation-toast");
	public static GameObject? CreateValidationToast(string text, ValidationToast.ValidationLevels level, Color? flavourColor = null)
	{
		AssetBundleHelper.GetAssetBundle(ValidationToastPath);
		Utilities.Logger.Info($"creating toast: {ValidationToastPath}");
		var toastAsset = AssetBundleHelper.LoadBundle(ValidationToastPath,"Assets/ValidationToast.prefab");
		if (toastAsset is null)
		{
			Utilities.Logger.Error($"toast asset was null");
			return null;
		}
		var toast = toastAsset.AddComponent<ValidationToast>();

		toast.Init(text, level, flavourColor);
		return toastAsset;
	}
	
	private static readonly string NeuroToastPath =  Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "neurotoast");
	public static GameObject? CreateNeuroToast(string text)
	{
		AssetBundleHelper.GetAssetBundle(NeuroToastPath);
		Utilities.Logger.Info($"creating toast: {NeuroToastPath}");
		var toastAsset = AssetBundleHelper.LoadBundle(NeuroToastPath,"Assets/NeuroToast.prefab");
		if (toastAsset is null)
		{
			Utilities.Logger.Error($"toast asset was null");
			return null;
		}
		var toast = toastAsset.AddComponent<NeuroToast>();

		toast.Init(text);
		Utilities.Logger.Info($"returning toast asset");
		return toastAsset;
	}
}