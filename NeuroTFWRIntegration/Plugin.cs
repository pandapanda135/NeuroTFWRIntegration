using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Actions;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;
using static NeuroTFWRIntegration.Logger;

namespace NeuroTFWRIntegration;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	public static Plugin? Instance { get; private set; }

	private static ConfigEntry<string>? _websocketUrl;

	public static ConfigEntry<bool>? Debug;
	
	public Plugin()
	{
		Instance = this;
		
		_websocketUrl = ConfigStrings.WebsocketUrl.BaseToEntry();
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
	}

	private void Update()
	{
		if (UnityInput.Current.GetKey(KeyCode.F))
		{
			var window = ActionWindow.Create(WorkspaceState.Object);
			window.AddAction(new CodeWindowActions.SelectWindow());
			window.Register();
		}
	}
}