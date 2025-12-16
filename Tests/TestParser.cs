using EditParsing.Patching;
using NeuroTFWRIntegration;
using UnityEngine.PlayerLoop;

namespace Tests;

public class TestParser
{
	[SetUp]
	public void Setup()
	{
	}
	
	#if TESTING
		
		// [HarmonyPatch(typeof(NeuroSdk.NeuroSdkSetup), nameof(NeuroSdk.NeuroSdkSetup.ModuleInitializer))]
		// [HarmonyPrefix]
		// static bool SetupPrefix()
		// {
		// 	return false;
		// }
		
	#endif

	[Test]
	public void ValidJson()
	{
		var searchParser = new SearchParser(PatchStrings.TestingString, ["main", "second"]);
		Console.WriteLine($"testing string: {PatchStrings.TestingString}");
		Console.WriteLine($"personal lines: {searchParser.Lines.Count}");
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Assert.Fail(e.ToString());
		}

		Console.WriteLine($"lines: {searchParser.IsValidPatch(PatchStrings.TestingString, out string reason)}");
		Assert.Pass();
	}

	[Test]
	public void InvalidJson()
	{
		string failString = "This string should make it fail.\n" + PatchStrings.TestingString;
		var searchParser = new SearchParser(failString, ["main", "second"]);
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Assert.Pass($"This should be Index was out of range: {e}");
		}

		Console.WriteLine($"lines: {searchParser.IsValidPatch(PatchStrings.TestingString, out string reason)}");
		Assert.Fail($"This should fail the parsing's test.");
	}
}