using EditParsing.Patching;

namespace Tests;

public class TestParser
{
	[SetUp]
	public void Setup()
	{
	}
	
	private SearchParser GetSearchParser(string str, List<string> files, string fileContents)
	{
		var fileHelper = new FileSystemHelper(fileContents);
		return new SearchParser(str, files, fileHelper.Open, fileHelper.Write, fileHelper.Delete);
	}

	#region SearchParser
	[Test]
	[Category(nameof(SearchParser))]
	public void ValidJson()
	{
		var searchParser = GetSearchParser(TestingStrings.StandardPatchString, ["main", "second"], TestingStrings.StandardFileContents);
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Assert.Fail($"There was an exception caught when running parsing: {e}");
		}

		foreach (var patchAction in searchParser.Patch.Actions)
		{
			Console.WriteLine($"actions: {patchAction.Type}    new file contents:\n{patchAction.NewFile}");
		}
		Assert.Pass($"There were no issues when parsing TestingStringOne");
	}

	[Test(Description = "This is meant to fail, the console should display an index out of range exception.")]
	[Category(nameof(SearchParser))]
	public void InvalidJson()
	{
		string failString = "This string should make it fail.\n" + TestingStrings.StandardPatchString;
		var searchParser = GetSearchParser(failString, ["main", "second"], TestingStrings.StandardFileContents);
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Assert.Pass($"This should be Index was out of range: {e}");
		}
		
		Assert.Fail($"This should not be a valid test.");
	}
	
	[Test]
	[Category(nameof(SearchParser))]
	public void EmptySearchTest()
	{
		var searchParser = GetSearchParser(TestingStrings.EmptySearchPatch, ["main", "second"], TestingStrings.EmptyFileContents);
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Assert.Fail($"There was an exception caught when running parsing: {e}");
		}

		foreach (var patchAction in searchParser.Patch.Actions)
		{
			Console.WriteLine($"actions: {patchAction.Type}    new file contents:\n{patchAction.NewFile}");
		}
		Assert.Pass($"There were no issues when parsing with an empty search string.");
	}

	[Test]
	[Category(nameof(SearchParser))]
	public void EmptyReplaceTest()
	{
		var searchParser = GetSearchParser(TestingStrings.EmptyReplacePatch, ["main", "second"], TestingStrings.StandardFileContents);
		
		try
		{
			searchParser.Parse();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Assert.Fail($"There was an exception caught when running parsing: {e}");
		}

		foreach (var patchAction in searchParser.Patch.Actions)
		{
			Console.WriteLine($"actions: {patchAction.Type}    new file contents:\n{patchAction.NewFile}");
		}
		Assert.Pass($"There were no issues when parsing with an empty replace string.");
	}
	
	#endregion
}