using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public class ToastsManager : MonoBehaviour
{
	public void Awake()
	{
		return;
	}

	public void Update()
	{
		return;
	}

	public void AddToast(GameObject toastPrefab)
	{
		Utilities.Logger.Info($"adding toast");
		var toastGo = Instantiate(toastPrefab, transform);
		var toast = toastGo.GetComponent<BaseToast>();

		if (!toast)
			throw new Exception("Toast prefab must contain a BaseToast component");

		toastGo.transform.SetAsLastSibling();
	}

	private static readonly string ToastPath =  Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "validation-toast");
	public static GameObject? CreateToastObject(string text, ValidationToast.ValidationLevels level, Color? flavourColor = null)
	{
		var toastBundle = AssetBundleHelper.GetAssetBundle(ToastPath);
		Utilities.Logger.Info($"creating toast: {ToastPath}");
		var toastAsset = AssetBundleHelper.LoadBundle(ToastPath,"Assets/ValidationToast.prefab");
		if (toastAsset is null)
		{
			Utilities.Logger.Error($"toast asset was null");
			return null;
		}
		var toast = toastAsset.AddComponent<ValidationToast>();

		toast.Init(text, level, flavourColor);
		return toastAsset;
	}
}