using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public abstract class BaseToast : MonoBehaviour
{
	private CanvasGroup? _canvasGroup;
	
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
		return;
	}
	
	public virtual void CloseClicked()
	{
		Utilities.Logger.Info($"pressed on close clicked");
		Plugin.Instance?.StartCoroutine(Fade(0, 0));
	}
	
	public IEnumerator Fade(float waitTime, float duration)
	{
		if (_canvasGroup is null) yield break;
		float currentWaitTime = 0f;

		while (currentWaitTime < waitTime)
		{
			currentWaitTime += Time.deltaTime;
			yield return null;
		}
		float startAlpha = _canvasGroup.alpha;
		float t = 0f;

		while (t < duration)
		{
			t += Time.deltaTime;
			_canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
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
}