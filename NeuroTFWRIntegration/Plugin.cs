using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Patches;
using NeuroTFWRIntegration.Unity;
using NeuroTFWRIntegration.Unity.Components.SwarmDrone;
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
	
	#region UIAssets

	public static GameObject? ToastContainer;
	public static ToastsManager? ToastsManager => ToastContainer?.GetComponent<ToastsManager>();

	#endregion
	
	public Plugin()
	{
		Instance = this;
	}

	private void Awake()
	{
		SetLogger(Logger);
		if (ConfigHandler.WebsocketUrl.Entry.Value != "")
		{
			Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", ConfigHandler.WebsocketUrl.Entry.Value);
		}

		NeuroSdk.NeuroSdkSetup.Initialize("The Farmer Was Replaced");

		Harmony.CreateAndPatchAll(typeof(RegisterPatches));
		Harmony.CreateAndPatchAll(typeof(ContextPatches));
		Harmony.CreateAndPatchAll(typeof(CreateDrone));
		
		Context.Send($"{Strings.StartGameContext}");
		RegisterMainActions.PopulateActionLists();
		LoadComponents.LoadStartingComponents();
	}

	private int _waitNext;
	private void Update()
	{
		if (!ConfigHandler.Debug.Entry.Value) return;

		if (_waitNext > 0)
		{
			_waitNext--;
			return;
		}
		
		if (_waitNext == 0)
		{
			_waitNext = -1;
			return;
		}

		if (UnityInput.Current.GetKey(KeyCode.U))
		{
			Utilities.Logger.Info($"propeller amount: {GameObject.Find("Farm").GetComponent<FarmRenderer>().propellerMeshes.Count}");
			_waitNext = 100;
			foreach (var kvp in ResourceManager.hats)
			{
				Utilities.Logger.Info($"hat: {kvp.Key}   {kvp.Value.className}    {kvp.Value.hidden}    {kvp.Value.droneFlyHeight}");
			}
		}
		
		if (UnityInput.Current.GetKey(KeyCode.H))
		{
			_waitNext = 100;
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
			_waitNext = 100;
			
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