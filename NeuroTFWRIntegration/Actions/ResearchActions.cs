using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroTFWRIntegration.Utilities;

namespace NeuroTFWRIntegration.Actions;

public static class ResearchActions
{
	public class BuyUpgrade : NeuroAction<string>
	{
		public override string Name => "buy_research_box";
		protected override string Description => "Buy a research box.";

		protected override JsonSchema? Schema => new()
		{
			Type = JsonSchemaType.Object,
			Required = ["name"],
			Properties = new()
			{
				["name"] = QJS.Enum(WorkspaceState.Sim.researchMenu.allBoxes.Where(kvp => kvp.Value.unlockState is
					UnlockBox.UnlockState.Unlocked or UnlockBox.UnlockState.Unlockable or UnlockBox.UnlockState.Upgradable)
					.Select(kvp => kvp.Value.unlockSO.unlockName).ToList())
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
			
			if (!WorkspaceState.Sim.researchMenu.allBoxes.Where(kvp => kvp.Value.unlockState is
				    UnlockBox.UnlockState.Unlocked or UnlockBox.UnlockState.Unlockable
				    or UnlockBox.UnlockState.Upgradable)
			    .Select(kvp => kvp.Value.unlockSO.unlockName).Contains(name))
			{
				return ExecutionResult.Failure($"The name you provided is not a valid upgrade or unlock.");
			}
			
			var (_, box) = WorkspaceState.Sim.researchMenu.allBoxes.Where(kvp => kvp.Value.unlockState is
					UnlockBox.UnlockState.Unlocked or UnlockBox.UnlockState.Unlockable
					or UnlockBox.UnlockState.Upgradable)
				.First(kvp => kvp.Value is not null && kvp.Value.unlockSO.unlockName == name);

			if (box is null) return ExecutionResult.Failure($"The name you provided is not a valid upgrade or unlock.");
			foreach (var item in box.unlockSO.unlockCost.serializeList)
			{
				if (WorkspaceState.Sim.inv.getItemBlock().serializeList is null) continue;
				
				if (WorkspaceState.Sim.inv.getItemBlock().serializeList.Where(invItem => item.name == invItem.name).Any(invItem => invItem.nr < item.nr))
				{
					return ExecutionResult.Failure($"You do not have enough {item.name} to by this item.");
				}
			}

			Logger.Info($"success execution");
			parsedData = name;
			return ExecutionResult.Success($"Unlocking {name}");
		}

		protected override void Execute(string? parsedData)
		{
			if (parsedData is null) return;

			var (_, box) = WorkspaceState.Sim.researchMenu.allBoxes.Where(kvp => kvp.Value.unlockState is
					UnlockBox.UnlockState.Unlocked or UnlockBox.UnlockState.Unlockable
					or UnlockBox.UnlockState.Upgradable)
				.First(kvp => kvp.Value.unlockSO.unlockName == parsedData);
			
			box.ButtonClicked();

			// if this has docs, they should open by themselves after being bought.
			if (!string.IsNullOrEmpty(box.unlockSO.docs))
				return;
			
			WorkspaceState.Sim.researchMenu.OpenCloseMenu();
		}
	}
}