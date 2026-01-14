using NeuroTFWRIntegration.Utilities;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace NeuroTFWRIntegration.Unity.Components.Chat;

public class BaseChat : MonoBehaviour
{
	protected static CanvasGroup? MainGroup;
	protected static bool ChatOpen;
	protected static GameObject? Extension;
	
	protected void AwakeCore()
	{
		Utilities.Logger.Info($"creating base chat");
		
		Utilities.Logger.Info($"finding");
		var find = GameObject.Find("MainButton");
		if (!find)
		{
			Utilities.Logger.Error($"could not find main button");
			return;
		}

		var component = find.GetComponent<Button>();
		if (!component)
		{
			Utilities.Logger.Error($"could not find button");
			return;
		}
		Utilities.Logger.Info($"setting onclick listener");
		component.onClick.AddListener(OpenClicked);
	}
	
	public virtual void OpenClicked()
	{
		Utilities.Logger.Info($"pressed on close clicked");
		if (ChatOpen)
		{
			CloseExtension();
			ChatOpen = false;
		}
		else
		{
			OpenExtension();
			ChatOpen = true;
		}
	}

	public virtual void OpenExtension()
	{
		Extension?.SetActive(true);
	}
	
	public virtual void CloseExtension()
	{
		Extension?.SetActive(false);
	}

	private void Start()
	{
		Utilities.Logger.Info($"base chat started");
	}

	private void Update()
	{
		if (WorkspaceState.MenuOpen)
		{
			MainGroup?.alpha = 0;
		}
		else
		{
			if (MainGroup?.alpha > 0) return;
			
			MainGroup?.alpha = 1;
		}
	}
}