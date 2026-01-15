using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Patches;
using NeuroTFWRIntegration.Unity;
using NeuroTFWRIntegration.Unity.Components.Chat;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using NeuroTFWRIntegration.Utilities;
using TMPro;
using UnityEngine;
using static NeuroTFWRIntegration.Utilities.Logger;

namespace NeuroTFWRIntegration;

[BepInPlugin(LocalPluginInfo.PLUGIN_GUID, LocalPluginInfo.PLUGIN_NAME, LocalPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	public static Plugin? Instance { get; private set; }

	private static ConfigEntry<string>? _websocketUrl;
	
	public static ConfigEntry<ResearchMenuActions>? ResearchMenuActions;

	public static ConfigEntry<bool>? Debug;

	#region UIAssets

	public static GameObject? ToastContainer;
	public static ToastsManager? ToastsManager => ToastContainer?.GetComponent<ToastsManager>();

	#endregion
	
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
		
		Context.Send($"{Strings.StartGameContext}");
		RegisterMainActions.PopulateActionLists();
		LoadComponents.LoadStartingComponents();
		// CreateTest();
	}

	private int _waitNext;
	private void Update()
	{
		if (Debug is null || !Debug.Value) return;
		if (_waitNext > 100)
		{
			_waitNext = 0;
		}

		if (_waitNext != 0)
		{
			_waitNext++;
			return;
		}

		if (UnityInput.Current.GetKey(KeyCode.H))
		{
			var toastPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "validation-toast");

			AssetBundleHelper.GetAssetBundle(toastPath);
			Logger.LogInfo($"creating toast: {toastPath}");
			var toastAsset = AssetBundleHelper.LoadBundle(toastPath,"Assets/ValidationToast.prefab");
			if (toastAsset is null)
			{
				Utilities.Logger.Error($"toast asset was null");
				return;
			}
			var toast = toastAsset.AddComponent<ValidationToast>();

			for (int i = 0; i < 5; i++)
			{
				Utilities.Logger.Info($"i: {i}");
				toast.Init($"text: {i}", ValidationToast.ValidationLevels.Warning);
				ToastContainer?.GetComponent<ToastsManager>().AddToast(toastAsset);
			}
		}
		
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
			Logger.LogInfo($"debug paths: {string.Join("\n", paths)}");
			
			Destroy(window.gameObject);
		}
	}

}