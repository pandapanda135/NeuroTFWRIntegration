using System.Collections.Generic;

namespace EditParsing.Patching;

public class FileChanges
{
	public FileChanges(ChangeType type, string? oldContent = null, string? newContext = null, string? movePath = null)
	{
		Type = type;
		OldContent = oldContent;
		NewContext = newContext;
		MovePath = movePath;
	}

	public ChangeType Type;

	public string? OldContent;
	public string? NewContext;
	public string? MovePath;
}

public class Commit
{
	public Dictionary<string, FileChanges> Changes = new();
}