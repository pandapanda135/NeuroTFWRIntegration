using System;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Patching;

namespace NeuroTFWRIntegration.Actions;

public static class CodeWindowActions
{
	// we probably don't need this anymore
	public class GetWindows : NeuroAction
	{
		public override string Name => "get_code_windows";
		protected override string Description => "";
		protected override JsonSchema Schema => new();

		protected override ExecutionResult Validate(ActionJData actionData)
		{
			return ExecutionResult.Success();
		}

		protected override void Execute()
		{
			string contextString = $"These are the name's of the windows in this workspace:";
			foreach (var window in MainSim.Inst.workspace.codeWindows)
			{
				contextString += $"\n{window.Key}";
				Logger.Info($"windows: {window.Key}   {window.Value.CodeInput.text}");
			}

			Context.Send(contextString);
			var w = ActionWindow.Create(MainSim.Inst.gameObject);
			w.AddAction(new GetWindows()).AddAction(new ExecuteWindow())
				.AddAction(new SelectWindow());
			w.Register();
		}
	}

	public class SelectWindow : NeuroAction<CodeWindow>
	{
		public override string Name => "select_window";
		protected override string Description => "";

		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["window"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["window"] = QJS.Enum(MainSim.Inst.workspace.codeWindows
					.Select(kvp => kvp.Key))
			}
		};

		protected override ExecutionResult Validate(ActionJData actionData, out CodeWindow parsedData)
		{
			string name = actionData.Data?.Value<string>("window");
			parsedData = new();
			if (name is null) return ExecutionResult.Failure($"You must sent the name of window.");

			if (!MainSim.Inst.workspace.codeWindows.ContainsKey(name))
				return ExecutionResult.Failure($"That is not a valid window");
			parsedData = MainSim.Inst.workspace.codeWindows[name];
			return ExecutionResult.Success($"");
		}

		protected override void Execute(CodeWindow parsedData)
		{
			parsedData.CodeInput.onSelect.Invoke("0");
			RegisterSelectedWindow(parsedData);
		}
	}

	private class WritePatch : NeuroAction<string>
	{
		public override string Name => "write_patch";

		protected override string Description => "Write a patch to modify the code in this code window. The format is" +
		                                         $"{PatchStrings.SearchParser}";

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
			parsedData = actionData.Data?.Value<string>("text");

			try
			{
				var parser = new SearchParser(parsedData);
				if (!parser.IsValidPatch(parsedData, out string reason))
				{
					return ExecutionResult.Failure(
						$"You provided an invalid patch string, this is why it is invalid: {reason}");
				}
			}
			catch (Exception e)
			{
				Logger.Error($"Write patch validation error: {e}");
				return ExecutionResult.Failure(
					$"You made a mistake when writing this patch, this is the error message: {e.Message}");
			}

			return ExecutionResult.Success($"");
		}

		protected override void Execute(string parsedData)
		{
			Logger.Info($"running write execute");
			var parser = new SearchParser(parsedData);

			try
			{
				parser.Parse(parsedData);
			}
			catch (Exception e)
			{
				Logger.Error($"What the fuck happened here: {e}");
				throw;
			}

			var window = ActionWindow.Create(MainSim.Inst.gameObject);
			window.AddAction(new GetWindows()).AddAction(new ExecuteWindow()).AddAction(new SelectWindow());
			window.Register();
		}
	}

	public class ExecuteWindow : NeuroAction<CodeWindow>
	{
		public override string Name => "execute_window";
		protected override string Description => "Execute a window";

		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["window"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["window"] = QJS.Enum(MainSim.Inst.workspace.codeWindows
					.Select(kvp => kvp.Key))
			}
		};

		protected override ExecutionResult Validate(ActionJData actionData, out CodeWindow parsedData)
		{
			string name = actionData.Data?.Value<string>("window");
			parsedData = new();
			if (name is null) return ExecutionResult.Failure($"You must sent the name of window.");

			if (!MainSim.Inst.workspace.codeWindows.ContainsKey(name))
				return ExecutionResult.Failure($"That is not a valid window");
			parsedData = MainSim.Inst.workspace.codeWindows[name];
			return ExecutionResult.Success();
		}

		protected override void Execute(CodeWindow parsedData)
		{
			parsedData.PressExecuteOrStop();
			var window = ActionWindow.Create(MainSim.Inst.gameObject);
			window.AddAction(new CodeWindowActions.GetWindows()).AddAction(new CodeWindowActions.ExecuteWindow())
				.AddAction(new CodeWindowActions.SelectWindow());
			window.Register();
		}
	}

	private static CodeWindow _window;

	private static void RegisterSelectedWindow(CodeWindow codeWindow)
	{
		_window = codeWindow;
		var window = ActionWindow.Create(MainSim.Inst.gameObject);
		window.SetForce(0, $"You are interacting with a window with the name of {codeWindow.fileName}",
			$"This is the code of this window: {codeWindow.CodeInput.text}", true);
		window.AddAction(new WritePatch());
		// window.AddAction(new ExecuteWindow());
		window.Register();
	}
}