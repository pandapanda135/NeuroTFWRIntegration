using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public class ToastsManager : MonoBehaviour
{
	public void AddToast(GameObject toastPrefab)
	{
		toastPrefab.transform.SetParent(transform);
		// var toastGo = Instantiate(toastPrefab, transform);
		var toast = toastPrefab.GetComponent<BaseToast>();

		if (!toast)
		{
			Destroy(toastPrefab);	
			throw new Exception("Toast prefab must contain a BaseToast component");
		}

		toastPrefab.transform.SetAsLastSibling();
	}

	private static readonly string ValidationToastPath =  AssetBundleHelper.GetBundlePath("validation-toast");
	public static GameObject? CreateValidationToast(string text, ValidationToast.ValidationLevels level, Color? flavourColor = null)
	{
		AssetBundleHelper.GetAssetBundle(ValidationToastPath);
		var prefab = AssetBundleHelper.LoadBundle(ValidationToastPath, "Assets/ValidationToast.prefab");
		var instance = Instantiate(prefab);
		if (!instance)
		{
			Utilities.Logger.Error($"instance was null");
			return null;
		}

		var toast = instance.AddComponent<ValidationToast>();
		toast.Init(text, level, flavourColor);

		return instance;
	}
	
	private static readonly string NeuroToastPath =  AssetBundleHelper.GetBundlePath("neurotoast");
	public static GameObject? CreateNeuroToast(string text)
	{
		AssetBundleHelper.GetAssetBundle(NeuroToastPath);
		var prefab = AssetBundleHelper.LoadBundle(NeuroToastPath, "Assets/NeuroToast.prefab");
		var instance = Instantiate(prefab);
		if (!instance)
		{
			Utilities.Logger.Error($"instance was null");
			return null;
		}

		var toast = instance.AddComponent<NeuroToast>();
		toast.Init(text);

		return instance;
	}
}