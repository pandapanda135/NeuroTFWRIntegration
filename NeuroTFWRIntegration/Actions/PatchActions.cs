
using System;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Utilities;
using NeuroTFWRIntegration.Utilities.Patching;

namespace NeuroTFWRIntegration.Actions;

public static class PatchActions
{
	public class WritePatch : NeuroActionWrapper<string>
	{
		public override string Name => "write_patch";
		protected override string Description => "Write a patch to modify the code in this code window. The format is" +
		                                         $"{(ConfigHandler.Debug.Entry.Value
			                                         ? "Debug is enabled so the format is not being sent." : PatchStrings.SearchParser)}";
		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["text"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["text"] = QJS.Type(JsonSchemaType.String)
			}
		};

		protected override ExecutionResult Validate(ActionJData actionData, out string parsedData)
		{
			string? patchText = actionData.Data?.Value<string>("text");
			parsedData = "";
			if (patchText is null)
				return ExecutionResult.Failure($"You cannot provide a null value.");
			
			try
			{
				patchText = patchText.Replace("\\n", "\n");
				patchText = patchText.Replace("\\t", "\t");
				var parser = PatchHelpers.GetParser(patchText);
				if (!parser.IsValidPatch(patchText, out string reason))
				{
					return ExecutionResult.Failure(
						$"You provided an invalid patch, this is why it is invalid: {reason}");
				}
			}
			catch (Exception e)
			{
				Utilities.Logger.Error($"Write patch validation error: {e}");
				return ExecutionResult.Failure(
					$"You made a mistake when writing this patch, this is the error message: {e.Message}");
			}

			parsedData = patchText;
			return ExecutionResult.Success($"The patch is being inserted now.");
		}

		protected override void Execute(string? parsedData)
		{
			if (parsedData is null) return;
			Utilities.Logger.Info($"running write execute");
			var parser = PatchHelpers.GetParser(parsedData);

			try
			{
				parser.Parse();
			}
			catch (Exception e)
			{
				Utilities.Logger.Error($"What the fuck happened here: {e}");
				Context.Send($"There was an error when trying to apply the patch you just sent, you should either," +
				             $" tell the person you are playing with and see if they can help you or try something else.");
				PostExecuteAction?.Invoke();
				throw;
			}
			
			PostExecuteAction?.Invoke();
		}
	}

	public class GetWindowCode : NeuroActionWrapper<string>
	{
		public override string Name => "get_window_code";
		protected override string Description => "Get the code of specific window";

		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["window"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["window"] = QJS.Enum(WorkspaceState.CodeWindows.Select(kvp => kvp.Value.fileNameText.text))
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string parsedData)
		{
			string? selectedWindow = actionData.Data?.Value<string>("window");

			parsedData = "";
			if (selectedWindow is null || !WorkspaceState.CodeWindows.ContainsKey(selectedWindow))
			{
				return ExecutionResult.Failure($"You did not provide a valid value.");
			}

			parsedData = selectedWindow;
			return ExecutionResult.Success();
		}

		protected override void Execute(string? parsedData)
		{
			var kvp = WorkspaceState.CodeWindows.First(kvp => kvp.Key == parsedData);
			
			Context.Send($"This is the code of {parsedData}:\n{kvp.Value.CodeInput.text}");
		}
	}
}