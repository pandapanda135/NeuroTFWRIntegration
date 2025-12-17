using HarmonyLib;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Actions;

namespace NeuroTFWRIntegration;

public static class RegisterPatches
{
	[HarmonyPatch(typeof(Menu), nameof(Menu.Play))]
	[HarmonyPrefix]
	public static void StartPrefix()
	{
		Logger.Info($"pressed play");
		RegisterMainActions.RegisterMain();
	}
	
	[HarmonyPatch(typeof(Menu), nameof(Menu.Open))]
	[HarmonyPrefix]
	public static void OpenMenu()
	{
		Logger.Info($"opened menu");
		NeuroActionHandler.UnregisterActions(new PatchActions.GetWindowCode(), new PatchActions.WritePatch());
	}

}