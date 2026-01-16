using System;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

/// <inheritdoc/>
public abstract class BaseNeuroActionWrapper : INeuroAction
{
	/// <summary>
	/// This can hold a method that will be run after execute, if validation is not successful this will not be ran.
	/// </summary>
	public Action? PostExecuteAction = null;
	public virtual ActionWindow? ActionWindow { get; private set; }
	
	public abstract string Name { get; }
	protected abstract string Description { get; }
	protected abstract JsonSchema? Schema { get; }
	
	public virtual bool CanAddToActionWindow(ActionWindow actionWindow) => true;
	
	ExecutionResult INeuroAction.Validate(ActionJData actionData, out object? parsedData)
	{
		ExecutionResult result = Validate(actionData, out parsedData);

		if (ActionWindow != null)
		{
			return new ActionWindowResponse(ActionWindow).Result(result);
		}
		
		// we don't add toast here as it will lead to multiple toasts for same action.
		return result;
	}
	
	protected abstract ExecutionResult Validate(ActionJData actionData, out object? parsedData);
	
	void INeuroAction.Execute(object? data) => Execute(data);
	protected abstract void Execute(object? data);
	
	public virtual WsAction GetWsAction()
	{
		return new WsAction(Name, Description, Schema);
	}

	public virtual void SetActionWindow(ActionWindow actionWindow)
	{
		if (ActionWindow != null)
		{
			if (ActionWindow != actionWindow)
			{
				Debug.LogError("Cannot set the action window for this action, it is already set.");
			}

			return;
		}

		ActionWindow = actionWindow;
	}

	protected virtual void AddToast(ExecutionResult result)
	{
		if (ConfigHandler.Toasts.Entry.Value == Toasts.Disabled) return;
		
		string text = result.Successful ? string.Format(Strings.SuccessfulToast, Name) : string.Format(Strings.UnsuccessfulToastNoMessage, Name);
		if (!result.Successful && !string.IsNullOrEmpty(result.Message))
			text = string.Format(Strings.UnsuccessfulToast, Name, result.Message);
		
		var toast = ToastsManager.CreateValidationToast(text,
			result.Successful ? ValidationToast.ValidationLevels.Success : ValidationToast.ValidationLevels.Failure);
		if (toast is null)
		{
			Utilities.Logger.Error($"toast was null in base AddToast, result was: {result.Successful}  {result.Message}");
			return;
		}

		Plugin.ToastsManager?.AddToast(toast);
	}
}

/// <inheritdoc/>
public abstract class NeuroActionWrapper : BaseNeuroActionWrapper
{
	protected abstract ExecutionResult Validate(ActionJData actionData);
	protected abstract void Execute();
	
	protected sealed override ExecutionResult Validate(ActionJData actionData, out object? parsedData)
	{
		ExecutionResult result = Validate(actionData);
		parsedData = null;

		AddToast(result);

		return result;
	}

	protected sealed override void Execute(object? data)
	{
		Execute();
		PostExecuteAction?.Invoke();
	}
}


/// <inheritdoc/>
public abstract class NeuroActionWrapper<TData> : BaseNeuroActionWrapper
{
	protected abstract ExecutionResult Validate(ActionJData actionData, out TData? parsedData);
	protected abstract void Execute(TData? parsedData);
	
	protected sealed override ExecutionResult Validate(ActionJData actionData, out object? parsedData)
	{
		ExecutionResult result = Validate(actionData, out TData? tParsedData);
		parsedData = tParsedData;
		
		AddToast(result);
		
		return result;
	}

	protected sealed override void Execute(object? parsedData)
	{
		Execute((TData?)parsedData);
		PostExecuteAction?.Invoke();
	}
}

/// <inheritdoc/>
public abstract class NeuroActionS<TData> : NeuroAction<TData?> where TData : struct
{
}