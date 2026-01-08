using System.Collections;
using System.Collections.Generic;
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

	public abstract void CloseClicked();
	
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
		
		DestroyImmediate(this);
	}
}