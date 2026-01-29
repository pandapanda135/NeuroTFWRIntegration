using System;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Utilities;
namespace NeuroTFWRIntegration.Actions;

public static class RegisterMainActions
{
	private static bool _mainRegistered;

	private static readonly List<Type> NoWindowsActions = new();

	private static readonly List<Type> AllActions = new();
	
	public static void RegisterMain()
	{
		if (_mainRegistered) return;
		_mainRegistered = true;
		if (!WorkspaceState.CodeWindows.Any())
		{
			NeuroActionHandler.RegisterActions(CreateActionClasses(NoWindowsActions));
			return;
		}
		
		var actions = CreateActionClasses(AllActions);

		NeuroActionHandler.RegisterActions(actions);
	}
	
	public static void UnregisterMain()
	{
		if (!_mainRegistered) return;
		_mainRegistered = false;
		
		// if there is only one or no code windows and the main actions are not registered.
		if (WorkspaceState.CodeWindows.Count < 2 && NeuroActionHandler.GetRegistered($"{new PatchActions.WritePatch().Name}") is null)
		{
			NeuroActionHandler.UnregisterActions(CreateActionClasses(NoWindowsActions));
			return;
		}
		
		// I believe this also includes actions registered via action windows
		var actions = CreateActionClasses(AllActions, action => NeuroActionHandler.GetRegistered(action.Name) is null);
		
		NeuroActionHandler.UnregisterActions(actions);
	}

	private static List<INeuroAction> CreateActionClasses(List<Type> actionTypes, Func<INeuroAction, bool>? customChecks = null)
	{
		List<INeuroAction> actions = new();
		foreach (var type in actionTypes)
		{
			if (Activator.CreateInstance(type) is not INeuroAction action) continue;

			if (customChecks is not null && customChecks(action)) continue;
			
			actions.Add(action);
		}

		return actions;
	}

	public static void PopulateActionLists()
	{
		AllActions.AddRange([
			typeof(PatchActions.GetWindowCode), typeof(PatchActions.WritePatch), typeof(CodeWindowActions.CreateWindow),
			typeof(QueryActions.QueryItems), typeof(DocsActions.GetDocumentation), typeof(QueryActions.QueryDrone),
			typeof(QueryActions.QueryWorld), typeof(QueryActions.QueryBuiltin)
		]);
		
		NoWindowsActions.AddRange([typeof(CodeWindowActions.CreateWindow), typeof(QueryActions.QueryItems),
			typeof(QueryActions.QueryDrone), typeof(QueryActions.QueryWorld), typeof(QueryActions.QueryBuiltin),
		]);

		if (ConfigHandler.Toasts.Entry.Value == Toasts.All)
		{
			AllActions.Add(typeof(ToastActions.CreateToast));
			NoWindowsActions.Add(typeof(ToastActions.CreateToast));
		}

		if (ConfigHandler.ResearchMenuActions.Entry.Value != ResearchMenuActions.OutOfMenu) return;
		
		AllActions.AddRange([typeof(ResearchActions.QueryUpgrades), typeof(ResearchActions.BuyUpgrade)]);
		NoWindowsActions.AddRange([typeof(ResearchActions.QueryUpgrades), typeof(ResearchActions.BuyUpgrade)]);
	}
}