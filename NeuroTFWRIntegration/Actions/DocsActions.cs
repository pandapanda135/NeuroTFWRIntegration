using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

public static class DocsActions
{
	public class GetDocumentation : NeuroAction<string>
	{
		public override string Name => "get_documentation";
		protected override string Description => "Get sent the contents of a documentation file.";
		protected override JsonSchema? Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["file"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["file"] = QJS.Enum(WorkspaceState.Sim.researchMenu.allBoxes
					.Where(box => box.Value.unlockState is UnlockBox.UnlockState.Unlocked)
					.Select(box => box.Value.unlockSO.docs))
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string? parsedData)
		{
			string? fileName = actionData.Data?.Value<string>("file");

			parsedData = null;
			if (!WorkspaceState.Sim.researchMenu.allBoxes
				    .Where(box => box.Value.unlockState is UnlockBox.UnlockState.Unlocked)
				    .Select(box => box.Value.unlockSO.docs).Contains(fileName))
			{
				return ExecutionResult.Failure($"The file you provided is not valid.");
			}

			parsedData = fileName;
			return ExecutionResult.Success($"");
		}

		protected override void Execute(string? parsedData)
		{
			WorkspaceState.CurrentWorkspace.AddNewDocsWindow(parsedData);
			
			foreach (var kvp in WorkspaceState.CurrentWorkspace.openWindows)
			{
				Logger.Info($"{kvp.Key}   {kvp.Value.windowName}");
				var component = kvp.Value.TryGetComponent(typeof(DocsWindow), out Component docsComponent);
				if (!component)
				{
					Logger.Info($"was false: {kvp.Key}");
					continue;
				}

				if (docsComponent is not DocsWindow docs) return;
				Context.Send($"These are the full contents of {docs.openDoc}\n{docs.fullOpenDoc}");
			}
		}
	}

	public class GetNewDocumentation : NeuroAction<string>
	{
		public override string Name => "get_new_documentation";
		protected override string Description => "sadf";

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
			throw new System.NotImplementedException();
		}
	
		protected override void Execute(string? parsedData)
		{
			throw new System.NotImplementedException();
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