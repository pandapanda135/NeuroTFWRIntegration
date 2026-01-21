using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeuroTFWRIntegration.Utilities;

public static class WorkspaceState
{
	public static Workspace CurrentWorkspace => Sim.workspace;

	public static ConcurrentDictionary<string, CodeWindow> CodeWindows => CurrentWorkspace.codeWindows;

	public static GameObject Object => Sim.gameObject;

	public static MainSim Sim => MainSim.Inst;

	public static Farm Farm => Sim.sim.farm;

	public static FarmRenderer FarmRenderer => GameObject.Find("Farm").GetComponent<FarmRenderer>();

	public static List<KeyValuePair<string, UnlockBox>> ValidBoxes => Sim.researchMenu.allBoxes.Where(kvp =>
		kvp.Value.unlockState is UnlockBox.UnlockState.Unlockable or UnlockBox.UnlockState.Upgradable).ToList();
	public static List<KeyValuePair<string, UnlockBox>> UnlockableBoxes => ValidBoxes.Where(kvp => CanUnlock(kvp.Value.unlockSO)).ToList();
	
	public static bool MenuOpen => MainMenuActive || ResearchMenuOpen;
	public static bool MainMenuActive => Sim.menu.menu.activeSelf;
	public static bool ResearchMenuOpen => Sim.researchMenu.IsOpen;

	private static bool CanUnlock(UnlockSO unlockSo, bool requireParent = true)
	{
		if ((requireParent && !string.IsNullOrEmpty(unlockSo.parentUnlock) && !Farm.IsUnlocked(unlockSo.parentUnlock)) || 
		    Farm.NumUnlocked(unlockSo) >= unlockSo.maxUnlockLevel || 
		    (Farm.sim.singleDrone && (unlockSo.unlockName == "megafarm" || unlockSo.unlockName == "expand")))
		{
			return false;
		}
		ItemBlock unlockCost = Farm.GetUnlockCost(unlockSo);
		if (unlockCost == null)
		{
			return false;
		}
		if (!Farm.Items.Contains(unlockCost))
		{
			return false;
		}

		return true;
	}
}