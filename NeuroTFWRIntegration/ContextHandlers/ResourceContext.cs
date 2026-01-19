using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Messages.Outgoing;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.ContextHandlers;

public class ResourceContext
{
	public static void SendBuiltinContext()
	{
		var functionNames = GetFunctionStrings();
		var items = GetItemStrings();
		var hats = GetHatStrings();
		var entities = GetEntityStrings();
		var ground = GetGroundStrings();
		
		Context.Send($"These are some built in parts of this language\n" +
		             $"# Functions{string.Join("", functionNames.Select(s => $"\n- {s}"))}\n" +
		             $"# Items{string.Join("", items.Select(s => $"\n- {s}"))}\n" +
		             $"# Hats{string.Join("", hats.Select(s => $"\n- {s}"))}\n" +
		             $"# Entities{string.Join("", entities.Select(s => $"\n- {s}"))}\n" +
		             $"# Grounds{string.Join("",ground.Select(s => $"\n- {s}"))}");
	}

	public static List<string> GetGroundStrings()
	{
		return (from obj in ResourceManager.GetAllFarmObjects() where WorkspaceState.Sim.IsUnlocked(obj.objectName) && obj.isGround select obj.ToString()).ToList();
	}

	public static List<string> GetEntityStrings()
	{
		return (from obj in ResourceManager.GetAllFarmObjects() where WorkspaceState.Sim.IsUnlocked(obj.objectName) && !obj.isGround select obj.ToString()).ToList();
	}

	public static List<string> GetItemStrings()
	{
		return (from item in ResourceManager.GetAllItems() where WorkspaceState.Sim.IsUnlocked(item.itemName)
			select item.ToString()).ToList();
	}

	public static List<string> GetHatStrings()
	{
		return (from hat in ResourceManager.GetAllHats() where WorkspaceState.Sim.IsUnlocked(hat.hatName) && !hat.hidden select hat.ToString()).ToList();
	}

	// TODO: find way to get function's arguments and their types, I spent some time trying this and I don't think there is a way to do this without just hardcoding everything :(. 
	public static List<string> GetFunctionStrings()
	{
		return (from kvp in BuiltinFunctions.Functions where WorkspaceState.Sim.IsUnlocked(kvp.Key) select kvp.Value.functionName).ToList();
	}
}