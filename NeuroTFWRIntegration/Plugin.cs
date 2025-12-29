using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.Utilities;
using TMPro;
using UnityEngine;
using static NeuroTFWRIntegration.Logger;

namespace NeuroTFWRIntegration;

[BepInPlugin(LocalPluginInfo.PLUGIN_GUID, LocalPluginInfo.PLUGIN_NAME, LocalPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	public static Plugin? Instance { get; private set; }

	private static ConfigEntry<string>? _websocketUrl;
	
	public static ConfigEntry<bool>? ResearchMenuActions;

	public static ConfigEntry<bool>? Debug;
	
	public Plugin()
	{
		Instance = this;
		
		_websocketUrl = ConfigStrings.WebsocketUrl.BaseToEntry();
		ResearchMenuActions = ConfigStrings.ResearchMenuActions.BaseToEntry();
		Debug = ConfigStrings.Debug.BaseToEntry();
	}

	private void Awake()
	{
		SetLogger(Logger);
		if (_websocketUrl?.Value != "")
		{
			Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", _websocketUrl?.Value);
		}
		
		NeuroSdk.NeuroSdkSetup.Initialize("The Farmer Was Replaced");

		Harmony.CreateAndPatchAll(typeof(RegisterPatches));
		Harmony.CreateAndPatchAll(typeof(ContextPatches));
		
		RegisterMainActions.PopulateActionLists();
	}

	private int _waitNext;
	private void Update()
	{
		if (_waitNext > 100)
		{
			_waitNext = 0;
		}

		if (_waitNext != 0)
		{
			_waitNext++;
			return;
		}
		if (Debug is null || !Debug.Value) return;
		
		if (UnityInput.Current.GetKey(KeyCode.F))
		{
			RegisterMainActions.UnregisterMain();
			RegisterMainActions.RegisterMain();
		}

		if (UnityInput.Current.GetKey(KeyCode.Z))
		{
			_waitNext = 1;

			
			var window = Instantiate(WorkspaceState.CurrentWorkspace.docWinPrefab, WorkspaceState.Sim.inv.container);
			Logger.LogInfo($"container: {window.container}");
			Logger.LogInfo($"markdown prefab: {window.markdownPrefab}");

			string homePath = "docs/home.md";
			window.LoadDoc(homePath);

			List<string> paths = new(); 
			foreach (CodeInputField textField in window.OpenMarkdownText.textFields)
			{
				Logger.LogInfo($"text field: {textField}");
					
				foreach (var link in textField.textComponent.textInfo.linkInfo)
				{
					paths.Add(link.GetLink());
					Logger.LogInfo($"link: {link.GetLink()}");
				}
			}
			
			Destroy(window.gameObject);
		}
	}
}