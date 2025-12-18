using BepInEx.Configuration;

namespace NeuroTFWRIntegration;

public class ConfigBase<T>(ConfigDefinition section, T defaultValue, ConfigDescription? description = null)
{
	public ConfigEntry<T>? BaseToEntry()
	{
		return Plugin.Instance is null ? null : Plugin.Instance.Config.Bind(section, defaultValue, description);
	}
}

public static class ConfigStrings
{
	public static readonly ConfigBase<string> WebsocketUrl = new(new("General", "WebsocketURL"), "",
		new("This is the url to use for the websocket, if this is not an empty string it will override the" +
		    " NEURO_SDK_WS_URL environment variable."));

	public static readonly ConfigBase<bool> ResearchMenuActions =
		new(new("Actions", "ResearchMenuActions"), false,
			new("This will register actions for Neuro to be able to interact with the research menu," +
			    " I recommend not enabling this if there is a person playing with her. Even if this is enabled context about the upgrades is still sent."));
	

	
	public static readonly ConfigBase<bool> Debug = new(new("Debug", "Debug"), false,
		new("You probably won't need this."));


}