namespace Tests;

public static class TestingStrings
{
	public const string TestFileContentsOne =
		"from variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)";
	
	// this is the JSON compliant string you can test with.
	public const string TestingStringOne =
		"main\n```\n<<<<<<< SEARCH\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n=======\nfrom variable_values import max_pos\nwhile True:\n\tmove(North)\n\tif can_harvest():\n\t\tharvest()\n\tif get_pos_y() == max_pos:\n\t\tmove(East)\n\t\twhile get_pos_y() > 0:\n\t\t\tmove(South)\n>>>>>>> REPLACE\n```";
}