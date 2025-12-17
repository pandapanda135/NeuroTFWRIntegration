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

	public PatchAction(ChangeType type,string replaceString, string searchingString)
	{
		Type = type;
		ReplaceString = replaceString;
		SearchingString = searchingString;
	}
	
	public PatchAction(ChangeType type, List<Chunk> chunks, string searchingString)
	{
		Type = type;
		Chunks = chunks;
		SearchingString = searchingString;
	}

	public readonly ChangeType Type;

	/// <summary>
	/// This is what the new file will be
	/// </summary>
	public string NewFile = "";
	
	/// <summary>
	/// The string to search for in the file.
	/// </summary>
	public string SearchingString = "";
	
	/// <summary>
	/// The string that will replace the searching string.
	/// </summary>
	public string ReplaceString = "";

	public List<Chunk> Chunks = [];
}