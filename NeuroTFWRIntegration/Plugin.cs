using System;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroTFWRIntegration.Actions;
using UnityEngine;
using static NeuroTFWRIntegration.Logger;

namespace NeuroTFWRIntegration;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	private void Awake()
	{
		SetLogger(Logger);
		Info($"This is the new plugin");
		Info($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
		Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", "ws://localhost:8000/ws/");
		NeuroSdk.NeuroSdkSetup.Initialize("The Farmer Was Replaced");

		Harmony.CreateAndPatchAll(typeof(Plugin));
		var window = ActionWindow.Create(MainSim.Inst.gameObject);

		Task.Run(async () =>
		{
			await Task.Delay(5000);

			// window.AddAction(new CodeWindowActions.GetWindows());

			window.AddAction(new CodeWindowActions.SelectWindow());
			window.Register();
		});
	}

	[HarmonyPatch(typeof(CodeWindow), nameof(CodeWindow.Scroll))]
	[HarmonyPrefix]
	static void Prefix(float scroll)
	{
		Info($"prefix of scroll");
		Info($"scroll: {scroll}");
	}

	private void Update()
	{
		if (UnityInput.Current.GetKey(KeyCode.F))
		{
			var window = ActionWindow.Create(MainSim.Inst.gameObject);
			window.AddAction(new CodeWindowActions.SelectWindow());
			window.Register();
		}
	}
}