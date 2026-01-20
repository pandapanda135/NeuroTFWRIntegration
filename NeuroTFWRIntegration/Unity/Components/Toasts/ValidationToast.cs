using System;
using System.Collections;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public class ValidationToast : BaseToast
{
	public enum ValidationLevels
	{
		Success,
		Warning,
		Failure,
		Standard
	}

	private CanvasGroup? _successImage;
	private CanvasGroup? _warningImage;
	private CanvasGroup? _errorImage;

	private static readonly string ValidationContainer = "ContentsContainer/ValidationImageContainer/{0}";
	private void Awake()
	{
		AwakeCore("ContentsContainer/CloseButton");
		Fade(5f, 1f);
		
		_successImage = transform.Find(string.Format(ValidationContainer, "SuccessValidationImage")).GetComponent<CanvasGroup>();
		_warningImage = transform.Find(string.Format(ValidationContainer, "WarningValidationImage")).GetComponent<CanvasGroup>();
		_errorImage = transform.Find(string.Format(ValidationContainer, "ErrorValidationImage")).GetComponent<CanvasGroup>();
	}
	
	public void Init(string descriptionText, ValidationLevels level, Color? flavourColour = null)
	{
		InitCore();
		
		SetValidationImage(level);
		SetText("ContentsContainer/DescriptionText", descriptionText);
		if (flavourColour is not null)
		{
			SetFlavourColor(flavourColour.Value);
		}
		else
		{
			SetValidationColour(level);
		}

	}

	private void SetValidationColour(ValidationLevels level)
	{
		if (!Initialized) return;
		
		switch (level)
		{
			case ValidationLevels.Success:
				SetFlavourColor(Color.green);
				break;
			case ValidationLevels.Warning:
				SetFlavourColor(Color.yellow);
				break;
			case ValidationLevels.Failure:
				SetFlavourColor(Color.red);
				break;
			case ValidationLevels.Standard:
				SetFlavourColor(Color.gray);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(level), level, null);
		}
	}
	
	// this may or may not end up getting used.
	private void SetValidationImage(ValidationLevels level)
	{
		if (!Initialized) return;
		
		DisableAllImages();
		
		switch (level)
		{
			case ValidationLevels.Success:
				SetImage(_successImage, true);
				break;
			case ValidationLevels.Warning:
				SetImage(_warningImage, true);
				break;
			case ValidationLevels.Failure:
				SetImage(_errorImage, true);
				break;
			case ValidationLevels.Standard:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(level), level, null);
		}
	}
	
	private void DisableAllImages()
	{
		SetImage(_successImage, false);
		SetImage(_warningImage, false);
		SetImage(_errorImage, false);
	}

	private static void SetImage(CanvasGroup? cg, bool enable)
	{
		if (!cg) return;

		cg.alpha = enable ? 1f : 0f;
		cg.interactable = enable;
		cg.blocksRaycasts = enable;
	}


	private void SetFlavourColor(Color color)
	{
		if (!Initialized) return;
		
		var flavour = transform.Find("FlavourColour");
		flavour.GetComponent<Image>().color = color;
	}
}