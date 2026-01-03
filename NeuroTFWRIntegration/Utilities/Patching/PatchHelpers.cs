using System.Collections.Generic;
using System.Linq;
using EditParsing.Patching;

namespace NeuroTFWRIntegration.Utilities.Patching;

public static class PatchHelpers
{
	#region IngameTestingStrings

	private const string MultiActionPatchTest =
		"main\n```\n<<<<<<< SEARCH\n\n=======\nthis is the first patch test.\n>>>>>>> REPLACE\n<<<<<<< SEARCH\nsecond patch test.\n=======\nthis is the second patch test.\n>>>>>>> REPLACE\n```";

	private const string MultiWindowPatchTest =
		"main\n```\n<<<<<<< SEARCH\n\n=======\nThis is the replace test.\n>>>>>>> REPLACE\n```\nsecond\n```\n<<<<<<< SEARCH\n\n=======\nThis is the replace test.\n>>>>>>> REPLACE\\n```";

	#endregion
	
	// we use parser in case we add any other formats in the future. 
	public static EditParsing.Patching.Parser GetParser(string patch, List<string>? files = null)
	{
		files ??= WorkspaceState.CurrentWorkspace.codeWindows.Select(kvp => kvp.Value.fileNameText.text).ToList();
		return new SearchParser(patch, files, WindowFileSystem.Open, WindowFileSystem.Write, WindowFileSystem.Delete);
	}
}