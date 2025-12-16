using System.Collections.Generic;

namespace EditParsing.Patching;

/// <summary>
/// This is for what a patch should change, if the type is add or change you should use NewFile. If you use Delete you don't need to provide anything
/// </summary>
public class PatchAction
{
	public PatchAction(ChangeType type)
	{
		Type = type;
	}

	public PatchAction(ChangeType type, string newFile)
	{
		Type = type;
		NewFile = newFile;
	}

	public PatchAction(ChangeType type, List<Chunk> chunks)
	{
		Type = type;
		Chunks = chunks;
	}

	public ChangeType Type;

	/// <summary>
	/// This is what the new file will be
	/// </summary>
	public string NewFile = "";

	public List<Chunk> Chunks;
}