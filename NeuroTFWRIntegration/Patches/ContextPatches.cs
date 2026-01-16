using System.Linq;
using HarmonyLib;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Patches;

public class ContextPatches
{
	// I don't think I've seen this one happen, but it could.
	private static ErrorMessage? _previousMessage;
	[HarmonyPatch(typeof(CodeWindow), nameof(CodeWindow.ShowError))]
	[HarmonyPostfix]
	public static void PostShowError(CodeWindow __instance)
	{
		if (!__instance.errorMessage.IsShowing() || _previousMessage == __instance.errorMessage) return;
		_previousMessage = __instance.errorMessage;

		// start of text to end of error
		var substring = __instance.CodeInput.text[..__instance.errorEndIndex];
		var lines = substring.Split("\n").ToList();
		int lineCount = lines.Count;
		lines.RemoveAt(lines.IndexOf(lines.Last()));
		int nonErrorCharacters = lines.Join().Length;

		Context.Send(string.Format(Strings.ErrorMessageContext,__instance.fileNameText.text,
			__instance.errorMessage.errorText.text,lineCount,__instance.errorStartIndex - nonErrorCharacters));
	}
	

	/// <summary>
	/// This is here to send context when the other playing buys an upgrade
	/// </summary>
	[HarmonyPatch(typeof(UnlockBox), nameof(UnlockBox.ButtonClicked))]
	[HarmonyPostfix]
	public static void PostUnlockBox(UnlockBox __instance)
	{
		if (ConfigHandler.ResearchMenuActions.Entry.Value != ResearchMenuActions.None) return;

		// this will happen if the item is maxed 
		if (WorkspaceState.Farm.GetUnlockCost(__instance.unlockSO) == null ||
		    WorkspaceState.Farm.GetUnlockCost(__instance.unlockSO) == ItemBlock.CreateEmpty()) return;
		
		// this is for upgrades, we also use this to check if a button that cannot be bought is clicked.
		if (__instance.NumUnlocked() == __instance.prevNumUnlocked) return;

		// we do this as this is how the game handles text.
		string snakeName = CodeUtilities.ToUpperSnake(__instance.unlockSO.unlockName);
		string unlockInformation = $"# Description\n{Localizer.Localize(__instance.unlockSO.description)}\n# Docs path\n{__instance.unlockSO.docs}";
		if (__instance.prevNumUnlocked != 0)
		{
			Context.Send($"{snakeName} was upgraded to level {__instance.NumUnlocked()}, here is information on it {unlockInformation}.");
			return;
		}
		
		Context.Send($"{snakeName} was unlocked, here is information on it {unlockInformation}.");
	}
}