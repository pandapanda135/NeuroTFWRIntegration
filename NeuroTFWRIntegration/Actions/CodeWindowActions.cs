using System.Collections.Generic;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Actions;

public static class CodeWindowActions
{
	public class CreateWindow : NeuroActionWrapper<string>
	{
		public override string Name => "create_window";
		protected override string Description => "Create a window for you to code in.";
		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["name"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["name"] = new()
				{
					Type = JsonSchemaType.String,
					MaxLength = 20,
					MinLength = 0
				}
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string parsedData)
		{
			string? name = actionData.Data?.Value<string>("name");

			parsedData = "";
			if (string.IsNullOrEmpty(name))
				return ExecutionResult.Failure($"You must provide a value for the window's name.");

			if (name.Length > 20)
				return ExecutionResult.Failure($"The name cannot be greater than 20 characters in length.");

			if (WorkspaceState.CodeWindows.ContainsKey(name))
				return ExecutionResult.Failure($"This name is already used for an existing window.");
			// we don't need to check if name is valid as it will change the window's name not file name if it is not valid.

			// we unregister in case she tries to do another action before the window is created. This will get registered again in the create code window patch
			RegisterMainActions.UnregisterMain();
			parsedData = name;
			return ExecutionResult.Success($"Created new window and called it {name}");
		}

		protected override void Execute(string? parsedData)
		{
			ICollection<string> previousWindows = WorkspaceState.CodeWindows.Keys;
			WorkspaceState.CurrentWorkspace.AddNewWindow();
			foreach (var kvp in WorkspaceState.CodeWindows)
			{
				if (previousWindows.Contains(kvp.Key)) continue;
				
				Utilities.Logger.Info($"renaming window: {kvp.Value.fileName}");
				kvp.Value.Rename(parsedData);
				break;
			}
		}
	}
}