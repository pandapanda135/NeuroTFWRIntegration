using System.Collections.Generic;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Unity.Components.Toasts;

namespace NeuroTFWRIntegration.Actions;

public static class ToastActions
{
	public class CreateToast : NeuroActionWrapper<string>
	{
		public override string Name => "create_toast";
		protected override string Description => "This will create a toast that has whatever message you want.";

		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["text"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["text"] = QJS.Type(JsonSchemaType.String)
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string? parsedData)
		{
			string? text = actionData.Data?.Value<string>("text");

			parsedData = null;
			if (string.IsNullOrEmpty(text))
			{
				return ExecutionResult.Failure($"You must provide text.");
			}

			parsedData = text;
			return ExecutionResult.Success($"");
		}

		protected override void Execute(string? parsedData)
		{
			if (parsedData is null) return;

			var toast = ToastsManager.CreateNeuroToast(parsedData);
			if (toast is null)
			{
				Utilities.Logger.Error($"Neuro toast was null");
				return;
			}
			Plugin.ToastsManager?.AddToast(toast);
		}
	}
}