using System.Collections;
using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Patches;

public static class RegisterPatches
{
	[HarmonyPatch(typeof(Menu), nameof(Menu.Play))]
	[HarmonyPrefix]
	public static void CloseMenuPrefix()
	{
		Context.Send($"The pause menu has been closed, allowing you to play the game.");
		RegisterMainActions.RegisterMain();
	}

	private static bool _firstStart = true;
	[HarmonyPatch(typeof(Menu), nameof(Menu.Open))]
	[HarmonyPrefix]
	public static void OpenMenuPrefix()
	{
		if (_firstStart)
		{
			Context.Send("The game has started and you are in the main menu!");
			_firstStart = false;
			return;
		}
		
		Context.Send("The pause menu has been opened preventing you from playing the game.");
		RegisterMainActions.UnregisterMain();
	}
	
	[HarmonyPatch(typeof(ResearchMenu), nameof(ResearchMenu.OpenCloseMenu))]
	[HarmonyPrefix]
	public static void SetupResearchMenu()
	{
		Plugin.Instance?.StartCoroutine(SetupResearchMenuRoutine());
	}

	private static IEnumerator SetupResearchMenuRoutine()
	{
		// we do this to prevent her running actions while the menu is open.
		if (WorkspaceState.ResearchMenuOpen)
		{
			yield return new WaitForSeconds(0.25f);
			RegisterMainActions.RegisterMain();
		}
		else
		{
			RegisterMainActions.UnregisterMain();	
		}
		
		Utilities.Logger.Info($"docs: {string.Join("\n",WorkspaceState.Sim.researchMenu.allBoxes.Select(box => box.Value.unlockSO.docs))}");
		
		// everything here is either handled by the action or not needed by it
		if (ConfigHandler.ResearchMenuActions.Entry.Value == ResearchMenuActions.OutOfMenu)
		{
			yield break;
		}
		
		// unlockable or upgradeable
		string getBoxesText = ResearchActions.GetBoxesText();
		if (ConfigHandler.ResearchMenuActions.Entry.Value == ResearchMenuActions.None)
		{
			Context.Send($"# Available Upgrades\n{getBoxesText}");
			yield break;
		}

		ActionWindow.Create(WorkspaceState.Object).AddAction(new ResearchActions.BuyUpgrade()).SetForce(0,
			"You are now in the research menu where you can buy upgrades to help you.",
			$"# Possible Upgrades\n{getBoxesText}", true).Register();
	}

	/// <summary>
	/// When a code window is created, we unregister and register the actions to update schemas and send context of its creation.
	/// </summary>
	[HarmonyPatch(typeof(Workspace), nameof(Workspace.OpenCodeWindow))]
	[HarmonyPostfix]
	public static void PostCreateCodeWindow(Workspace __instance, string fileName)
	{
		// we check if menu is active as otherwise when the game is first loaded and the windows are created the actions will be registered in the menu.
		if (WorkspaceState.MenuOpen) return;
		
		Context.Send(string.Format(Strings.CodeWindowCreated, __instance.codeWindows[fileName].fileNameText.text));
		RegisterMainActions.UnregisterMain();
		RegisterMainActions.RegisterMain();
	}
}