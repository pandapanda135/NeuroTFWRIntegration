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
	private static List<string> _options = new();
	private static bool _dropDownBeenChanged;

	private static GameObject? _errorText;
	private void Awake()
	{
		MainGroup = GetComponent<CanvasGroup>();
		GameObject.Find("Submit").GetComponent<Button>().onClick.AddListener(SubmitPrompt);
		GameObject.Find("WindowDropdown").GetComponent<TMP_Dropdown>().onValueChanged.AddListener(ValueChanged);
		Extension = GameObject.Find("Extension");
		_errorText = GameObject.Find("ErrorText");
		AwakeCore();
		
		PopulateWindowList();
		Extension.SetActive(false);
	}

	public override void OpenClicked()
	{
		Utilities.Logger.Info($"clicked open");
		// we reset this whether it is being opened or not.
		_dropDownBeenChanged = false;
		ChangeErrorText("");
		base.OpenClicked();
		
		PopulateWindowList();
	}
	
	private static void PopulateWindowList()
	{
		Utilities.Logger.Info($"prompting windows");
		var find = GameObject.Find("WindowDropdown");
		if (!find)
		{
			Utilities.Logger.Error($"find was null");
			return;
		}
		
		var component = find.GetComponent<TMP_Dropdown>();
		if (!component)
		{
			Utilities.Logger.Error($"dropdown was null");
			return;
		}
		component.ClearOptions();
		_options = WorkspaceState.CodeWindows.Keys.ToList();
		component.AddOptions(_options);
	}

	private static void SubmitPrompt()
	{
		try
		{
			ApplyPrompt();
		}
		catch (PromptException promptException)
		{
			ChangeErrorText(promptException.Reason == PromptException.Reasons.Internal ? $"There was an internal issue, this is the reason: {promptException.Message}" : promptException.Message);
		}
		catch (Exception e)
		{
			Utilities.Logger.Error(e);
			throw;
		}
	}

	private static void ApplyPrompt()
	{
		var dropdown = GameObject.Find("WindowDropdown").GetComponent<TMP_Dropdown>();
		if (dropdown is null)
		{
			throw new PromptException(PromptException.Reasons.Internal, "Drop down was null.");
		}

		if (_options.Count == 0 || dropdown.value > _options.Count)
		{
			throw new PromptException(PromptException.Reasons.DropDown, "There are no allowed valid dropdown options.");
		}
		var option = dropdown.options[dropdown.value];
		if (!_dropDownBeenChanged)
		{
			throw new PromptException(PromptException.Reasons.DropDown,
				"You must change the value of the drop down to submit.");
		}
		var prompt = GameObject.Find("PromptInput").GetComponent<TMP_InputField>().text;
		if (string.IsNullOrEmpty(prompt))
		{
			throw new PromptException(PromptException.Reasons.Prompt, "You did not supply a prompt.");
		}
		
		// we probably won't need existing error at this point.
		ChangeErrorText("");
		RegisterMainActions.UnregisterMain();

		var actionWindow = ActionWindow.Create(WorkspaceState.Object);

		actionWindow.SetForce(0, "You have been asked to write a patch for a window by whoever you are playing with.",
			$"This is the code in the windows they want you to modify, {WindowFileSystem.Open(option.text)}\nThis is the prompt they sent you.\n{prompt}");

		var patchAction = new PatchActions.WritePatch();
		patchAction.PostExecuteAction = RegisterMainActions.RegisterMain;
		
		actionWindow.AddAction(patchAction).AddAction(new DenyRequest());
		actionWindow.Register();
	}

	private static void ValueChanged(int arg0)
	{
		_dropDownBeenChanged = true;
	}

	private static void ChangeErrorText(string text)
	{
		_errorText?.SetActive(!string.IsNullOrEmpty(text));
		_errorText?.GetComponent<TextMeshProUGUI>().text = text;
	}

	protected class PromptException(PromptException.Reasons reason, string message) : Exception
	{
		public enum Reasons
		{
			Internal,
			DropDown,
			Prompt
		}

		public Reasons Reason { get; } = reason;
		public override string Message { get; } = message;
	}
}

internal class DenyRequest : NeuroActionWrapper<string?>
{
	public override string Name => "deny_request";
	protected override string Description => "Deny the request from the user. If you want to give them feedback you can by populating the feedback field.";
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
		return ExecutionResult.Success();
	}

	protected override void Execute(string? s)
	{
		if (s is null)
		{
			ToastsManager.CreateValidationToast("Neuro denied your request.",
				ValidationToast.ValidationLevels.Failure);
			return;
		}
		
		ToastsManager.CreateValidationToast($"Neuro denied your request, this is feedback she sent you.\n{s}",
			ValidationToast.ValidationLevels.Failure);
		RegisterMainActions.RegisterMain();
	}

	protected override void AddToast(ExecutionResult result)
	{}
}