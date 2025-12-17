using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Actions;

public static class CodeWindowActions
{
	public class CreateWindow : NeuroAction<string>
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
			string name = actionData.Data?.Value<string>("name");

			parsedData = "";
			if (string.IsNullOrEmpty(name))
				return ExecutionResult.Failure($"You must provide a value for the window's name.");

			if (name.Length > 20)
				return ExecutionResult.Failure($"The name cannot be greater than 20 characters in length.");

			if (WorkspaceState.CodeWindows.ContainsKey(name))
				return ExecutionResult.Failure($"This name is already used for an existing window.");
			// we don't need to check if name is valid as it will change the window's name not file name if it is not valid.

			parsedData = name;
			return ExecutionResult.Success($"Created new window and called it {name}");
		}

		protected override void Execute(string parsedData)
		{
			ICollection<string> previousWindows = WorkspaceState.CodeWindows.Keys;
			WorkspaceState.CurrentWorkspace.AddNewWindow();
			foreach (var kvp in WorkspaceState.CodeWindows)
			{
				if (previousWindows.Contains(kvp.Key)) continue;
				
				Logger.Info($"renaming window: {kvp.Value.fileName}");
				kvp.Value.Rename(parsedData);
				break;
			}
		}
	}
	
	// TODO: we probably don't need this anymore
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
			foreach (var window in WorkspaceState.CurrentWorkspace.codeWindows)
			{
				contextString += $"\n{window.Key}";
				Logger.Info($"windows: {window.Key}   {window.Value.CodeInput.text}");
			}

			Context.Send(contextString);
			var w = ActionWindow.Create(WorkspaceState.Object);
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
				["window"] = QJS.Enum(WorkspaceState.CurrentWorkspace.codeWindows
					.Select(kvp => kvp.Key))
			}
		};

		protected override ExecutionResult Validate(ActionJData actionData, out CodeWindow parsedData)
		{
			string name = actionData.Data?.Value<string>("window");
			parsedData = new();
			if (name is null) return ExecutionResult.Failure($"You must sent the name of window.");

			if (!WorkspaceState.CurrentWorkspace.codeWindows.ContainsKey(name))
				return ExecutionResult.Failure($"That is not a valid window");
			parsedData = WorkspaceState.CurrentWorkspace.codeWindows[name];
			return ExecutionResult.Success($"");
		}

		protected override void Execute(CodeWindow parsedData)
		{
			parsedData.CodeInput.onSelect.Invoke("0");
			RegisterSelectedWindow(parsedData);
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
				["window"] = QJS.Enum(WorkspaceState.CurrentWorkspace.codeWindows
					.Select(kvp => kvp.Key))
			}
		};

		protected override ExecutionResult Validate(ActionJData actionData, out CodeWindow parsedData)
		{
			string name = actionData.Data?.Value<string>("window");
			parsedData = new();
			if (name is null) return ExecutionResult.Failure($"You must sent the name of window.");

			if (!WorkspaceState.CurrentWorkspace.codeWindows.ContainsKey(name))
				return ExecutionResult.Failure($"That is not a valid window");
			parsedData = WorkspaceState.CurrentWorkspace.codeWindows[name];
			return ExecutionResult.Success();
		}

		protected override void Execute(CodeWindow parsedData)
		{
			parsedData.PressExecuteOrStop();
			var window = ActionWindow.Create(WorkspaceState.Object);
			window.AddAction(new GetWindows()).AddAction(new ExecuteWindow())
				.AddAction(new SelectWindow());
			window.Register();
		}
	}
	
	private static void RegisterSelectedWindow(CodeWindow codeWindow)
	{
		var window = ActionWindow.Create(WorkspaceState.Object);
		window.SetForce(0, $"You are interacting with a window with the name of {codeWindow.fileName}",
			$"This is the code of this window: {codeWindow.CodeInput.text}", true);
		// window.AddAction(new WritePatch());
		// window.AddAction(new ExecuteWindow());
		window.Register();
	}
}