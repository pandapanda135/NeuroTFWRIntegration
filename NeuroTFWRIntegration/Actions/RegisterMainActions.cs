using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Actions;

public static class RegisterMainActions
{
	private static bool _mainRegistered;
	
	public static void RegisterMain()
	{
		if (_mainRegistered) return;
		_mainRegistered = true;
		if (!WorkspaceState.CodeWindows.Any())
		{
			NeuroActionHandler.RegisterActions(new CodeWindowActions.CreateWindow());
			return;
		}

		List<INeuroAction> actions = new();
		// for some reason this requires closing and opening a menu after pressing play to work,
		// IDK why IDK how to fix it, and I've spent way too much time trying to get it to work already
		if (WorkspaceState.Sim.researchMenu.allBoxes.Count > 0)
			actions.Add(new DocsActions.GetDocumentation());
		
		actions.AddRange([new PatchActions.GetWindowCode(), new PatchActions.WritePatch(), new CodeWindowActions.CreateWindow()]);
		NeuroActionHandler.RegisterActions(actions);
	}
	
	public static void UnregisterMain()
	{
		if (!_mainRegistered) return;
		_mainRegistered = false;
		
		// if there is only one or no code windows and all the mains are not registered.
		if (WorkspaceState.CodeWindows.Count < 2 && NeuroActionHandler.GetRegistered($"{new PatchActions.WritePatch().Name}") is null)
		{
			NeuroActionHandler.UnregisterActions(new CodeWindowActions.CreateWindow());
			return;
		}
		List<INeuroAction> actions = new();
		
		if (WorkspaceState.Sim.researchMenu.allBoxes.Any())
			actions.Add(new DocsActions.GetDocumentation());
		
		actions.AddRange([new PatchActions.GetWindowCode(), new PatchActions.WritePatch(), new CodeWindowActions.CreateWindow()]);
		NeuroActionHandler.UnregisterActions(actions);
	}
}