using System.Collections.Generic;

namespace EditParsing.Patching;

public abstract class BasePatch
{
	public List<PatchAction> Actions = new();
}