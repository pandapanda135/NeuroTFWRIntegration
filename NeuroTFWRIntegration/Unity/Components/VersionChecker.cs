using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NeuroTFWRIntegration.Unity.Components;

public class VersionChecker : MonoBehaviour
{
	private const string ReleaseUri =
		"https://raw.githubusercontent.com/pandapanda135/NeuroTFWRIntegration/refs/heads/main/version.json";
	private const string DefaultRepoURl = "https://github.com/pandapanda135/NeuroTFWRIntegration/releases";
	
	private static VersionInformation? _versionInformation;
	
	private TMP_Text? _isUpdateText;
	private TMP_Text? _installedVersion;
	private TMP_Text? _releaseVersion;
	private GameObject? _repoButtonObj;
	private Button? _repoButton;

	private Color _buttonEnabledColour;
	private Color _buttonDisabledColour;
	
	private void Awake()
	{
		transform.position = new Vector3(1920, 0, 0);
		transform.localScale = new Vector3(1, 1, 1);

		_isUpdateText = GameObject.Find("IsUpdated").GetComponent<TMP_Text>();
		_installedVersion = GameObject.Find("InstalledVersion").GetComponent<TMP_Text>();
		_releaseVersion = GameObject.Find("NewestVersion").GetComponent<TMP_Text>();
		_repoButtonObj = GameObject.Find("OpenURLButton");
		_repoButton = _repoButtonObj.GetComponent<Button>();
		_repoButton.onClick.AddListener(OpenRepo);
		
		ColorUtility.DoTryParseHtmlColor("#414141", out var disabled);
		_buttonDisabledColour = disabled;
		ColorUtility.DoTryParseHtmlColor("#60730d", out var enable);
		_buttonEnabledColour = enable;
	}

	private static void OpenRepo()
	{
		if (_versionInformation?.RedirectURL is null)
		{
			Utilities.Logger.Warning($"Couldn't get RedirectURL, going to default.");
			Application.OpenURL($"{DefaultRepoURl}/releases");
			return;
		}
		
		Application.OpenURL($"{_versionInformation.RedirectURL}/releases");
	}

	private void Start()
	{
		StartCoroutine(GetVersion(ReleaseUri));
		StartCoroutine(SetUiText());
	}

	private IEnumerator SetUiText()
	{
		_isUpdateText?.text = "Loading the version information.";
		_installedVersion?.text = $"V{LocalPluginInfo.PLUGIN_VERSION}";
		_releaseVersion?.text = $"Loading.";
		while (_versionInformation is null)
		{
			yield return null;
		}

		if (LocalPluginInfo.PLUGIN_VERSION != _versionInformation.LatestVersion)
		{
			_isUpdateText?.text = ":DinkDonk: You do not have the latest version installed. :DinkDonk:";
			_releaseVersion?.text = $"V{_versionInformation.LatestVersion}";
			_installedVersion?.color = Color.red;
			_repoButtonObj?.GetComponent<Image>().color = _buttonEnabledColour;
			yield break;
		}
		
		_repoButtonObj?.GetComponent<Image>().color = _buttonDisabledColour;
		_repoButton?.interactable = false;
		
		_isUpdateText?.text = "You are up-to-date!";
		_releaseVersion?.text = $"V{_versionInformation.LatestVersion}";
	}

	private IEnumerator GetVersion(string uri)
	{
		var request = UnityWebRequest.Get(uri);
		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Utilities.Logger.Error($"There was an issue getting the current version.\nThe result was: {request.result}\nThe error is:{request.error}");
			Destroy(gameObject);
			yield break;
		}

		var jsonText = request.downloadHandler.text;
		Utilities.Logger.Info(jsonText);
		
		try
		{
			_versionInformation = new VersionInformation(JObject.Parse(jsonText));
		}
		catch (Exception e)
		{
			Utilities.Logger.Error($"There was an error creating version information:\n{e}");
			// peak error handling
			Destroy(gameObject);
			yield break;
		}
		
		Utilities.Logger.Info($"final version: {_versionInformation.LatestVersion}   {_versionInformation.RedirectURL}");
	}

	private class VersionInformation
	{
		public VersionInformation(JObject obj)
		{
			ObjectToVersion(obj);
		}

		public string? LatestVersion;
		public string? RedirectURL;

		private void ObjectToVersion(JObject obj)
		{
			if (!obj.TryGetValue("Version", out var ver) || ver.Value<string>() is null)
			{
				throw new MissingVersionFieldException("Missing Version.");
			}

			if (!obj.TryGetValue("RedirectURL", out var url) || url.Value<string>() is null)
			{
				throw new MissingVersionFieldException("Missing RedirectURL.");
			}

			LatestVersion = ver.Value<string>() ?? string.Empty;
			RedirectURL = url.Value<string>() ?? string.Empty;
		}
	}

	private class MissingVersionFieldException(string message) : Exception
	{
		public override string Message { get; } = message;
	}
}