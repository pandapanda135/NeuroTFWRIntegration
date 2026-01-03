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
		bool startedBlock = false;
		// Is done and read string uses Lines which is window text not patch text. 
		while (!IsDone())
		{
			Logger.Info($"starting is done loop: {GetCurrentLine()}");
			// start
			if (!startedBlock && ReadString(StartPatch, out var next))
			{
				Logger.Info($"next line here : {next}");
				if (next != SearchPatch)
				{
					throw new ParsingException(
						$"You did not provide the search symbol after declaring a new file block.",
						ParsingErrors.ParsingIssue);
				}				Logger.Info($"start patch next line: {next}");
				
				// we check if the next is search as end and start are the same. 
				if (next == SearchPatch)
				{
					startedBlock = true;
					currentPath = GetCurrentLine(Index - 2);
					continue;
				}
			}

			var actions = InsideFileBlock(currentPath);
			if (actions.Count == 0)
			{
				throw new ParsingException("No actions could be made from the patch you provided.",
					ParsingErrors.ParsingIssue);
			}
			patch.Actions.AddRange(actions);

			// end
			if (ReadString(EndPatch, out var empty))
			{
				startedBlock = false;
				Logger.Info($"end patch was found: {empty}");
				
				while (!IsDone())
				{
					// we do it this way as to not need a specific amount of whitespace in between each block.
					var line = GetCurrentLine();
					Logger.Info($"other block currnet line: {line}");
					if (line != "")
					{
						if (!ValidFileNames.Contains(line))
						{
							throw new ParsingException(
								"You did not provide a valid file name for one of your file blocks.",
								ParsingErrors.InvalidFileName);
						}
						
						currentPath = line;
						break;
					}

					Index++;
				}
			}
			
			Logger.Info($"while increase index");
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

	private int CheckForSymbol(int startingIndex, string symbol)
	{
		var nextSymbolIndex = Lines.FindIndex(startingIndex,line => line == symbol);
		if (nextSymbolIndex != Lines.FindIndex(startingIndex, line => line.Contains(symbol)))
		{
			throw new ParsingException(
				"There was an issue trying to find the correct next symbol in the patch you provided," +
				" you may want to make sure you are correctly separating parts of the patch with new lines.",
				ParsingErrors.ParsingIssue);
		}

		return nextSymbolIndex;
	}

	private List<PatchAction> InsideFileBlock(string currentPath)
	{
		Logger.Info($"inside file block");
		List<PatchAction> actions = new();
		PatchAction currentAction = new(currentPath, ChangeType.Change);
		
		while (!IsDone())
		{
			// this is the text to search for.
			if (ReadString(SearchPatch, out _))
			{
				List<string> searchLines = [];
				var nextSymbolIndex = CheckForSymbol(Index, SeparatePatch);
				for (int i = Index; i < nextSymbolIndex; i++)
				{
					Logger.Info($"searching text lines: {GetCurrentLine()}");
					searchLines.Add(GetCurrentLine());
					Index++;
				}
				
				currentAction.SearchingString = string.Join("\n", searchLines);
				continue;
			}

			// separate patch, this is for the text to add.
			if (ReadString(SeparatePatch, out _))
			{
				List<string> lines = [];
				var nextSymbolIndex = CheckForSymbol(Index, ReplacePatch);
				for (int i = Index; i < nextSymbolIndex; i++)
				{
					Logger.Info($"replace text lines: {GetCurrentLine()}");
					lines.Add(GetCurrentLine());
					Index++;
				}
				
				currentAction.ReplaceString = string.Join("\n", lines);
				actions.Add(currentAction);
			}

			// replace patch
			if (ReadString(ReplacePatch, out var end))
			{
				if (end == EndPatch) break;
				
				if (!ReadString(SearchPatch, out var endPatchLine, Index + 1) || endPatchLine == "")
					throw new ParsingException("A valid end patch line was not provided after the replace symbols.",
						ParsingErrors.ParsingIssue);
				// should either go to next block or end parsing as action is already handled

				// Read string will increase index a second time and cause skipping parts of the search string if there are multiple actions.
				--Index;
				break;
			}

			Index++;
		}

		return actions;
	}
}