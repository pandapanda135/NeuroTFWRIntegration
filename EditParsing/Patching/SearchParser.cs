using System.Collections.Generic;

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

	protected override string GetPatchFilePath(List<string> patchText)
	{
		// TODO: rewrite to support multiple patches, there is currently a workaround to this in the parser. So maybe we don't need to implement?
		// this should be the file name.
		return patchText[0];
	}

	protected override Patch ParseInputPatch()
	{
		// check if provided name is present
		if (!ValidFileNames.Contains(ModifiedFilePath))
		{
			throw new ParsingException($"The provided file name was not valid, you provided: {ModifiedFilePath}", ParsingErrors.InvalidFileName);
		}
		
		Logger.Info($"window name: {ModifiedFilePath}");

		string currentPath = ModifiedFilePath;
		Patch patch = new([]);
		PatchAction patchAction = new(currentPath, ChangeType.Change);
		// Is done and read string uses Lines which is window text not patch text. 
		while (!IsDone())
		{
			Logger.Info($"current path: {currentPath}");
			Logger.Info($"starting is done loop: {GetCurrentLine()}");
			// start
			if (ReadString(StartPatch, out var next))
			{
				Logger.Info($"start patch next line: {next}");
				// we check if the next is search as end and start are the same. We cannot check in if as we need to reduce index 
				if (next == SearchPatch)
				{
					// this gets the file
					currentPath = GetCurrentLine(Index - 2);
					patchAction = new(currentPath, ChangeType.Change);
					continue;
				}
				
				// read string increases so we need to decrease as start and end patch are the same.
				Index--;
			}

			// this is the text to search for.
			if (ReadString(SearchPatch, out var nextLine))
			{
				List<string> searchLines = [];
				Logger.Info($"next line thing: {nextLine}");
				var nextSymbolIndex = Lines.FindIndex(Index,line => line == SeparatePatch);
				for (int i = Index; i < nextSymbolIndex; i++)
				{
					Logger.Info($"searching text lines: {GetCurrentLine()}");
					searchLines.Add(GetCurrentLine());
					Index++;
				}
				
				patchAction.SearchingString = string.Join("\n", searchLines);
				Logger.Info($"searching string in parse input patch: {patchAction.SearchingString}");
				continue;
			}

			// separate patch, this is for the text to add.
			if (ReadString(SeparatePatch, out _))
			{
				List<string> lines = [];
				var nextSymbolIndex = Lines.FindIndex(Index,line => line == ReplacePatch);
				for (int i = Index; i < nextSymbolIndex; i++)
				{
					Logger.Info($"searching text lines: {GetCurrentLine()}");
					lines.Add(GetCurrentLine());
					Index++;
				}
				
				patchAction.ReplaceString = string.Join("\n", lines);
				patch.Actions.Add(patchAction);
			}

			// replace patch
			if (ReadString(ReplacePatch, out var end))
			{
				// TODO: we need to change this to be if there is an empty string then check for multiple patches for that file.
				
				// TODO: this will not work if there is an empty line after the end of the current patch separating the next and current patch.
				// TODO: IDK if that should be supported but I care enough to put this here.
				if (end != EndPatch)
				{
					if (!ReadString(SearchPatch, out var line, Index + 1) || line == "")
						throw new ParsingException("A valid end patch line was not provided after the replace symbols.",
							ParsingErrors.ParsingIssue);
					// should go to next block as action is already handled
				}
					
				continue;
			}

			// end
			if (ReadString(EndPatch, out var empty))
			{
				Logger.Info($"end patch was found: {empty}");
				// TODO: this is where we can check for multiple file blocks
				// if there are multiple file blocks they should be separated by an empty line so we need to skip by increasing.
				// we also need to get the new file name
				if (!IsDone() && !ReadString(StartPatch, out var line) && line == "")
					throw new ParsingException("The patch continued after the end patch symbol",
						ParsingErrors.ParsingIssue);
				
				// change this to not multiple
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