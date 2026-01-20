using System;
using System.IO;
using BepInEx;
using NeuroTFWRIntegration.Unity.Components.Chat;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeuroTFWRIntegration.Unity;

public static class LoadComponents
{
	private static readonly string ChatPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "neuro-chat-canvas");
	private static readonly string ContainerPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "toastcontainer");

	public static void LoadStartingComponents()
	{
		if (ConfigHandler.Toasts.Entry.Value != Toasts.Disabled)
		{
			AddToastsContainer();
		}
		
		if (ConfigHandler.NeuroChat.Entry.Value)
		{
			Utilities.Logger.Info($"neuro chat value: {ConfigHandler.NeuroChat.Entry.Value}");
			AddChat();
		}
	}
	
	private static void AddToastsContainer()
	{
		var obj = CreateGameObject("ToastsContainer", ContainerPath, "Assets/ToastsContainer.prefab", typeof(ToastsManager));
		if (!obj)
		{
			Utilities.Logger.Error($"There was an error when creating the toasts container.");
			return;
		}

		Plugin.ToastContainer = obj;
	}
	
	private static void AddChat()
	{
		var obj = CreateGameObject("NeuroChatCanvas", ChatPath, "Assets/NeuroChatCanvas.prefab", typeof(NeuroChat));
		if (!obj)
		{
			Utilities.Logger.Error($"There was an error when creating NeuroChat.");
		}
	}

	private static GameObject? CreateGameObject(string gameObjectName, string bundlePath, string assetPath, Type componentType)
	{
		if (GameObject.Find(gameObjectName) is not null)
		{
			return null;
		}

		if (!File.Exists(bundlePath))
		{
			Utilities.Logger.Error($"could not find neuro-chat file.");
			return null;
		}
		
		AssetBundle bundle = AssetBundleHelper.GetAssetBundle(bundlePath);
		if (bundle is null)
		{
			throw new NullReferenceException("Toast's container AssetBundle was null.");
		}

		Utilities.Logger.Info($"creating chat");
		var chat = AssetBundleHelper.LoadBundle(bundlePath, assetPath);
		if (chat is null)
		{
			throw new NullReferenceException("container was null, there was an issue when loading it.");
		}
		chat.AddComponent(componentType);
		var overlay = GameObject.Find("OverlayUI");
		if (overlay is null)
		{
			throw new NullReferenceException($"There was an issue finding OverlayUI.");
		}
		
		Utilities.Logger.Info($"instantiating object");
		var chatInst = Object.Instantiate(chat, overlay.transform);
		
		chatInst.SetActive(true);
		AssetBundleHelper.UnloadBundle(bundlePath);
		return chatInst;
	}
}