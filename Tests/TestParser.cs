using EditParsing.Patching;

namespace Tests;

public class TestParser
{
	[SetUp]
	public void Setup()
	{
	}
	
	public string Open(string path)
	{
		return "This is fake file contents";
	}

	public void Write(string path, string write)
	{
		return;
	}
	public void Delete(string path)
	{
		return;
	}
	
	private SearchParser GetSearchParser(string str, List<string> files)
	{
		return new SearchParser(str, files, Open, Write, Delete);
	}

	[Test]
	public void ValidJson()
	{
		var searchParser = GetSearchParser(TestingStrings.TestingStringOne, ["main", "second"]);
		Console.WriteLine($"testing string: {TestingStrings.TestingStringOne}");
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

		foreach (var patchAction in searchParser.Patch.Actions)
		{
			Console.WriteLine($"actions: {patchAction.Type}    new file contents:\n{patchAction.NewFile}");
		}
		Assert.Pass($"There were no issues when parsing TestingStringOne");
	}

	[Test]
	public void InvalidJson()
	{
		string failString = "This string should make it fail.\n" + TestingStrings.TestingStringOne;
		var searchParser = GetSearchParser(TestingStrings.TestingStringOne, ["main", "second"]);
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Assert.Pass($"This should be Index was out of range: {e}");
		}

		Console.WriteLine($"lines: {searchParser.IsValidPatch(TestingStrings.TestingStringOne, out string reason)}");
		Assert.Fail($"This should fail the parsing's test.");
	}
}