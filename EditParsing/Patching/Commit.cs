using System.Collections.Generic;

namespace EditParsing.Patching;

public class FileChanges
{
	public ChangeType Type;

	public string OldContent;
	public string NewContext;
	public string MovePath;
}

public class Commit
{
	public Dictionary<string, FileChanges> Changes;
}