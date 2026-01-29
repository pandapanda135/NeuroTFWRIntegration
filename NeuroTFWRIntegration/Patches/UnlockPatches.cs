using System.Collections;
using HarmonyLib;
using NeuroTFWRIntegration.Unity;
using NeuroTFWRIntegration.Unity.Components.SwarmDrone;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeuroTFWRIntegration.Patches;

public static class UnlockPatches
{
	[HarmonyPatch(typeof(UnlockBox), nameof(UnlockBox.ButtonClicked))]
	[HarmonyPostfix]
	public static void PostUnlock(UnlockBox __instance)
	{
		Utilities.Logger.Info($"Unlock box buy: {__instance.unlockSO.unlockName}    {__instance.unlockSO.unlockedHat}");
		
		if (string.IsNullOrEmpty(__instance.unlockSO.unlockedHat)) 
			return;

		if (MainSim.Inst.IsUnlocked(CreateSwarm.HatSo?.hatName))
			return;
		
		MainSim.Inst.UnlockHat(CreateSwarm.HatSo);
	}
	
	/// <summary>
	/// This will create the added components after a scene has been loaded. This checks if they exist and will create them if so.
	/// This checks 2 seconds after the scene has been switched if those objects exist.
	/// </summary>
	[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadScene), typeof(int))]
	[HarmonyPostfix]
	public static void PostLoadScene()
	{
		Plugin.Instance?.StartCoroutine(CreateComponents());
	}

	private static IEnumerator CreateComponents()
	{
		 float currentWaitTime = 0f;
		 
		while (currentWaitTime < 1f)
		{
			currentWaitTime += Time.deltaTime;
			yield return null;
		}
		
		if (ConfigHandler.NeuroChat.Entry.Value && !GameObject.Find("NeuroChat"))
		{
			Utilities.Logger.Info($"readding chat");
			LoadComponents.AddChat();
		}
		
		if (ConfigHandler.Toasts.Entry.Value != Toasts.Disabled && !GameObject.Find("ToastsContainer"))
		{
			Utilities.Logger.Info($"readding container");
			LoadComponents.AddToastsContainer();
		}
		
		if (ConfigHandler.VersionChecking.Entry.Value && !GameObject.Find("VersionCanvas"))
		{
			Utilities.Logger.Info($"readding version");
			Plugin.Instance?.StartCoroutine(LoadComponents.AddVersion());
		}
	}
}