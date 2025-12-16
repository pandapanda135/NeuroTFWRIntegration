using System.Collections.Generic;

namespace EditParsing.Patching;

public interface IChunk
{
	public int Index { get; set; }

	public List<string> InsertLines { get; set; }

	public List<string> RemoveLines { get; set; }
}