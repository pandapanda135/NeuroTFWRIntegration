using System.Collections.Concurrent;
using UnityEngine;

namespace NeuroTFWRIntegration.Utilities;

public static class WorkspaceState
{
	public static Workspace CurrentWorkspace => MainSim.Inst.workspace;

	public static ConcurrentDictionary<string, CodeWindow> CodeWindows => CurrentWorkspace.codeWindows;

	public static GameObject Object => MainSim.Inst.gameObject;
}