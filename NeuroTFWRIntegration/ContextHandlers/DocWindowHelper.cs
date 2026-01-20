using System;
using System.Collections.Generic;
using System.Linq;
using NeuroTFWRIntegration.Utilities;
using TMPro;
using Object = UnityEngine.Object;

namespace NeuroTFWRIntegration.ContextHandlers;

public class DocWindowHelper
{
	private DocsWindow? _window;
	
	private const string HomePath = "docs/home.md";

	public void CreateDocWindow(string path = HomePath)
	{
		// we use the inventory container as the workspace container doesn't work when in the menu.
		var window = Object.Instantiate(WorkspaceState.CurrentWorkspace.docWinPrefab, WorkspaceState.Sim.inv.container);
		// TODO: load doc causes a stutter. This can be seen in game when switching from a file to the general docs home. I don't think I can fix this here.
		window.LoadDoc(path);

		_window = window;
	}

	public void ChangeDoc(string path)
	{
		_window?.LoadDoc(path);
	}
	
	public List<string> GetLinks()
	{
		if (_window is null) throw new NullReferenceException();
		
		List<string> lines = new(); 
		foreach (CodeInputField textField in _window.OpenMarkdownText.textFields)
		{
			lines.AddRange(textField.textComponent.textInfo.linkInfo.Select(link => link.GetLink()));
		}
		
		return lines;
	}

	public string GetDocText()
	{
		if (_window is null) throw new NullReferenceException();
		string text = _window.OpenMarkdownText.textFields.Aggregate("", (current, field) => current + field.textComponent.GetParsedText());

		return text;
	}

	public static string GetText(string link)
	{
		var window = new DocWindowHelper();
		window.CreateDocWindow(link);

		string text = window.GetDocText();
		window.Destroy();
		return text;
	}

	public void Destroy()
	{
		Object.Destroy(_window?.gameObject);
	}
}