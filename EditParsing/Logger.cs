using System;

namespace EditParsing;

public static class Logger
{	
	public static void Info(object message)
	{
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine(message);
	}

	public static void Warning(object message)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(message);
	}

	public static void Error(object message)
	{
		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.WriteLine(message);
	}

	public static void Fatal(object message)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
	}
}