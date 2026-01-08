using System;
using NeuroTFWRIntegration.Unity.Components.Toasts;
using TMPro;
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
	private void Awake()
	{
		AwakeCore("ContentsContainer/CloseButton");
		Plugin.Instance?.StartCoroutine(Fade(5f, 1f));
		Utilities.Logger.Info($"validation awake");
	}
	
	public void Init(string descriptionText, ValidationLevels level, Color? flavourColour = null)
	{
		SetText(descriptionText);
		SetValidationImage(level);
		if (flavourColour is not null)
		{
			SetFlavourColor(flavourColour.Value);
		}
		else
		{
			SetValidationColour(level);
		}

		InitCore();
	}

	public override void CloseClicked()
	{
		Utilities.Logger.Info($"close button clicked");
		Plugin.Instance?.StartCoroutine(Fade(0, 0));
	}

	private void SetValidationColour(ValidationLevels level)
	{
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
	
	private void SetValidationImage(ValidationLevels validation)
	{
		var find = transform.Find("ContentsContainer/WarningValidationImage");
		if (find is null)
		{
			Utilities.Logger.Error($"issue finding valdation image as it was null");
			return;
		}
		var img = find.GetComponent<Image>();
		if (img is null)
		{
			Utilities.Logger.Error($"Issue setting left image as it was null");
			return;
		}
		img.sprite = new();
	}

	private void SetFlavourColor(Color color)
	{
		var flavour = transform.Find("FlavourColour");
		flavour.GetComponent<Image>().color = color;
	}

	private void SetText(string text)
	{
		var descriptionTransform = transform.Find("ContentsContainer/DescriptionText");
		var textGui = descriptionTransform.GetComponent<TextMeshProUGUI>();
		textGui.text = text;
	}
}