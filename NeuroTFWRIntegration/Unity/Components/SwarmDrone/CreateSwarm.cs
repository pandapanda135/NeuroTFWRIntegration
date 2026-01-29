using System.Linq;
using HarmonyLib;
using NeuroTFWRIntegration.Utilities;
using UnityEngine;

namespace NeuroTFWRIntegration.Unity.Components.SwarmDrone;

public static class CreateSwarm
{
	private static Mesh? _defaultDroneMesh;
	
	// private static Mesh? _defaultPropellerMesh;

	private static Vector3 _defaultPropellerOffset1;
	private static Vector3 _defaultPropellerOffset2;
	private static Vector3 _defaultPropellerOffset3;
	private static Vector3 _defaultPropellerOffset4;

	public static HatSO? HatSo;

	private static Mesh? _swarmDroneMesh;
	private static Material? _defaultDroneMaterial;
	private static Material? _swarmDroneMaterial;

	private static void CreateSwarmHat()
	{
		if (HatSo is not null) return;
		
		var hatSo = ScriptableObject.CreateInstance<HatSO>();
		hatSo.hatName = "swarm";
		hatSo.className = "Hat";
		hatSo.droneFlyHeight = 2f;
		hatSo.hidden = false;
		hatSo.isGolden = false;
		hatSo.preventWrapping = false;
		// I kinda prefer how this looks so we are doing it this way.
		hatSo.rotateDroneToMove = true;

		HatSo = hatSo;
	}
	
	private static readonly string GymBagPath = AssetBundleHelper.GetBundlePath("gym-bag-drone");
	private static readonly string MaterialPath = AssetBundleHelper.GetBundlePath("drone-material");
	private static void SetHatInformation()
	{
		if (HatSo is null)
		{
			CreateSwarmHat();
		}
		_defaultDroneMesh = WorkspaceState.FarmRenderer.droneMesh;
		_defaultDroneMaterial = WorkspaceState.FarmRenderer.material;
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
			return;
		}
		_swarmDroneMesh = asset.GetComponent<MeshFilter>().mesh;
		if (_swarmDroneMesh is null)
		{
			Utilities.Logger.Error($"error getting hat mesh.");
			return;
		}
		
		AssetBundleHelper.GetAssetBundle(MaterialPath);
		Material? mat = AssetBundleHelper.LoadBundle(MaterialPath,"Assets/Models/DroneMaterial.mat", true);
		if (mat is null)
		{
			Utilities.Logger.Error($"error getting drone material.");
			return;	
		}

		_swarmDroneMaterial = mat;
		
		HatSo?.hatMesh = new();
		HatSo?.sound1 = ResourceManager.GetAllHats().ToArray()[0].sound1;
		HatSo?.sound2 = ResourceManager.GetAllHats().ToArray()[0].sound2;
		
		ResourceManager.hats.TryAdd("swarm_hat", HatSo);
	}
	
	[HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.LoadAll))]
	[HarmonyPostfix]
	public static void LoadPostfix()
	{
		Utilities.Logger.Info($"Loading drone hat.");
		SetHatInformation();
	}
	
	[HarmonyPatch(typeof(Drone), nameof(Drone.ChangeHat))]
	[HarmonyPrefix]
	public static void HideDroneOnHatChange(Drone __instance, HatSO hatSO)
	{
		if (!hatSO.hidden && !WorkspaceState.Farm.IsUnlocked(hatSO.hatName))
			return;
		
		if (_swarmDroneMesh is null || _swarmDroneMaterial is null)
		{
			Utilities.Logger.Error($"hat asset prefab was null.");
			return;
		}

		WorkspaceState.FarmRenderer.droneMesh = hatSO != HatSo ? _defaultDroneMesh : _swarmDroneMesh;
		WorkspaceState.FarmRenderer.material =
			WorkspaceState.FarmRenderer.material == _swarmDroneMaterial ? _defaultDroneMaterial : _swarmDroneMaterial;
		ModifyPropellers(hatSO != HatSo);
	}

	private const float SwarmYOffset = 0.5f;
	private const float SwarmXOffset = 0.285f;
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