namespace NeuroTFWRIntegration.ContextHandlers;

public static class Strings
{
	public const string ErrorMessageContext = "There is an error in {0}, it's message is {1} it occurs on line {2} at column {3}.";
	
	public const string StartGameContext = "You have started playing the game \"The Farmer Was Replaced\", " +
	"this is a programming and automation game where you write code in a simple python-like language to acquire resources and advance in the tech tree. " +
	"The code you write is stored in \"windows\", these windows function similarly to files in a traditional code editor. " +
	"Certain features that are typically in python are either not available to be used when you start playing or are not available in this language. " +
	"You should try to only stick to only keywords that you have unlocked (you can get this information from actions like get_documentation and query_builtin)" +
	" and features that are very basic. So try to avoid more advanced features like f-strings as most of them wont work.";
}