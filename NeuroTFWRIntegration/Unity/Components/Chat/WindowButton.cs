using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroTFWRIntegration.Unity.Components.Chat;

public class WindowButton : MonoBehaviour
{
	public string? displayString;
	public CodeWindow? codeWindow;
	public bool selected;
	
	private Toggle? _toggle;
	private TMP_Text? _text;
	private Graphic? _graphic;
	
	private readonly Color _onColor = Color.green;
	private readonly Color _onTextColor = Color.black;
	private readonly Color _offColor = Color.gray;
	private readonly Color _offTextColor = Color.white;
	
	private void Awake()
	{
		_toggle = GetComponentInChildren<Toggle>();
		_text = GetComponentInChildren<TMP_Text>();
		_graphic = _toggle.targetGraphic;
		_toggle.onValueChanged.AddListener(ValueChanged);
		_toggle.onValueChanged.AddListener(UpdateVisual);
		UpdateVisual(_toggle.isOn);
	}
	
	private void ValueChanged(bool value)
	{
		selected = value;
	}

	private void UpdateVisual(bool isOn)
	{
		_graphic?.color = isOn ? _onColor : _offColor;
		_text?.color = isOn ? _onTextColor : _offTextColor;
	}

	public void SetDisplay(KeyValuePair<string, CodeWindow> kvp)
	{
		displayString = kvp.Key;
		codeWindow = kvp.Value;

		GetComponentInChildren<TMP_Text>()?.text = displayString;
	}
}