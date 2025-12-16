using System.Collections.Generic;

namespace EditParsing.Patching;

public class Patch(string path, List<PatchAction>? actions = null) : BasePatch(path, actions)
{
}