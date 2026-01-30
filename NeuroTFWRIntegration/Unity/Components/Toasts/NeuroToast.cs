
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
		if (Initialized)
		{
			Utilities.Logger.Error($"This toast has already been initialized");
			return;
		}
		InitCore();
		
		SetText("ContentsContainer/DescriptionText", text);
	}
}