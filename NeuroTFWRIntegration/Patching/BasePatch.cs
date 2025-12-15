using System.Collections.Generic;

namespace NeuroTFWRIntegration.Patching;

public abstract class BasePatch
{
	public List<PatchAction> Actions = new();
}