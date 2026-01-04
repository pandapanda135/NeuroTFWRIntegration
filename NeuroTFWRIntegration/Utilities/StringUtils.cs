using System.Text.RegularExpressions;

namespace NeuroTFWRIntegration.Utilities;

public static class StringUtils
{
	public static string RemoveTextMeshTags(string text)
	{
		return Regex.Replace(text, "<.*?>", string.Empty);
	}
}