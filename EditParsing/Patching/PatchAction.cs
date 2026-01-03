using System.Collections.Generic;

namespace EditParsing.Patching;

/// <summary>
/// This is for what a patch should change, if the type is add or change you should use NewFile. If you use Delete you don't need to provide anything
/// </summary>
public class PatchAction
{
	public PatchAction(string path, ChangeType type)
	{
		Path = path;
		Type = type;
	}

	public PatchAction(string path, ChangeType type,string replaceString, string searchingString)
	{
		Path = path;
		Type = type;
		ReplaceString = replaceString;
		SearchingString = searchingString;
	}
	
	public PatchAction(string path ,ChangeType type, List<Chunk> chunks, string searchingString)
	{
		Path = path;
		Type = type;
		Chunks = chunks;
		SearchingString = searchingString;
	}

	public readonly ChangeType Type;

	public string Path;
	
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