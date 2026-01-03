namespace Tests;

public static class TestingStrings
{
	/// <summary>
	/// This is a standard file.
	/// </summary>
	public const string StandardFileContents =
		"from variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)";

	/// <summary>
	/// This is an empty file
	/// </summary>
	public const string EmptyFileContents = "";
	
	// this is the JSON compliant string you can test with.
	/// <summary>
	/// This is a standard patch with no issues.
	/// </summary>
	public const string StandardPatchString =
		"main\n```\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n\t\twhile get_pos_y() > 0:\n\t\t\tmove(South)\n>>>>>>> REPLACE\n```";

	/// <summary>
	/// This is a patch with an empty search block.
	/// </summary>
	public const string EmptySearchPatch =
		"main\\n```\\n<<<<<<< SEARCH\\n=======\\nfrom variable_values import max_pos\\nwhile True:\\n\\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n\t\twhile get_pos_y() > 0:\n\t\t\tmove(South)\n>>>>>>> REPLACE\n```";
	
	/// <summary>
	/// This is a patch with an empty replace block.
	/// </summary>
	public const string EmptyReplacePatch =
		"main\\n```\\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\n>>>>>>> REPLACE\\n```";
	
	public const string MultipleActionPatch =
		"main\n```\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n\t\twhile get_pos_y() > 0:\n\t\t\tmove(South)\n>>>>>>> REPLACE\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n=======\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n\t\twhile get_pos_y() > 0:\n\t\t\tmove(South)\n>>>>>>> REPLACE\n```";
	
	public const string MultipleFilePatch =
		"main\n```\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\nThis is the replace test.\n>>>>>>> REPLACE\n```\nsecond\n```\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\nThis is the replace test.\n>>>>>>> REPLACE\\n```";
}