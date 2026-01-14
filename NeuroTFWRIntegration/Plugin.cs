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

	private static GameObject? _toastContainer;
	public static ToastsManager? ToastsManager => _toastContainer?.GetComponent<ToastsManager>();

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
		AddToastsContainer();
		CreateChat();
	}

	private AssetBundle? _bundle;
	private AssetBundle? _toastBundle;
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

			_toastBundle = AssetBundleHelper.GetAssetBundle(toastPath);
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
				_toastContainer?.GetComponent<ToastsManager>().AddToast(toastAsset);
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

	private static readonly string ContainerPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "toastcontainer");
	private void AddToastsContainer()
	{
		if (GameObject.Find("ToastsContainer") is not null)
		{
			return;
		}
		
		_bundle = AssetBundleHelper.GetAssetBundle(ContainerPath);
		if (_bundle is null)
		{
			throw new NullReferenceException("Toast's container AssetBundle was null.");
		}

		Logger.LogInfo($"creating toast");
		var container = AssetBundleHelper.LoadBundle(ContainerPath, "Assets/ToastsContainer.prefab");
		if (container is null)
		{
			throw new NullReferenceException("container was null, there was an issue when loading it.");
		}
		container.AddComponent(typeof(ToastsManager));
		var overlay = GameObject.Find("OverlayUI");
		if (overlay is null)
		{
			throw new NullReferenceException($"There was an issue finding OverlayUI.");
		}
			
		var containerInst = Instantiate(container, overlay.transform, false);
			
		containerInst.transform.localPosition = Vector3.zero;
		containerInst.transform.localRotation = Quaternion.identity;
		containerInst.transform.localScale = Vector3.one;
			
		containerInst.SetActive(true);
		containerInst.transform.SetAsLastSibling();

		Canvas canvas = containerInst.GetComponent<Canvas>();
		if (canvas)
		{
			canvas.sortingOrder = 100;
		}

		_toastContainer = containerInst;
	}
	
	private static readonly string ChatPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "neuro-chat");
	private void CreateChat()
	{
		if (GameObject.Find("NeuroChat") is not null)
		{
			return;
		}

		if (!File.Exists(ChatPath))
		{
			Logger.LogError($"could not find neuro-chat file.");
			return;
		}
		
		AssetBundle bundle = AssetBundleHelper.GetAssetBundle(ChatPath);
		if (bundle is null)
		{
			throw new NullReferenceException("Toast's container AssetBundle was null.");
		}

		Logger.LogInfo($"creating chat");
		var chat = AssetBundleHelper.LoadBundle(ChatPath, "Assets/NeuroChat.prefab");
		if (chat is null)
		{
			throw new NullReferenceException("container was null, there was an issue when loading it.");
		}
		chat.AddComponent(typeof(NeuroChat));
		var overlay = GameObject.Find("OverlayUI");
		if (overlay is null)
		{
			throw new NullReferenceException($"There was an issue finding OverlayUI.");
		}
		
		Logger.LogInfo($"instantiating NeuroChat");
		var chatInst = Instantiate(chat, overlay.transform, false);
		
		// var rect = chatInst.GetComponent<RectTransform>();
		// rect.anchorMin = Vector2.zero;
		// rect.anchorMax = Vector2.one;
		// rect.offsetMin = Vector2.zero;
		// rect.offsetMax = Vector2.zero;
		
		// chatInst.transform.localPosition = new Vector3(0,0,0);
		// chatInst.transform.position = new Vector3(0,0,0);
		// chatInst.transform.localRotation = Quaternion.identity;
		// chatInst.transform.localScale = Vector3.one;
		
		chatInst.SetActive(true);
	}
}