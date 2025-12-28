using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration;

public static class RegisterPatches
{
	[HarmonyPatch(typeof(Menu), nameof(Menu.Play))]
	[HarmonyPrefix]
	public static void CloseMenuPrefix()
	{
		Logger.Info($"pressed play");
		RegisterMainActions.RegisterMain();
	}
	
	[HarmonyPatch(typeof(Menu), nameof(Menu.Open))]
	[HarmonyPrefix]
	public static void OpenMenuPrefix()
	{
		Logger.Info($"opened menu");
		RegisterMainActions.UnregisterMain();
	}
	
	[HarmonyPatch(typeof(ResearchMenu), nameof(ResearchMenu.OpenCloseMenu))]
	[HarmonyPrefix]
	public static void SetupResearchMenu()
	{
		Logger.Info($"setup research");
		if (WorkspaceState.Sim.researchMenu.IsOpen)
		{
			RegisterMainActions.RegisterMain();
			return;
		}
		
		RegisterMainActions.UnregisterMain();
		
		Logger.Info($"docs: {string.Join("\n",WorkspaceState.Sim.researchMenu.allBoxes.Select(box => box.Value.unlockSO.docs))}");
		
		string getBoxesText = string.Join("\n", WorkspaceState.Sim.researchMenu.allBoxes
			.Where(kvp => kvp.Value.unlockState is UnlockBox.UnlockState.Unlockable or UnlockBox.UnlockState.Upgradable)
			.Select<KeyValuePair<string, UnlockBox>, string>(kvp =>
			{
				string text = $"## {kvp.Value.unlockSO.unlockName}\n### {(kvp.Value.unlockState is UnlockBox.UnlockState.Upgradable ? "Upgrade Cost" : "Unlock Cost")}";
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
	/// When a code window is created, we unregister and register the actions to update schemas
	/// </summary>
	[HarmonyPatch(typeof(Workspace), nameof(Workspace.OpenCodeWindow))]
	[HarmonyPostfix]
	public static void PostCreateCodeWindow()
	{
		RegisterMainActions.UnregisterMain();
		RegisterMainActions.RegisterMain();
	} 
	
	[HarmonyPatch(typeof(UnlockBox), nameof(UnlockBox.ButtonClicked))]
	[HarmonyPostfix]
	public static void PostUnlockBox(UnlockBox __instance)
	{
		if (Plugin.ResearchMenuActions is not null && Plugin.ResearchMenuActions.Value) return;

		// this will happen if the item is maxed 
		if (WorkspaceState.Farm.GetUnlockCost(__instance.unlockSO) == null ||
		    WorkspaceState.Farm.GetUnlockCost(__instance.unlockSO) == ItemBlock.CreateEmpty()) return;
		
		// this is for upgrades, we also use this to check if a button that cannot be bought is still clicked.
		if (__instance.NumUnlocked() == __instance.prevNumUnlocked) return;

		if (__instance.prevNumUnlocked != 0)
		{
			Context.Send($"{__instance.unlockSO.unlockName} was upgraded to level {__instance.NumUnlocked()}");
			return;
		}
		
		Context.Send($"{__instance.unlockSO.unlockName} was unlocked.");
	}
	
	[HarmonyPatch(typeof(CodeWindow), nameof(CodeWindow.ShowError))]
	[HarmonyPostfix]
	public static void PostShowError(CodeWindow __instance)
	{
		if (!__instance.errorMessage.IsShowing()) return;

		// start of text to end of error
		var substring = __instance.CodeInput.text[..__instance.errorEndIndex];
		var lines = substring.Split("\n").ToList();
		int lineCount = lines.Count;
		lines.RemoveAt(lines.IndexOf(lines.Last()));
		int nonErrorCharacters = lines.Join().Length;

		Context.Send(string.Format(Strings.ErrorMessageContext,__instance.fileNameText.text,
			__instance.errorMessage.errorText.text,lineCount,__instance.errorStartIndex - nonErrorCharacters));
	}
}