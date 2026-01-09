
namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public class NeuroToast : BaseToast
{
	private void Awake()
	{
		AwakeCore("ContentsContainer/CloseButton");
		Plugin.Instance?.StartCoroutine(Fade(5f, 1f));
	}
	
	public void Init(string text)
	{
		SetText("ContentsContainer/DescriptionText", text);
		InitCore();
	}
}