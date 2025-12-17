using System.Linq;

namespace NeuroTFWRIntegration.Utilities.Patching;

public static class WindowFileSystem
{
	public static string Open(string path)
	{
		return MainSim.Inst.workspace.codeWindows.First(kvp => kvp.Key == path).Value.CodeInput.text;
	}
	
	public static void Write(string path, string content)
	{
		var codeWindow = MainSim.Inst.workspace.codeWindows.First(kvp => kvp.Key == path).Value;
		
		// I would use SetText, but I'd have to import more stuff and this works
		codeWindow.CodeInput.text = content;
	}

	public static void Delete(string path)
	{
		// we can return as we won't use this yet and I don't really know how to implement.
	}
}