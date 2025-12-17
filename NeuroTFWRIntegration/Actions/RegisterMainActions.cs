using System.Linq;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Actions;

public static class RegisterMainActions
{
	private static bool _mainRegistered;
	
	public static void RegisterMain()
	{
		if (_mainRegistered || !WorkspaceState.CodeWindows.Any()) return;
		// var actionWindow = ActionWindow.Create(WorkspaceState.Object);
		//
		// actionWindow.AddAction(new PatchActions.GetWindowCode()).AddAction(new PatchActions.WritePatch()).AddAction(new CodeWindowActions.CreateWindow());
		//
		// actionWindow.Register();

		_mainRegistered = true;
		NeuroActionHandler.RegisterActions(new PatchActions.GetWindowCode(), new PatchActions.WritePatch(), new CodeWindowActions.CreateWindow());
	}
	
	public static void UnregisterMain()
	{
		if (!_mainRegistered) return;
		
		_mainRegistered = false;
		NeuroActionHandler.UnregisterActions(new PatchActions.GetWindowCode(), new PatchActions.WritePatch(), new CodeWindowActions.CreateWindow());
	}
}