namespace EditParsing;

public static class FileHelpers
{
	/// <summary>
	/// This is for getting the contents of a file as a string.
	/// </summary>
	public delegate string OpenFile(string path);
	
	/// <summary>
	/// This is for both writing to a file and creating them.
	/// </summary>
	public delegate void WriteFile(string path, string content);
	
	/// <summary>
	/// This is for deleting a file.
	/// </summary>
	public delegate void DeleteFile(string path);
}