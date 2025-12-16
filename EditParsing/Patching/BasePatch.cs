using System.Collections.Generic;

namespace EditParsing.Patching;

public abstract class BasePatch(string path, List<PatchAction>? actions = null)
{
	public string Path = path;

	public List<PatchAction> Actions = actions ?? [];
}