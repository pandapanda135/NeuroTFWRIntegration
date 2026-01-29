using System;
using System.IO;
using NeuroTFWRIntegration.Unity.Components;
using NeuroTFWRIntegration.Unity.Components.Chat;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeuroTFWRIntegration.Unity;

public static class LoadComponents
{
	public static GameObject? NeuroChat;

	public static VersionChecker.VersionInformation? CachedVersion = null;
	
	private static readonly string ChatPath = AssetBundleHelper.GetBundlePath("neuro-chat-canvas");
	private static readonly string ContainerPath = AssetBundleHelper.GetBundlePath("toastcontainer");
	private static readonly string VersionPath = AssetBundleHelper.GetBundlePath("version-checker");

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

		if (ConfigHandler.VersionChecking.Entry.Value)
		{
			AddVersion();
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
			return;
		}

		NeuroChat = obj;
	}

	private static void AddVersion()
	{
		var obj = CreateGameObject("VersionCanvas", VersionPath, "Assets/VersionCanvas.prefab", typeof(VersionChecker), GameObject.Find("Menu"));
		if (!obj)
		{
			Utilities.Logger.Error($"There was an error when creating the Version checker.");
		}
	}

	private static GameObject? CreateGameObject(string gameObjectName, string bundlePath, string assetPath, Type componentType, GameObject? parent = null)
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
		var overlay = parent ?? GameObject.Find("OverlayUI");
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