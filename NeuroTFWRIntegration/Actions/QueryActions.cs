using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.ContextHandlers;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

public static class QueryActions
{
	public class QueryItems : NeuroActionWrapper
	{
		public override string Name => "query_items";
		protected override string Description => "Get the amount of each item you have.";
		protected override JsonSchema Schema => new();
		protected override ExecutionResult Validate(ActionJData actionData)
		{
			if (!WorkspaceState.Sim.GetInventory().ItemIds().Any()) return ExecutionResult.Failure($"You do not have any items, you should stop being poor.");
			
			return ExecutionResult.Success($"");
		}

		protected override void Execute()
		{
			string contextMessage = "# Items";
			foreach (var id in WorkspaceState.Sim.GetInventory().ItemIds())
			{
				// this is what the game does for tool tips
				string itemName = CodeUtilities.ToUpperSnake(ResourceManager.GetItem(id).itemName);
				double quantity = WorkspaceState.Sim.GetNumItem(id);
				contextMessage += $"\n## {itemName}\n- quantity: {quantity}";
			}
			
			Context.Send($"{contextMessage}");
		}
	}
	
	public class QueryDrone : NeuroActionWrapper
	{
		public override string Name => "query_drone";
		protected override string Description => "Sends information about your drones.";
		protected override JsonSchema Schema => new();
		protected override ExecutionResult Validate(ActionJData actionData)
		{
			return ExecutionResult.Success($"");
		}

		protected override void Execute()
		{
			string contextMessage = $"# Drones";
			
			foreach (var drone in WorkspaceState.Farm.drones)
			{
				contextMessage += $"\n## Drone {drone.DroneId}\n- Position: {drone.pos}\n- Current state: {drone.droneState}\n- Hat: {drone.hat.hatSO.hatName}";	
			}
			
			Context.Send(contextMessage);
		}
	}

	public class QueryWorld : NeuroActionWrapper<Vector2Int?>
	{
		public override string Name => "query_world";
		protected override string Description =>
			$"Query information about the world, if both of the tile positions are not set the whole world will be sent, else the tile you provide." +
			$" The current map size is {WorkspaceState.Sim.GetWorldSize()} the initial index is 0.";
		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = [],
			Properties = new Dictionary<string, JsonSchema>
			{
				["tile_x"] = QJS.Type(JsonSchemaType.Integer),
				["tile_y"] = QJS.Type(JsonSchemaType.Integer)
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out Vector2Int? parsedData)
		{
			int? tileX = actionData.Data?.Value<int?>("tile_x");
			int? tileY = actionData.Data?.Value<int?>("tile_y");
			
			parsedData = null;
			if (tileX is null && tileY is null)
			{
				return ExecutionResult.Success($""); 
			}

			// if only one is null
			if (tileX is null || tileY is null)
			{
				return ExecutionResult.Failure($"You must either provide a valid tile or no tile.");
			}
			
			parsedData = new(tileX.Value, tileY.Value);
			if (!WorkspaceState.Farm.grid.IsWithinBounds((Vector2Int)parsedData))
				return ExecutionResult.Failure($"The tile you provided is not withing bounds.");
			
			return ExecutionResult.Success($"");
		}

		protected override void Execute(Vector2Int? parsedData)
		{
			string contextMessage = "# Tile Information";
			if (parsedData is not null)
			{
				contextMessage += GetTileString(parsedData.Value);
				Context.Send($"{contextMessage}");
				return;
			}
			
			for (int x = 0; x < WorkspaceState.Sim.GetWorldSize().x; x++)
			{
				for (int y = 0; y < WorkspaceState.Sim.GetWorldSize().y; y++)
				{
					contextMessage += $"{GetTileString(new Vector2Int(x, y))}";
				}
			}
			
			Context.Send(contextMessage);
		}

		private static string GetTileString(Vector2Int tile)
		{
			GridManager grid = WorkspaceState.Farm.grid;
			return $"\n## {tile}\n- Ground: {grid.grounds[tile].objectSO.objectName}\n- Entity: {grid.entities[tile].objectSO.name}";
		}
	}

	public class QueryBuiltin : NeuroActionWrapper<string?>
	{
		private readonly string[] _validOptions = ["entities", "items", "hats", "functions", "grounds"];
		public override string Name => "query_builtin";
		protected override string Description => "Query the built in features of this language, these can be variables, enums and functions." +
		                                         " If you want all to query all of the built-ins you should not provide an option.";
		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = [],
			Properties = new Dictionary<string, JsonSchema>
			{
				["option"] = QJS.Enum(_validOptions)
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string? resultData)
		{
			string? option = actionData.Data?.Value<string?>("option");

			resultData = null;
			if (string.IsNullOrEmpty(option))
			{
				return ExecutionResult.Success();
			}

			if (!_validOptions.Contains(option)) return ExecutionResult.Failure($"You did not provide a valid option.");

			resultData = option;
			return ExecutionResult.Success();
		}

		protected override void Execute(string? resultData)
		{
			if (string.IsNullOrEmpty(resultData))
			{
				ResourceContext.SendBuiltinContext();
				return;
			}

			switch (resultData)
			{
				case "entities":
					Context.Send($"# Entities{ListToSingular(ResourceContext.GetEntityStrings())}");
					break;
				case "items":
					Context.Send($"# Items{ListToSingular(ResourceContext.GetItemStrings())}");
					break;
				case "hats":
					Context.Send($"# Hats{ListToSingular(ResourceContext.GetHatStrings())}");
					break;
				case "functions":
					Context.Send($"# Functions{ListToSingular(ResourceContext.GetFunctionStrings())}");
					break;
				case "grounds":
					Context.Send($"# Grounds{ListToSingular(ResourceContext.GetGroundStrings())}");
					break;
				default:

					Context.Send($"Your request for {resultData} was not valid, if you want all of the possible builtins you should not specify anything.");
					ResourceContext.SendBuiltinContext();
					Utilities.Logger.Error($"Allowed invalid result data into execute: {resultData}");
					break;
			}
		}

		private static string ListToSingular(List<string> list)
		{
			return string.Join("", list.Select(s => $"\n- {s}"));
		}
	}
}