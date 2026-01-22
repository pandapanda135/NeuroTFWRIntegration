using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Unity.Components.SwarmDrone;

public static class CreateDrone
{
	private static Mesh? _defaultDroneMesh;
	// private static Mesh? _defaultPropellerMesh;

	private static Vector3 _defaultPropellerOffset1;
	private static Vector3 _defaultPropellerOffset2;
	private static Vector3 _defaultPropellerOffset3;
	private static Vector3 _defaultPropellerOffset4;

	private static HatSO? _hatSo;

	private static Mesh? _swarmDroneMesh;
	// private static Material? SwarmDroneMaterial;

	private static void CreateSwarmHat()
	{
		if (_hatSo is not null) return;
		
		var hatSo = ScriptableObject.CreateInstance<HatSO>();
		hatSo.hatName = "swarm";
		hatSo.className = "Hat";
		hatSo.droneFlyHeight = 2f;
		hatSo.hidden = false;
		hatSo.isGolden = false;
		hatSo.preventWrapping = false;
		hatSo.rotateDroneToMove = false;

		_hatSo = hatSo;
	}
	
	private static readonly string GymBagPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "gym-bag-drone");
	private static void SetHatInformation()
	{
		if (_hatSo is null)
		{
			CreateSwarmHat();
		}
		_defaultDroneMesh = WorkspaceState.FarmRenderer.droneMesh;
		// _defaultPropellerMesh = WorkspaceState.FarmRenderer.propellerMesh;
		_defaultPropellerOffset1 = WorkspaceState.FarmRenderer.propellerOffset1;
		_defaultPropellerOffset2 = WorkspaceState.FarmRenderer.propellerOffset2;
		_defaultPropellerOffset3 = WorkspaceState.FarmRenderer.propellerOffset3;
		_defaultPropellerOffset4 = WorkspaceState.FarmRenderer.propellerOffset4;
		
		AssetBundleHelper.GetAssetBundle(GymBagPath);
		var asset = AssetBundleHelper.LoadBundle(GymBagPath,"Assets/Models/gym_bag_drone.fbx");
		if (asset is null)
		{
			Utilities.Logger.Error($"hat asset prefab was null.");
		}
		
		_swarmDroneMesh = asset?.GetComponent<MeshFilter>().mesh;
		if (_swarmDroneMesh is null)
		{
			Utilities.Logger.Error($"error getting hat mesh.");
			return;
		}
		Utilities.Logger.Info($"mesh rotation: {asset?.transform.rotation}");
		
		_hatSo?.hatMesh = new();
		_hatSo?.sound1 = ResourceManager.GetAllHats().ToArray()[0].sound1;
		_hatSo?.sound2 = ResourceManager.GetAllHats().ToArray()[0].sound2;
		
		ResourceManager.hats.TryAdd("swarm_hat", _hatSo);
		MainSim.Inst.UnlockHat(_hatSo);
	}
	
	[HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.LoadAll))]
	[HarmonyPostfix]
	public static void LoadPostfix()
	{
		Utilities.Logger.Warning($"post load all");
		SetHatInformation();
	}
	
	[HarmonyPatch(typeof(Drone), nameof(Drone.ChangeHat))]
	[HarmonyPrefix]
	public static void HideDroneOnHatChange(Drone __instance, HatSO hatSO)
	{
		if (!hatSO.hidden && !WorkspaceState.Farm.IsUnlocked(hatSO.hatName))
			return;
		
		// this should already be loaded
		if (hatSO != _hatSo)
		{
			Utilities.Logger.Info($"default drone: {_defaultDroneMesh}");
			WorkspaceState.FarmRenderer.droneMesh = _defaultDroneMesh;
			ModifyPropellers(true);
			return;
		}
		
		if (_swarmDroneMesh is null)
		{
			Utilities.Logger.Error($"hat asset prefab was null.");
			return;
		}

		WorkspaceState.FarmRenderer.droneMesh = _swarmDroneMesh;
		ModifyPropellers(false);
	}

	private const float SwarmYOffset = 0.5f;
	private const float SwarmXOffset = 0.21f;
	private const float SwarmZOffset = 0.28f;
	private static void ModifyPropellers(bool setDefault)
	{
		if (setDefault)
		{
			WorkspaceState.FarmRenderer.propellerOffset1 = _defaultPropellerOffset1;
			WorkspaceState.FarmRenderer.propellerOffset2 = _defaultPropellerOffset2;
			WorkspaceState.FarmRenderer.propellerOffset3 = _defaultPropellerOffset3;
			WorkspaceState.FarmRenderer.propellerOffset4 = _defaultPropellerOffset4;
			return;
		}

		// if they have already been moved.
		if (WorkspaceState.FarmRenderer.propellerOffset1 != _defaultPropellerOffset1)
			return;

		// this is the left side
		// move it between the two propellers
		WorkspaceState.FarmRenderer.propellerOffset2.y -= SwarmYOffset;
		// make closer to bands
		WorkspaceState.FarmRenderer.propellerOffset2.x -= SwarmXOffset;
		// increase height
		WorkspaceState.FarmRenderer.propellerOffset2.z += SwarmZOffset;
		WorkspaceState.FarmRenderer.propellerOffset3 = WorkspaceState.FarmRenderer.propellerOffset2;
		
		// this is the right side
		WorkspaceState.FarmRenderer.propellerOffset1.y -= SwarmYOffset;
		WorkspaceState.FarmRenderer.propellerOffset1.x += SwarmXOffset;
		WorkspaceState.FarmRenderer.propellerOffset1.z += SwarmZOffset;
		WorkspaceState.FarmRenderer.propellerOffset4 = WorkspaceState.FarmRenderer.propellerOffset1;
	}
}