using System.Linq;
using NeuroTFWRIntegration.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroTFWRIntegration.Unity.Components.Chat;

public class NeuroChat : BaseChat
{
	private void Awake()
	{
		MainGroup = GetComponent<CanvasGroup>();
		GameObject.Find("Submit").GetComponent<Button>().onClick.AddListener(SubmitPrompt);
		Extension = GameObject.Find("Extension");
		AwakeCore();
		
		PopulateWindowList();
		Extension.SetActive(false);
	}

	public override void OpenClicked()
	{
		base.OpenClicked();
		
		PopulateWindowList();
	}

	private static void PopulateWindowList()
	{
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
		component.AddOptions(WorkspaceState.CodeWindows.Keys.ToList());
	}

	private static void SubmitPrompt()
	{
		Utilities.Logger.Info($"submit prompt");
	}
}