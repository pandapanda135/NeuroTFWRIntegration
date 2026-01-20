
namespace NeuroTFWRIntegration.Unity.Components.Toasts;

public class NeuroToast : BaseToast
{
	private void Awake()
	{
		AwakeCore("ContentsContainer/CloseButton");
		Fade(7.5f, 1f);
	}
	
	public void Init(string text)
	{
		InitCore();
		SetText("ContentsContainer/DescriptionText", text);
	}
}