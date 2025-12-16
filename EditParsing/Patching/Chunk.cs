using System.Collections.Generic;

namespace EditParsing.Patching;

public class Chunk : IChunk
{
	public Chunk(int index, List<string> insertLines, List<string> removeLines)
	{
		Index = index;
		InsertLines = insertLines;
		RemoveLines = removeLines;
	}

	public int Index { get; set; }

	public List<string> InsertLines { get; set; }

	public List<string> RemoveLines { get; set; }
}