using BepInEx.Configuration;

namespace NeuroTFWRIntegration;

public class ConfigBase<T>(ConfigDefinition section, T defaultValue, ConfigDescription description = null)
{
	public ConfigEntry<T> BaseToEntry()
	{
		return Plugin.Instance.Config.Bind(section, defaultValue, description);
	}
}

public static class ConfigStrings
{
	public static readonly ConfigBase<string> WebsocketUrl = new(new("General", "WebsocketURL"), "",
		new("This is the url to use for the websocket, if this is not an empty string it will override the" +
		    " NEURO_SDK_WS_URL environment variable."));
	public static readonly ConfigBase<bool> Debug = new(new("Debug", "Debug"), false,
		new("You probably won't need this."));

	
}