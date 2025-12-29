using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

public static class RegisterMainActions
{
	private static bool _mainRegistered;

	private static List<Type> _noBoxesActions = new();

	private static List<Type> _allActions = new();
	
	public static void RegisterMain()
	{
		if (_mainRegistered) return;
		_mainRegistered = true;
		if (!WorkspaceState.CodeWindows.Any())
		{
			NeuroActionHandler.RegisterActions(GetActions(_noBoxesActions));
			return;
		}
		
		// for some reason documentation requires closing and opening a menu after pressing play to populate schema,
		// IDK why IDK how to fix it, and I've spent way too much time trying to get it to work already
		var actions = GetActions(_allActions, action => action is DocsActions.GetDocumentation 
		                                                && !WorkspaceState.Sim.researchMenu.allBoxes.Any(box
			                                                => box.Value.unlockState is UnlockBox.UnlockState.Unlocked));

		NeuroActionHandler.RegisterActions(actions);
	}
	
	public static void UnregisterMain()
	{
		if (!_mainRegistered) return;
		_mainRegistered = false;
		
		// if there is only one or no code windows and all the mains are not registered.
		if (WorkspaceState.CodeWindows.Count < 2 && NeuroActionHandler.GetRegistered($"{new PatchActions.WritePatch().Name}") is null)
		{
			NeuroActionHandler.UnregisterActions(GetActions(_noBoxesActions));
			return;
		}
		
		// I believe this also includes actions registered via action windows
		var actions = GetActions(_allActions, action => NeuroActionHandler.GetRegistered(action.Name) is null);
		
		NeuroActionHandler.UnregisterActions(actions);
	}

	public static List<INeuroAction> GetActions(List<Type> actionTypes, Func<INeuroAction, bool>? customChecks = null)
	{
		List<INeuroAction> actions = new();
		foreach (var type in actionTypes)
		{
			if (Activator.CreateInstance(type) is not INeuroAction action) continue;

			Logger.Info($"test actions: {action.Name}  {customChecks != null && customChecks(action)}   any boxes: {WorkspaceState.Sim.researchMenu.allBoxes.Any(box => box.Value.unlockState is UnlockBox.UnlockState.Unlocked)}");
			if (customChecks is not null && customChecks(action)) continue;
			
			actions.Add(action);
		}

		return actions;
	}

	public static void PopulateActionLists()
	{
		_allActions.AddRange([
			typeof(PatchActions.GetWindowCode), typeof(PatchActions.WritePatch), typeof(CodeWindowActions.CreateWindow),
			typeof(QueryActions.QueryItems), typeof(DocsActions.GetDocumentation), typeof(DocsActions.GetNewDocumentation), typeof(QueryActions.QueryDrone),
			typeof(QueryActions.QueryWorld), typeof(QueryActions.QueryBuiltin)
		]);
		
		_noBoxesActions.AddRange([typeof(CodeWindowActions.CreateWindow), typeof(QueryActions.QueryItems),
			typeof(QueryActions.QueryDrone), typeof(QueryActions.QueryWorld), typeof(QueryActions.QueryBuiltin)]);
	}
}