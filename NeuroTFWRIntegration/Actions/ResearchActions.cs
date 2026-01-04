using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Actions;

public static class ResearchActions
{
	public class BuyUpgrade : NeuroAction<string>
	{
		public override string Name => "buy_research_box";
		protected override string Description => "Buy a research box.";

		protected override JsonSchema Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["name"],
			Properties = new()
			{
				["name"] = QJS.Enum(WorkspaceState.UnlockableBoxes.Select(kvp => kvp.Value.unlockSO.unlockName).ToList())
			}
		};
		protected override ExecutionResult Validate(ActionJData actionData, out string? parsedData)
		{
			string? name = actionData.Data?.Value<string>("name");

			parsedData = null;
			if (name is null)
			{
				return ExecutionResult.Failure($"You did not provide a valid value.");
			}

			var validBoxes = WorkspaceState.UnlockableBoxes;
			if (!validBoxes.Select(kvp => kvp.Value.unlockSO.unlockName).Contains(name))
			{
				return ExecutionResult.Failure($"The name you provided is not a valid upgrade or unlock.");
			}
			
			var (_, box) = validBoxes.First(kvp => kvp.Value is not null && kvp.Value.unlockSO.unlockName == name);

			if (box is null) return ExecutionResult.Failure($"The name you provided is not a valid upgrade or unlock.");
			foreach (var item in box.unlockSO.unlockCost.serializeList)
			{
				if (WorkspaceState.Sim.inv.getItemBlock().serializeList is null) continue;
				
				if (WorkspaceState.Sim.inv.getItemBlock().serializeList.Where(invItem => item.name == invItem.name).Any(invItem => invItem.nr < item.nr))
				{
					return ExecutionResult.Failure($"You do not have enough {item.name} to by this item.");
				}
			}

			Utilities.Logger.Info($"success execution");
			parsedData = name;
			return ExecutionResult.Success($"Unlocking {name}");
		}

		protected override void Execute(string? parsedData)
		{
			if (parsedData is null) return;
			Plugin.Instance?.StartCoroutine(ExecuteRoutine(parsedData));
		}

		private static IEnumerator ExecuteRoutine(string parsedData)
		{
			if (Plugin.ResearchMenuActions?.Value == ResearchMenuActions.OutOfMenu)
			{
				if (!WorkspaceState.ResearchMenuOpen)
					WorkspaceState.Sim.researchMenu.OpenCloseMenu();
				yield return new WaitForSeconds(1);
			}

			var (_, box) = WorkspaceState.UnlockableBoxes.First(kvp => kvp.Value.unlockSO.unlockName == parsedData);
			
			box.ButtonClicked();
			
			// if this has docs, they should open by themselves after being bought. No good way to test that accounts for upgradable boxes so we wait
			yield return new WaitForSeconds(1);

			if (WorkspaceState.ResearchMenuOpen)
				WorkspaceState.Sim.researchMenu.OpenCloseMenu();
		}
	}
	
	public class QueryUpgrades : NeuroAction
	{
		public override string Name => "query_upgrades";
		protected override string Description => "Query the upgrades that are available in the tech tree.";
		protected override JsonSchema Schema => new();
		protected override ExecutionResult Validate(ActionJData actionData)
		{
			return ExecutionResult.Success();
		}

		protected override void Execute()
		{
			Context.Send($"# Upgrades\n{GetBoxesText()}");
		}
	}

	public static string GetBoxesText(bool onlyUnlockable = false)
	{
		var boxes = onlyUnlockable ? WorkspaceState.UnlockableBoxes : WorkspaceState.ValidBoxes;
		return string.Join("\n", boxes.Select<KeyValuePair<string, UnlockBox>, string>(kvp =>
			{
				string text = $"## {kvp.Value.unlockSO.unlockName}\n### Description\n{StringUtils.RemoveTextMeshTags(Localizer.Localize(kvp.Value.unlockSO.description))}" +
				              $"\n### {(kvp.Value.unlockState is UnlockBox.UnlockState.Upgradable ? "Upgrade Cost" : "Unlock Cost")}";
				foreach (var item in kvp.Value.unlockSO.unlockCost.serializeList)
				{
					text += $"\n- {item.name} amount: {item.nr}";
				}

				if (!string.IsNullOrEmpty(kvp.Value.unlockSO.docs))
				{
					text += $"\n### Docs Path\n {kvp.Value.unlockSO.docs}";
				}

				return text;
			})
		);
	}
}