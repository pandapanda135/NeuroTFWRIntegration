using System.Collections.Generic;
using System.Linq;

namespace EditParsing.Patching;

/// <summary>
/// This is the parser for the format detailed here:
/// https://cookbook.openai.com/examples/gpt4-1_prompting_guide#other-effective-diff-formats
/// or here:
/// https://aider.chat/docs/more/edit-formats.html#diff
/// </summary>
public class SearchParser : Parser
{
	// example of what we want, window name may be changed to {window name}.py in case there are some issues with being trained on files and not working with this. 
	// {window name}
	// ```
	// <<<<<<< SEARCH
	// print("This is a test")
	// =======
	// return
	// print("This is a test")
	// >>>>>>> REPLACE
	// ```
	public SearchParser(string patchString, List<string> fileNames,
		FileHelpers.OpenFile open, FileHelpers.WriteFile write, FileHelpers.DeleteFile delete)
		: base(patchString, fileNames, open, write, delete)
	{
		StartPatch = "```";
		SearchPatch = "<<<<<<< SEARCH";
		SeparatePatch = "=======";
		ReplacePatch = ">>>>>>> REPLACE";
		EndPatch = "```";
	}

	protected override bool IsDone()
	{
		return Index >= Lines.Count;
	}

	public override Patch TextToPatch(string text)
	{
		Lines = GetLines(text);
		return ParseInputPatch();
	}

	// public override List<CodeWindow> GetWindows(string text) => WorkspaceHelpers.CurrentWorkspace.codeWindows.Select(kvp =>
	// 	kvp.Value).Where(window => text.Contains(window.fileName)).ToList();

	protected override string GetPatchFile(List<string> patchText)
	{
		// this should be the file name.
		return patchText[0];
	}

	protected override Patch ParseInputPatch()
	{
		// check if provided name is present
		if (!ValidFileNames.Contains(ModifiedFilePath))
		{
			// return new();
			throw new ParsingException("The provided file name was not valid.", ParsingErrors.InvalidFileName);
		}
		
		Logger.Info($"window name: {ModifiedFilePath}");

		Patch patch = new(ModifiedFilePath, []);
		PatchAction patchAction = new(ChangeType.Change);
		// Is done and read string uses Lines which is window text not patch text. 
		while (!IsDone())
		{
			Logger.Info($"starting is done loop: {CurrentLine}");
			// start
			if (ReadString(StartPatch, out var next))
			{
				// if this is not here, end patch will trigger here.
				if (next == SearchPatch) continue;
				
				// read string increases so we need to decrease for EndPatch.
				Index--;
			}

			// this is the text to search for.
			if (ReadString(SearchPatch, out _))
			{
				List<string> searchLines = [];
				var nextSymbolIndex = Lines.IndexOf(SeparatePatch);
				for (int i = Index; i < nextSymbolIndex; i++)
				{
					Logger.Info($"searching text lines: {CurrentLine}");
					searchLines.Add(CurrentLine);
					Index++;
				}
				
				patchAction.SearchingString = string.Join("\n", searchLines);
				continue;
			}

			// separate patch, this is for the text to add.
			if (ReadString(SeparatePatch, out _))
			{
				List<string> lines = [];
				var nextSymbolIndex = Lines.IndexOf(ReplacePatch);
				for (int i = Index; i < nextSymbolIndex; i++)
				{
					Logger.Info($"searching text lines: {CurrentLine}");
					lines.Add(CurrentLine);
					Index++;
				}
				
				patchAction.ReplaceString = string.Join("\n", lines);
				patch.Actions.Add(patchAction);
			}

			// replace patch
			if (ReadString(ReplacePatch, out var end))
			{
				if (end != EndPatch)
					throw new ParsingException("A valid end patch line was not provided after the replace symbols.",
						ParsingErrors.ParsingIssue);
				continue;
			}

			// end
			if (ReadString(EndPatch, out var empty))
			{
				Logger.Info($"end patch was found: {empty}");
				if (empty != "")
					throw new ParsingException("The patch continued after the end patch symbol",
						ParsingErrors.ParsingIssue);
				return patch;
			}

			Index++;
		}
		
		Logger.Info($"patch actions amount: {patch.Actions.Count}");
		return patch;
	}

	// this is for when changing file contents
	protected override void ParseUpdateFile(string text)
	{
		throw new System.NotImplementedException();
	}

	public Chunk ParseText(string text)
	{
		// Lines = text.Split("\n").ToList();
		// Plugin.Logger.LogInfo($"lines: {string.Join("\nNext line\n",Lines)}");
		// return new Chunk();
		return new(0, new(), new());
	}
}