
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
		if (Initialized)
		{
			Utilities.Logger.Error($"This toast has already been initialized");
			return;
		}
		
		SetText("ContentsContainer/DescriptionText", text);
	}
}