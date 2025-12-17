using NeuroSdk.Actions;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Actions;

public static class RegisterMainActions
{
	public static void RegisterMain()
	{
		// var actionWindow = ActionWindow.Create(WorkspaceState.Object);
		//
		// actionWindow.AddAction(new PatchActions.GetWindowCode()).AddAction(new PatchActions.WritePatch()).AddAction(new CodeWindowActions.CreateWindow());
		//
		// actionWindow.Register();
		
		NeuroActionHandler.RegisterActions(new PatchActions.GetWindowCode(), new PatchActions.WritePatch(), new CodeWindowActions.CreateWindow());
	}
}