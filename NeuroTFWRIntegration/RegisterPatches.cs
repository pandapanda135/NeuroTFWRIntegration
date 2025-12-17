using HarmonyLib;
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
		RegisterMainActions.UnregisterMain();
	}

	[HarmonyPatch(typeof(ResearchMenu), nameof(ResearchMenu.OpenCloseMenu))]
	[HarmonyPrefix]
	public static void SetupResearchMenu()
	{
		Logger.Warning($"setup research");
		if (MainSim.Inst.researchMenu.IsOpen)
		{
			RegisterMainActions.RegisterMain();
			return;
		}
		
		RegisterMainActions.UnregisterMain();
	}
}