using System;
using System.Collections.Generic;
using System.Linq;

namespace EditParsing.Patching;

public enum ChangeType
{
	/// <summary>
	/// This is for adding new files, this is not used by SearchParser.
	/// </summary>
	Add,
	/// <summary>
	/// This is for deleting files, this is not used by SearchParser
	/// </summary>
	Delete,
	/// <summary>
	/// This is for changing existing files.
	/// </summary>
	Change,
	/// <summary>
	/// This is for when you want to move a file to a new directory.
	/// </summary>
	Move,
}

/// <summary>
/// This is intended as a base for creating patch formats and parsing said format, Applying the patch you create will be done for you.
/// </summary>
public abstract class Parser
{
 	protected Parser(string providedPatchString, List<string> fileNames, FileHelpers.OpenFile openFile, FileHelpers.WriteFile writeFile, FileHelpers.DeleteFile deleteFile)
	{
		ProvidedPatchString = FormatPatchString(providedPatchString);
		ValidFileNames = fileNames;
		
		_openFile = openFile;
		_writeFile = writeFile;
		_deleteFile = deleteFile;
	}
	// protected List<CodeWindow> CurrentWindows;

	private static string FormatPatchString(string patch)
	{
		if (!patch.Contains("\\n") && !patch.Contains("\\t")) return patch;
		patch = patch.Replace("\\n", "\n");
		return patch.Replace("\\t", "\t");
	}

	private readonly FileHelpers.OpenFile _openFile;
	private readonly FileHelpers.WriteFile _writeFile;
	private readonly FileHelpers.DeleteFile _deleteFile;
	

	#region Symbols
	
	protected string StartPatch = "";
	protected string SeparatePatch = "";
	protected string EndPatch = "";

	// yes the key code is ugly but it works
	/// <summary>
	/// The part of file to look for. in search and replace this would be "&lt;&lt;&lt;&lt;&lt;&lt; SEARCH"
	/// </summary>
	protected string SearchPatch = "";

	/// <summary>
	/// The end of replacing, in search and replace this would be ">>>>>>> REPLACE"
	/// </summary>
	protected string ReplacePatch = "";

	// these will not be used in search replace format.
	protected string NewLineSymbol = "";
	protected string RemoveLineSymbol = "";

	#endregion

	/// <summary>
	/// This is the patch sent as it's raw text
	/// </summary>
	protected readonly string ProvidedPatchString;

	public Patch Patch = new([]);

	/// <summary>
	/// This holds the provided patch's lines
	/// </summary>
	public List<string> Lines = new();

	/// <inheritdoc cref="GetCurrentLine"/>
	
	protected readonly List<string> ValidFileNames;
	/// <summary>
	/// This is for the path sent in a patch.
	/// </summary>
	protected string ModifiedFilePath = "";

	protected int Index;

	#region ImplementedHelpers

	/// <summary>
	/// This will get the current line
	/// </summary>
	/// <returns>This will return the current line according to <see cref="Index"/> and <see cref="Lines"/>. If the index is greater than the amount of lines, an exception will happen.</returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	protected string GetCurrentLine(int index = -1)
	{
		index = index == -1 ? Index : index;
		
		if (index >= Lines.Count) throw new IndexOutOfRangeException("The parser's index was greater than the line count.");
		return Lines[index];
	}

	/// <summary>
	/// Get the current line and check if it contains a patch format e.g. <see cref="StartPatch"/>. If it does, it will advance the index and return the next line.
	/// </summary>
	/// <param name="patchFormat">This is the part of the format that you want to look for.</param>
	/// <param name="nextLine">This is the next line after the patch format, this next line is also the current line.</param>
	/// <param name="index">This is for if you want to look for an arbitrary line in Lines and get it's next line, if this is any value that is not -1 it will be set to Index</param>
	/// <returns>Will return true if the format is specified else false. If the next string is out of bounds, then line will be an empty string</returns>
	protected bool ReadString(string patchFormat, out string nextLine, int index = -1)
	{
		index = index == -1 ? Index : index;
		nextLine = "";
		if (!GetCurrentLine().Contains(patchFormat)) return false;

		string currentLine = GetCurrentLine(index);
		if (index == Index) Index++;
		// This is not be a reference of Index so we don't just increase this in an else.
		index++;
		
		// could throw an error if index is out of bounds
		try
		{
			nextLine = GetCurrentLine(index);
		}
		catch (Exception e)
		{
			nextLine = "";
			// This could cause issues with formats other than SearchParser, but it is the only decent solution I can think of right now. 
			if (currentLine == EndPatch)
				return true;
			
			Logger.Error($"Index was out of bounds {e}");
			throw;
		}
		
		Logger.Info($"read string line: {nextLine}");
		return true;
	}

	/// <summary>
	/// Removes all the trailing characters at the start of a line. 
	/// </summary>
	protected string RemoveTrailing(string line)
	{
		return line.TrimStart();
	}

	/// <summary>
	/// Returns the current line then advances the index 
	/// </summary>
	/// <returns></returns>
	protected string ReadLine()
	{
		return Lines[Index++];
	}
	
	/// <summary>
	/// Split a string into it's individual lines.
	/// </summary>
	/// <returns></returns>
	protected static List<string> GetLines(string text)
	{
		return text.Split("\n").ToList();
	}

	private void ParsePatchString(string patchString)
	{
		Lines = GetLines(patchString);
		ModifiedFilePath = GetPatchFilePath(Lines);
		
		if (string.IsNullOrEmpty(ModifiedFilePath) || !ValidFileNames.Contains(ModifiedFilePath))
		{
			throw new ParsingException(
				$"There was an issue with getting the patch's file, this is what was received: {ModifiedFilePath}",
				ParsingErrors.InvalidFileName);
		}
		Logger.Info($"modified file path: {ModifiedFilePath}");
		
		try
		{
			Patch = ParseInputPatch();
		}
		catch (Exception e)
		{
			Logger.Error($"There was an error when parsing: {e}");
			throw;
		}
		
		if (Patch.Actions.Any(action => !_openFile(ModifiedFilePath).Contains(action.SearchingString)))
		{
			throw new ParsingException($"{ModifiedFilePath} does not contain the contents you are searching for.",
				ParsingErrors.InvalidFileName);
		}
	}

	/// <summary>
	/// This will add the whole corrected file to the patch's NewFile.
	/// </summary>
	/// <param name="patch">The patch you want to modify.</param>
	private Patch CreatePatchNewFile(Patch patch)
	{
		foreach (var action in patch.Actions)
		{
			string fileString = _openFile(action.Path);

			// we replace whole file if searching is not defined
			if (action.SearchingString == string.Empty)
			{
				action.NewFile = action.ReplaceString;
				Logger.Info($"action new file empty search: {action.NewFile}     searching file: {action.SearchingString}");
				continue;
			}
			action.NewFile = fileString.Replace(action.SearchingString, action.ReplaceString);
			Logger.Info($"action new file: {action.NewFile}");
		}

		return patch;
	}

	#endregion

	#region AbstractMethods
	
	/// <summary>
	/// Check if the current index is the max possible
	/// </summary>
	protected abstract bool IsDone();

	public abstract Patch TextToPatch(string text);
	
	/// <summary>
	/// Return where the patch wants to modify
	/// </summary>
	/// <param name="patchText"></param>
	/// <returns>This should return the name of the file that it comes from.</returns>
	protected abstract string GetPatchFilePath(List<string> patchText);

	/// <summary>
	/// This is meant to parse the patch that was input and create a <see cref="BasePatch"/> that includes those actions.
	/// </summary>
	/// <returns>This should return the patch you created, if you use <see cref="Parse"/> the parser's <see cref="Patch"/> will be set for you.</returns>
	protected abstract Patch ParseInputPatch();

	protected abstract void ParseUpdateFile(string text);

	#endregion
	
	#region PublicEntryPoints
	
	public void Parse()
	{
		try
		{
			ParsePatchString(ProvidedPatchString);
		}
		catch (Exception e)
		{
			Logger.Error($"There was an issue parsing: {e}");
			throw;
		}
		
		Patch = CreatePatchNewFile(Patch);

		Commit commit = CreateCommitFromPatch(Patch);

		ApplyCommit(commit);
	}
	
	public bool IsValidPatch(string patchString, out string failureReason)
	{
		try
		{
			ParsePatchString(patchString);
		}
		catch (Exception e)
		{
			Logger.Error($"Invalid patch string: {e}");
			failureReason = e.Message;
			return false;
		}

		failureReason = "";
		return true;
	}
	
	#endregion
	
	#region CommitCreation

	private Commit CreateCommitFromPatch(Patch patch)
	{
		Commit commit = new();

		foreach (var action in patch.Actions)
		{
			Logger.Info($"action path: {action.Path}");
			var fileChanges = new FileChanges(action.Type);
			switch (action.Type)
			{
				case ChangeType.Add:
					fileChanges.NewContext = action.NewFile;
					break;
				case ChangeType.Delete:
					fileChanges.OldContent = _openFile(action.Path);
					break;
				case ChangeType.Change:
					fileChanges.NewContext = action.NewFile;
					fileChanges.OldContent = _openFile(action.Path);
					break;
				case ChangeType.Move:
					fileChanges.MovePath = action.Path;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(commit), "Commits can only apply the types valid in ChangeType.");
			}

			if (commit.Changes.ContainsKey(action.Path)) commit.Changes[action.Path].Add(fileChanges);
			else
			{
				if (commit.Changes.TryAdd(action.Path, [fileChanges])) continue;
				
				throw new ParsingException("Tried to add multiple commit changes that have the same key.",
					ParsingErrors.CommitIssue);
			}
		}
		
		return commit;
	}

	private void ApplyCommit(Commit commit)
	{
		foreach (var commitKvp in commit.Changes)
		{
			string path = commitKvp.Key;
			if (string.IsNullOrEmpty(path))
				throw new ParsingException("The path for this commit was either empty or null",ParsingErrors.MissingCommitContent);

			foreach (var change in commitKvp.Value)
			{
				switch (change.Type)
				{
					case ChangeType.Add:
					{
						// we do this to support patch formats that don't allow for setting a file's contents while creating it.
						_writeFile(path, change.NewContext ?? "");
						break;
					}
					case ChangeType.Change:
					{
						if (change.NewContext is null)
							throw new ParsingException("The new context was missing from this commit.",ParsingErrors.MissingCommitContent);
						_writeFile(path, change.NewContext);
						break;
					}
					case ChangeType.Delete:
					{
						_deleteFile(path);
						break;
					}
					case ChangeType.Move:
					{
						if (change.MovePath is null)
							throw new ParsingException($"The move path was not set when wanting to move {path}.",
								ParsingErrors.MissingCommitContent);
						string fileContent = _openFile(path);

						_deleteFile(path);
						_writeFile(change.MovePath, fileContent);
						break;
					}
					default:
						throw new ArgumentOutOfRangeException(nameof(commit), "Commits can only apply the types in ChangeType.");
				}
			}
		}
	}

	#endregion
}

public enum ParsingErrors
{
	InvalidFileName,
	ParsingIssue,
	MissingCommitContent,
	CommitIssue
}

public class ParsingException(string message, ParsingErrors reason) : Exception
{
	public override string Message { get; } = message;
	public override string StackTrace { get; } = new System.Diagnostics.StackTrace().ToString();

	public ParsingErrors Reason { get; } = reason;
}