using System.Collections.Generic;

namespace EditParsing.Patching;

public abstract class BasePatch(List<PatchAction>? actions = null)
{
	public List<PatchAction> Actions = actions ?? [];
}