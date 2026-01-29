using System;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using NeuroTFWRIntegration.Utilities;
using NeuroTFWRIntegration.Utilities.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroTFWRIntegration.Unity.Components.Chat;

public class NeuroChat : BaseChat
{
	private static readonly string ButtonPath = AssetBundleHelper.GetBundlePath("chat-window-button");

	private Button? _submitButton;
	private GameObject? _windowGrid;
	private GameObject? _errorText;
	private GameObject? _windowsErrorText;
	private GameObject? _promptInput;

	private GameObject? _windowButton;
	
	private void Awake()
	{
		MainGroup = GetComponent<CanvasGroup>();
		Extension = GameObject.Find("Extension");
		
		_submitButton = GameObject.Find("Submit").GetComponent<Button>();
		_submitButton.onClick.AddListener(SubmitPrompt);
		_windowGrid = GameObject.Find("WindowGrid");
		_errorText = GameObject.Find("ErrorText");
		_windowsErrorText = GameObject.Find("WindowsErrorText");
		_promptInput = GameObject.Find("PromptInput");
		AwakeCore();
		
		Extension.SetActive(false);
		_windowsErrorText.SetActive(false);

		_windowButton = LoadWindowButton(typeof(WindowButton));
		_windowButton.transform.localScale = new Vector3(1f, 1f, 0f);
	}
	
	public override void OpenClicked()
	{
		// we reset this whether it is being opened or not.
		ChangeErrorText("");
		base.OpenClicked();
		
		if (ChatOpen) PopulateWindowList();
	}
	
	private void PopulateWindowList()
	{
		// reset all buttons and prevent overflow
		for (int i = 0; i < _windowGrid?.transform.childCount; i++)
		{
			Destroy(_windowGrid.transform.GetChild(i).gameObject);
		}

		if (!WorkspaceState.CodeWindows.Any())
		{
			ChangeErrorText("It seems there are no valid windows to select :(", _windowsErrorText);
			return;
		}
		
		if (!_windowGrid)
		{
			Utilities.Logger.Error($"window grid was null when adding PopulateWindowList");
			ChangeErrorText("There was an internal issue when finding window grid. Oops!", _windowsErrorText);
			return;
		}
		
		foreach (var kvp in WorkspaceState.CodeWindows)
		{
			_windowButton?.GetComponent<WindowButton>().SetDisplay(kvp);
			Instantiate(_windowButton, _windowGrid.transform);
		}
	}

	private void SubmitPrompt()
	{
		try
		{
			ApplyPrompt();
		}
		catch (PromptException promptException)
		{
			ChangeErrorText(promptException.Reason == PromptException.Reasons.Internal 
				? $"There was an internal issue, this is the reason: {promptException.Message}" 
				: promptException.Message);
		}
		catch (Exception e)
		{
			Utilities.Logger.Error(e);
			throw;
		}
	}

	private void ApplyPrompt()
	{
		List<CodeWindow> windows = new();
		for (int i = 0; i < _windowGrid?.transform.childCount; i++)
		{
			var button = _windowGrid.transform.GetChild(i).GetComponent<WindowButton>();
		
			if (button.selected && button.codeWindow)
				windows.Add(button.codeWindow);
			else
			{
				if (!button.selected) continue;
				
				Utilities.Logger.Error($"There was an error when getting a code window as it was null, the button was" +
				                       $"button.selected {button.displayString} selected: {button.selected}");
			}
		}

		if (!windows.Any())
		{
			throw new PromptException(PromptException.Reasons.Windows, "You need to select at least one button.");
		}
		
		var prompt = _promptInput?.GetComponent<TMP_InputField>().text;
		if (string.IsNullOrEmpty(prompt))
		{
			throw new PromptException(PromptException.Reasons.Prompt, "You did not supply a prompt.");
		}
		
		// we probably won't need existing error at this point.
		ChangeErrorText("");
		RegisterMainActions.UnregisterMain();
		var patchAction = new PatchActions.WritePatch
		{
			PostExecuteAction = RegisterMainActions.RegisterMain
		};
		string windowState = "";
		foreach (var codeWindow in windows)
		{
			windowState += $"\n# File name\n{codeWindow.fileNameText.text}\n## Contents\n{WindowFileSystem.Open(codeWindow.fileName)}";
		}
		
		ActionWindow.Create(WorkspaceState.Object).AddAction(patchAction).AddAction(new DenyRequest())
			.SetForce(0, "You have been asked to write a patch for a window by whoever you are playing with.",
				$"This is the code in the windows they want you to modify.\n{windowState}\nThis is the prompt they sent you.\n{prompt}",
				true)
			.Register();
	}

	public void ChangeErrorText(string text, GameObject? obj = null)
	{
		obj ??= _errorText;
		
		obj?.SetActive(!string.IsNullOrEmpty(text));
		obj?.GetComponent<TextMeshProUGUI>().text = text;
	}
	
	private static GameObject LoadWindowButton(Type component)
	{
		AssetBundle bundle = AssetBundleHelper.GetAssetBundle(ButtonPath);
		if (bundle is null)
		{
			throw new NullReferenceException("Button's container AssetBundle was null.");
		}
		
		var button = AssetBundleHelper.LoadBundle(ButtonPath, "Assets/WindowButtonCanvas.prefab");
		if (button is null)
		{
			throw new NullReferenceException("button was null, there was an issue when loading it.");
		}
		button.AddComponent(component);
		// I think this is good practice
		AssetBundleHelper.UnloadBundle(ButtonPath);
		return button;
	}
	
	protected class PromptException(PromptException.Reasons reason, string message) : Exception
	{
		public enum Reasons
		{
			Internal,
			Windows,
			Prompt
		}

		public Reasons Reason { get; } = reason;
		public override string Message { get; } = message;
	}
}

internal class DenyRequest : NeuroActionWrapper<string?>
{
	public override string Name => "deny_request";
	protected override string Description => "Deny the request from the user. If you want to give them feedback, you can, by populating the feedback field.";
	protected override JsonSchema Schema => new()
	{
		Type = JsonSchemaType.Object,
		Required = [],
		Properties = new Dictionary<string, JsonSchema>
		{
			["feedback"] = QJS.Type(JsonSchemaType.String)
		}
	};
	protected override ExecutionResult Validate(ActionJData actionData, out string? s)
	{
		s = actionData.Data?.Value<string?>("feedback");
		
		var toast = ToastsManager.CreateValidationToast("test", ValidationToast.ValidationLevels.Failure);
		if (!toast)
			return ExecutionResult.Failure($"There was an error creating the toast. This is an internal error. Oops!");
		
		return ExecutionResult.Success();
	}

	protected override void Execute(string? s)
	{
		var toastDescription = s is null
			? "Neuro denied your request."
			: $"Neuro denied your request, this is the feedback she sent you:\n{s}";

		var toast = ToastsManager.CreateValidationToast(toastDescription,ValidationToast.ValidationLevels.Failure);
		if (!toast)
			return;
		
		Plugin.ToastsManager?.AddToast(toast);
		Plugin.NeuroChat?.GetComponent<NeuroChat>().ChangeErrorText(toastDescription);

		RegisterMainActions.RegisterMain();
	}

	protected override void AddToast(ExecutionResult result)
	{
		if (!result.Successful)
			base.AddToast(result);
	}
}