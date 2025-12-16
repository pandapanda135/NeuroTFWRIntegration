using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
		ProvidedPatchString = providedPatchString;
		ValidFileNames = fileNames;
		
		_openFile = openFile;
		_writeFile = writeFile;
		_deleteFile = deleteFile;

	}
	// protected List<CodeWindow> CurrentWindows;

	private readonly FileHelpers.OpenFile _openFile;
	private readonly FileHelpers.WriteFile _writeFile;
	private readonly FileHelpers.DeleteFile _deleteFile;
	

	#region Symbols

	protected List<string> ValidFileNames;
	/// <summary>
	/// This is for the path sent in a patch.
	/// </summary>
	protected string ModifiedFilePath;

	protected string StartPatch;
	protected string SeparatePatch;
	protected string EndPatch;

	// yes the key code is ugly but it works
	/// <summary>
	/// The part of file to look for. in search and replace this would be "&lt;&lt;&lt;&lt;&lt;&lt; SEARCH"
	/// </summary>
	protected string SearchPatch;

	/// <summary>
	/// The end of replacing, in search and replace this would be ">>>>>>> REPLACE"
	/// </summary>
	protected string ReplacePatch;

	// these will not be used in search replace format.
	protected string NewLineSymbol;
	protected string RemoveLineSymbol;

	#endregion

	/// <summary>
	/// This is the patch sent as it's raw text
	/// </summary>
	protected readonly string ProvidedPatchString;

	public Patch Patch = new("",[]);

	/// <summary>
	/// This holds the provided patch's lines
	/// </summary>
	public List<string> Lines = new();

	/// <inheritdoc cref="GetCurrentLine"/>
	protected string CurrentLine => GetCurrentLine();

	protected int Index;

	// this is for getting the file that gets updated I think?????
	// maybe don't need and can remove?
	public string GetUpdatedFile(string fileText, Patch patch)
	{
		return "";
	}

	#region ImplementedHelpers

	/// <summary>
	/// This will get the current line
	/// </summary>
	/// <returns>This will return the current line according to <see cref="Index"/> and <see cref="Lines"/>. If the index is greater than the amount of lines, an exception will happen.</returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	private string GetCurrentLine()
	{
		if (Index > Lines.Count) throw new IndexOutOfRangeException();
		return Lines[Index];
	}

	/// <summary>
	/// Get the current line and check if it contains a patch format e.g. <see cref="StartPatch"/>. If it does, it will advance the index and return the next line.
	/// </summary>
	/// <returns>Will return true if the format is specified else false. If the next string is out of bounds, then line will be an empty string</returns>
	protected bool ReadString(string patchFormat, out string line)
	{
		line = "";
		if (!CurrentLine.Contains(patchFormat)) return false;

		Index++;
		// could throw an error if index is out of bounds
		try
		{
			line = GetCurrentLine();
		}
		catch (Exception e)
		{
			Logger.Error($"Index was out of bounds? {e}");
			line = "";
			// return true;
			throw;
		}
		Logger.Info($"read string line: {line}");

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

	private void ParsePatchString(string patchString)
	{
		// this is for testing provided file name
		// var window = GetWindowFromPatch(patchString);
		// if (window is null)
		// 	throw new ParsingException("There was an issue getting the name of the file.",
		// 		ParsingErrors.InvalidFileName);
		
		Lines = GetLines(patchString);
		ModifiedFilePath = GetPatchFile(Lines);
		if (string.IsNullOrEmpty(ModifiedFilePath))
		{
			throw new ParsingException(
				$"There was an issue with getting the patch's file, this is what was received: {ModifiedFilePath}",
				ParsingErrors.InvalidFileName);
		}
		
		try
		{
			Patch = ParseInputPatch();
		}
		catch (Exception e)
		{
			Logger.Error($"There was an error when parsing: {e}");
			throw;
		}
	}

	#endregion

	#region AbstractMethods
	
	/// <summary>
	/// Check if the current index is the max possible
	/// </summary>
	protected abstract bool IsDone();

	public abstract Patch TextToPatch(string text);
	
	/// <summary>
	/// Get the windows that are mentioned in the patch
	/// </summary>
	/// <param name="text">The patch's text</param>
	/// <returns>The code windows targeted in the patch</returns>
	// public abstract List<CodeWindow> GetWindows(string text);
	
	/// <summary>
	/// Return where the patch wants to modify
	/// </summary>
	/// <param name="patchText"></param>
	/// <returns>This should return the name of the file that it comes from.</returns>
	protected abstract string GetPatchFile(List<string> patchText);

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

		Commit commit = CreateCommitFromPatch(Patch);

		ApplyCommit(commit);
	}
	
	
	public bool IsValidPatch(string patchString, out string reason)
	{
		try
		{
			ParsePatchString(patchString);
		}
		catch (Exception e)
		{
			Logger.Error($"Invalid patch string: {e}");
			reason = e.Message;
			return false;
		}

		reason = "";
		return true;
	}
	
	#endregion
	
	#region Window

	/// <summary>
	/// Split a string into it's individual lines.
	/// </summary>
	/// <returns></returns>
	protected static List<string> GetLines(string text)
	{
		return text.Split("\n").ToList();
	}

	#endregion

	#region CommitCreation

	private Commit CreateCommitFromPatch(Patch patch)
	{
		Commit commit = new();

		foreach (var action in patch.Actions)
		{
			var fileChanges = new FileChanges(action.Type);
			switch (action.Type)
			{
				case ChangeType.Add:
					fileChanges.NewContext = action.NewFile;
					break;
				case ChangeType.Delete:
					fileChanges.OldContent = _openFile(patch.Path);
					break;
				case ChangeType.Change:
					fileChanges.NewContext = action.NewFile;
					fileChanges.OldContent = _openFile(patch.Path);
					break;
				case ChangeType.Move:
					fileChanges.MovePath = patch.Path;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(commit), "Commits can only apply the types valid in ChangeType.");
			}
			
			commit.Changes.Add(ModifiedFilePath, fileChanges);
		}
		
		return commit;
	}

	private void ApplyCommit(Commit commit)
	{
		foreach (var change in commit.Changes)
		{
			string path = change.Key;

			switch (change.Value.Type)
			{
				case ChangeType.Add:
					_writeFile(path, change.Value.NewContext);
					break;
				case ChangeType.Change:
					_writeFile(path, change.Value.NewContext);
					break;
				case ChangeType.Delete:
					_deleteFile(path);
					break;
				case ChangeType.Move:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(commit), "Commits can only apply the types in ChangeType.");
			}
		}
	}

	#endregion
}

public enum ParsingErrors
{
	InvalidFileName,
	ParsingIssue,
}

public class ParsingException(string message, ParsingErrors reason) : Exception
{
	public override string Message { get; } = message;
	public override string StackTrace { get; } = new System.Diagnostics.StackTrace().ToString();

	public ParsingErrors Reason { get; } = reason;
}