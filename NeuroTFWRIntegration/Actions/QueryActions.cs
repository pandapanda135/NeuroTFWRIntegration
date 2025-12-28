using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

public static class QueryActions
{
	public class QueryResources : NeuroAction
	{
		public override string Name => "query_resources";
		protected override string Description => "Get the amount of each resources you have.";
		protected override JsonSchema Schema => new();
		protected override ExecutionResult Validate(ActionJData actionData)
		{
			if (!WorkspaceState.Sim.GetInventory().ItemIds().Any()) return ExecutionResult.Failure($"You do not have any resources, you should stop being poor.");
			
			return ExecutionResult.Success($"");
		}

		protected override void Execute()
		{
			string contextMessage = "# Resources";
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
	
	public class QueryDrone : NeuroAction
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
				contextMessage += $"\n## Drone {drone.DroneId}\n- Position: {drone.pos}\n- Hat: {drone.hat.hatSO.hatName}\n- Current state: {drone.droneState}";	
			}
			
			Context.Send(contextMessage);
		}
	}

	public class QueryWorld : NeuroAction<Vector2Int?>
	{
		public override string Name => "query_world";
		protected override string Description =>
			$"Query information about the world, if full world is set the whole world will be sent otherwise just the tile you want." +
			$" The current map size is {WorkspaceState.Sim.GetWorldSize()} the initial index is 0.";
		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["full_world"],
			Properties = new Dictionary<string, JsonSchema>
			{
				["full_world"] = QJS.Type(JsonSchemaType.Boolean),
				["tile_x"] = QJS.Type(JsonSchemaType.Integer),
				["tile_y"] = QJS.Type(JsonSchemaType.Integer)
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out Vector2Int? parsedData)
		{
			// we include full world as integers default to 0 if not set and I can't just assume if she wants 0,0 she is not setting anything.
			bool? fullWorld = actionData.Data?.Value<bool>("full_world");
			int? tileX = actionData.Data?.Value<int>("tile_x");
			int? tileY = actionData.Data?.Value<int>("tile_y");
			
			parsedData = null;
			if (fullWorld is null) return ExecutionResult.Failure($"You must provide if you want the full world.");
			
			if (fullWorld.Value)
			{
				return ExecutionResult.Success($""); 
			}
			
			if ((tileX is null && tileY is not null) || (tileX is not null && tileY is null))
			{
				return ExecutionResult.Failure($"You must either provide a full valid tile or no tile.");
			}
			
			parsedData = new(tileX!.Value, tileY!.Value);
			
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
					Logger.Info($"adding {x},{y}");
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
}