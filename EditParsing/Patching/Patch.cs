using System.Collections.Generic;

namespace EditParsing.Patching;

public class Patch(List<PatchAction>? actions = null) : BasePatch(actions)
{
}