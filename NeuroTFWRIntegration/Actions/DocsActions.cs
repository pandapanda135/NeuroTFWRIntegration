using System.Collections.Generic;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;

namespace NeuroTFWRIntegration.Actions;

public static class DocsActions
{
	public class GetDocumentation : NeuroAction<string>
	{
		public override string Name => "get_documentation";
		protected override string Description => "Get the contents of a documentation file.";
		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["file"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["file"] = QJS.Enum(GetPaths())
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string? parsedData)
		{
			string? link = actionData.Data?.Value<string>("file");

			parsedData = null;
			if (string.IsNullOrEmpty(link) || !GetPaths().Contains(link))
			{
				return ExecutionResult.Failure($"You did not provide a valid file");
			}

			parsedData = link;
			return ExecutionResult.Success();
		}
	
		protected override void Execute(string? parsedData)
		{
			if (parsedData is null) return;
			
			Context.Send($"This is the contents of {parsedData}.\n{DocWindowHelper.GetText(parsedData)}");
		}

		private static List<string> GetPaths()
		{
			var windowHelper = new DocWindowHelper();
			windowHelper.CreateDocWindow();
			var links = windowHelper.GetLinks();
			links.RemoveAll(string.IsNullOrEmpty);
			windowHelper.Destroy();
			
			return links;
		}
	}
}