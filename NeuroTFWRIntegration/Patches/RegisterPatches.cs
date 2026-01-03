using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.Utilities;

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
		Utilities.Logger.Info($"docs: {string.Join("\n",WorkspaceState.Sim.researchMenu.allBoxes.Select(box => box.Value.unlockSO.docs))}");
		
		// unlockable or upgradeable
		string getBoxesText = string.Join("\n", WorkspaceState.Sim.researchMenu.allBoxes
			.Where(kvp => kvp.Value.unlockState is UnlockBox.UnlockState.Unlockable or UnlockBox.UnlockState.Upgradable)
			.Select<KeyValuePair<string, UnlockBox>, string>(kvp =>
			{
				string text = $"## {kvp.Value.unlockSO.unlockName}\n### Description\n{Localizer.Localize(kvp.Value.unlockSO.description)}\n### {(kvp.Value.unlockState is UnlockBox.UnlockState.Upgradable ? "Upgrade Cost" : "Unlock Cost")}";
				foreach (var item in kvp.Value.unlockSO.unlockCost.serializeList)
				{
					text += $"\n- {item.name} amount: {item.nr}";
				}

				if (!string.IsNullOrEmpty(kvp.Value.unlockSO.docs))
				{
					text += $"\n### Docs Path\n {kvp.Value.unlockSO.docs}";
				}

				return text;
			})
		);
		if (Plugin.ResearchMenuActions is not null && !Plugin.ResearchMenuActions.Value)
		{
			Context.Send($"# Available Upgrades\n{getBoxesText}");
			return;
		}

		var window = ActionWindow.Create(WorkspaceState.Object);
		window.AddAction(new ResearchActions.BuyUpgrade());
		window.SetForce(0, "You are now in the research menu where you can buy upgrades to help you.",
			$"# Possible Upgrades\n{getBoxesText}", true);
		window.Register();
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
		
		Context.Send($"A code window has been created, it is called {__instance.codeWindows[fileName].fileNameText.text}");
		RegisterMainActions.UnregisterMain();
		RegisterMainActions.RegisterMain();
	}
}