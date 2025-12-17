using BepInEx.Logging;

namespace NeuroTFWRIntegration;

internal static class Logger
{
	private static ManualLogSource? _log;

	public static void SetLogger(ManualLogSource logger) => _log = logger;

	public static void Info(object message)
	{
		_log?.LogInfo(message);
	}

	public static void Warning(object message)
	{
		_log?.LogWarning(message);
	}

	public static void Error(object message)
	{
		_log?.LogError(message);
	}

	public static void Fatal(object message)
	{
		_log?.LogFatal(message);
	}
}