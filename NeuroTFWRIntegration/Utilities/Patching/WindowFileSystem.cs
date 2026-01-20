using System.Linq;

namespace NeuroTFWRIntegration.Utilities.Patching;

public static class WindowFileSystem
{
	public static string Open(string path)
	{
		return WorkspaceState.CodeWindows.First(kvp => kvp.Value.fileNameText.text == path).Value.CodeInput.text;
	}
	
	public static void Write(string path, string content)
	{
		var codeWindow = WorkspaceState.CodeWindows.First(kvp => kvp.Value.fileNameText.text == path).Value;
		
		// I would use SetText, but I'd have to import more stuff and this works
		codeWindow.CodeInput.text = content;
	}

	public static void Delete(string path)
	{
		var codeWindow = WorkspaceState.CodeWindows.First(kvp => kvp.Value.fileNameText.text == path).Value;
		// this will make the pop-up appear, could maybe use toasts in the future.
		codeWindow.PromptDelete();
	}
}