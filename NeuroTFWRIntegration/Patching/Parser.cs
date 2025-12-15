using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeuroTFWRIntegration.Patching;

public enum ChangeType
{
	Add,
	Delete,
	Change
}

/// <summary>
/// This is intended as a base for creating patch formats and parsing said format, Applying the patch you create will be done for you.
/// </summary>
public abstract class Parser
{
	protected List<CodeWindow> CurrentWindows;

	#region Symbols

	/// <summary>
	/// This is for formats like the one featured in the OpenAI cookbook, where the file is specified like
	/// "*** Update File: file.py" rather than just the file name.
	/// </summary>
	protected string ModifiedFile;

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
	protected string ProvidedText;

	protected Patch Patch = new();

	/// <summary>
	/// This holds the provided patch's lines
	/// </summary>
	public List<string> Lines;

	/// <inheritdoc cref="GetCurrentLine"/>
	protected string CurrentLine => GetCurrentLine();

	protected int Index;

	// this is for getting the file that gets updated I think?????
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
			Logger.Info($"Index was out of bounds? {e}");
			line = "";
			return true;
		}

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
		var window = GetWindowFromPatch(patchString);
		if (window is null)
			throw new ParsingException("There was an issue getting the name of the file.",
				ParsingErrors.InvalidFileName);
		ModifiedFile = window.fileName;

		Lines = GetLines(patchString);
		try
		{
			ParseInputPatch();
		}
		catch (Exception e)
		{
			Logger.Error($"There was an error when parsing: {e}");
			throw;
		}
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

	public void Parse(string patchString)
	{
		try
		{
			ParsePatchString(patchString);
		}
		catch (Exception e)
		{
			Logger.Error($"There was an issue parsing: {e}");
			throw;
		}

		Commit commit = CreateCommitFromPatch(Patch);

		ApplyCommit(commit);
	}

	#endregion

	#region AbstractMethods

	protected abstract bool IsDone();

	// TODO: probably replace return value this with a Patch class
	public abstract Patch TextToPatch(string text);

	/// <summary>
	/// Get the windows that are mentioned in the patch
	/// </summary>
	/// <param name="text">The patch's text</param>
	/// <returns>The code windows targeted in the patch</returns>
	public abstract List<CodeWindow> GetWindows(string text);

	// get where in the file the patch is targeting to modify, IDK what to return rn 
	public abstract void GetOrigin(string text);

	public abstract void ParseInputPatch();

	protected abstract void ParseUpdateFile(string text);

	#endregion

	#region Window

	/// <summary>
	/// Split a string into it's individual lines.
	/// </summary>
	/// <returns></returns>
	private static List<string> GetLines(string text)
	{
		return text.Split("\n").ToList();
	}

	/// <inheritdoc cref="GetLines(string)"/>
	private static List<string> GetLines(CodeWindow window)
	{
		return GetLines(window.CodeInput.text);
	}

	private static CodeWindow GetWindowFromPatch(string patchString)
	{
		var patchLines = GetLines(patchString);
		CodeWindow window = null;
		foreach (var line in patchLines)
		{
			if (!MainSim.Inst.workspace.codeWindows.ContainsKey(line)) return null;

			var kvp = MainSim.Inst.workspace.codeWindows.First(kvp => kvp.Key == line);
			window = kvp.Value;
			if (window is not null) break;
		}

		if (window is null)
			throw new ParsingException("There was an issue getting the name of the file.",
				ParsingErrors.InvalidFileName);

		return window;
	}

	// This should get replaced with commit stuff
	// public abstract void ApplyPatch(string text, CodeWindow window);

	#endregion

	#region CommitCreation

	public Commit CreateCommitFromPatch(Patch patch)
	{
		return new Commit();
	}

	public void ApplyCommit(Commit commit)
	{
	}

	#endregion
}

public enum ParsingErrors
{
	InvalidFileName,
	ParsingIssue,
}

public class ParsingException(string message, ParsingErrors reasons) : Exception
{
	public override string Message { get; } = message;
	public override string StackTrace { get; } = StackTraceUtility.ExtractStackTrace();

	public ParsingErrors Reasons { get; } = reasons;
}