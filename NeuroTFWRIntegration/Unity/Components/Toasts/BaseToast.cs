using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public abstract class BaseToast : MonoBehaviour
{
	private CanvasGroup? _canvasGroup;
	protected bool Initialized;
	
	protected void AwakeCore(string closeButtonPath = "")
	{
		if (closeButtonPath != "")
		{
			transform.Find(closeButtonPath).GetComponent<Button>().onClick.AddListener(CloseClicked);	
		}
		_canvasGroup = GetComponent<CanvasGroup>();
		if (_canvasGroup is null)
		{
			Utilities.Logger.Error($"could not find canvas group");
		}
	}

	protected void InitCore()
	{
		Initialized = true;
	}
	
	public virtual void CloseClicked()
	{
		Utilities.Logger.Info($"pressed on close clicked");
		Fade(0d, 0);
	}
	
	// I know this is ugly but it works
	public void Fade(double waitTime, double fadeOutDuration)
	{
		StartCoroutine(Fade((float)waitTime, (float)fadeOutDuration));
	}
	
	private IEnumerator Fade(float waitTime, float fadeOutDuration)
	{
		if (_canvasGroup is null) yield break;
		float currentWaitTime = 0f;

		while (currentWaitTime < waitTime)
		{
			currentWaitTime += Time.deltaTime;
			yield return null;
		}
		// Utilities.Logger.Info($"post wait");
		float startAlpha = _canvasGroup.alpha;
		float t = 0f;

		while (t < fadeOutDuration)
		{
			t += Time.deltaTime;
			_canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeOutDuration);
			yield return null;
		}
		
		Destroy(gameObject);
	}
	
	protected void SetText(string findPath, string text)
	{
		var descriptionTransform = transform.Find(findPath);
		if (descriptionTransform is null)
		{
			Utilities.Logger.Error($"description transform was null");
			return;
		}
		var textGui = descriptionTransform.GetComponent<TextMeshProUGUI>();
		textGui.text = text;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}