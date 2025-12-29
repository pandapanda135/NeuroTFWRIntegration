using System;
using System.Collections.Generic;
using NeuroTFWRIntegration.Utilities;
using TMPro;
using Object = UnityEngine.Object;

namespace NeuroTFWRIntegration;

public class DocWindowHelper
{
	private DocsWindow? _window;
	
	private const string HomePath = "docs/home.md";

	public void CreateDocWindow(string path = HomePath)
	{
		// we use the inventory container as the workspace container doesn't work when in the menu.
		var window = Object.Instantiate(WorkspaceState.CurrentWorkspace.docWinPrefab, WorkspaceState.Sim.inv.container);
		// TODO: load doc causes a stutter
		window.LoadDoc(path);

		_window = window;
	}
	
	public List<string> GetLinks()
	{
		if (_window is null) throw new NullReferenceException();
		
		List<string> lines = new(); 
		foreach (CodeInputField textField in _window.OpenMarkdownText.textFields)
		{
			foreach (var link in textField.textComponent.textInfo.linkInfo)
			{
				lines.Add(link.GetLink());
			}
		}
		
		return lines;
	}

	public void Destroy()
	{
		Object.Destroy(_window?.gameObject);
	}
}