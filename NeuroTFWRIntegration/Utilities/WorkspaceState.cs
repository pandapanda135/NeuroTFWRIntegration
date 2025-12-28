using System.Collections.Concurrent;
using UnityEngine;

namespace NeuroTFWRIntegration.Utilities;

public static class WorkspaceState
{
	public static Workspace CurrentWorkspace => Sim.workspace;

	public static ConcurrentDictionary<string, CodeWindow> CodeWindows => CurrentWorkspace.codeWindows;

	public static GameObject Object => Sim.gameObject;

	public static MainSim Sim => MainSim.Inst;

	public static Farm Farm => Sim.sim.farm;


	public static bool MenuOpen => MainMenuActive || ResearchMenuOpen;
	public static bool MainMenuActive => Sim.menu.menu.activeSelf;
	public static bool ResearchMenuOpen => Sim.researchMenu.IsOpen;
}