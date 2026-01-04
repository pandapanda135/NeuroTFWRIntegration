using BepInEx.Configuration;

namespace NeuroTFWRIntegration;

public class ConfigBase<T>(ConfigDefinition section, T defaultValue, ConfigDescription? description = null)
{
	public ConfigEntry<T>? BaseToEntry()
	{
		return Plugin.Instance is null ? null : Plugin.Instance.Config.Bind(section, defaultValue, description);
	}
}

public enum ResearchMenuActions
{
	None,
	InMenu,
	OutOfMenu
}
public static class ConfigStrings
{
	public static readonly ConfigBase<string> WebsocketUrl = new(new("General", "WebsocketURL"), "",
		new("This is the url to use for the websocket, if this is not an empty string it will override the" +
		    " NEURO_SDK_WS_URL environment variable."));

	public static readonly ConfigBase<ResearchMenuActions> ResearchMenuActions =
		new(new("Actions", "ResearchMenuActions"), NeuroTFWRIntegration.ResearchMenuActions.None,
			new("This decides how much control Neuro get's over the research menu (this is the tech tree.)" +
			    "\n- None\nNothing is registered leaving all control up to the collab partner, this will send context about the options available when the menu is opened however." +
			    "\n- InMenu\nThis will mean that when the research menu is opened by the collab partner Neuro will be sent an actions force allowing her to select an upgrade." +
			    "\n- OutOfMenu\nThis will have an action to buy an upgrade registered at all times and will automate the opening of the menu and buying." +
			    " This is meant for if Neuro plays without a collab partner."));
	
	public static readonly ConfigBase<bool> Debug = new(new("Debug", "Debug"), false,
		new("You probably won't need this."));
}