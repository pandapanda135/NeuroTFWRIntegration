using System.Collections.Generic;
using System.Linq;
using EditParsing.Patching;

namespace NeuroTFWRIntegration.Utilities.Patching;

public static class PatchHelpers
{
	// we use parser in case we add any other formats in the future. 
	public static EditParsing.Patching.Parser GetParser(string patch, List<string>? files = null)
	{
		files ??= WorkspaceState.CurrentWorkspace.codeWindows.Select(kvp => kvp.Value.fileNameText.text).ToList();
		return new SearchParser(patch, files, WindowFileSystem.Open, WindowFileSystem.Write, WindowFileSystem.Delete);
	}
}