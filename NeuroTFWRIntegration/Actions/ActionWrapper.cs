using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Unity;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

/// <inheritdoc/>
public abstract class BaseNeuroActionWrapper : INeuroAction
{
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
	public abstract void Execute(object? data);
	
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

	protected void AddToast(ExecutionResult result)
	{
		var toast = ToastsManager.CreateToastObject(
			result.Successful
				? string.Format(Strings.SuccessfulToast, Name)
				: string.Format(Strings.UnsuccessfulToast, Name),
			result.Successful ? ValidationToast.ValidationLevels.Success : ValidationToast.ValidationLevels.Failure);
		if (toast is null)
		{
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

	public sealed override void Execute(object? data) => Execute();
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

	public sealed override void Execute(object? parsedData) => Execute((TData?) parsedData);
}

/// <inheritdoc/>
public abstract class NeuroActionS<TData> : NeuroAction<TData?> where TData : struct
{
}