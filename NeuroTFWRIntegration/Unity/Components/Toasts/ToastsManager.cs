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
}