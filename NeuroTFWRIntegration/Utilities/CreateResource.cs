using System.IO;
using System.Linq;
using BepInEx;
using NeuroTFWRIntegration.Unity;
using UnityEngine;

namespace NeuroTFWRIntegration.Utilities;

public static class CreateResource
{
	public static Mesh? DefaultDroneMesh;
	public static Mesh? DefaultPropellerMesh;

	public static HatSO? HatSo;

	public static Mesh? SwarmDroneMesh;
	public static Material? SwarmDroneMaterial;

	public static void CreateSwarmHat()
	{
		if (HatSo is not null) return;

		var hatSo = ScriptableObject.CreateInstance<HatSO>();
		hatSo.hatName = "swarm";
		hatSo.className = "Hat";
		hatSo.droneFlyHeight = 2f;
		hatSo.hidden = false;
		hatSo.isGolden = false;
		hatSo.preventWrapping = false;
		hatSo.rotateDroneToMove = false;

		HatSo = hatSo;
	}
	
	public static readonly string GymBagPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "gym-bag");
	public static void CreateNewHat()
	{
		if (HatSo is null)
		{
			CreateSwarmHat();
		}
		DefaultDroneMesh = WorkspaceState.FarmRenderer.droneMesh;
		DefaultPropellerMesh = WorkspaceState.FarmRenderer.propellerMesh;
		
		AssetBundleHelper.GetAssetBundle(GymBagPath);
		var asset = AssetBundleHelper.LoadBundle(GymBagPath,"Assets/Models/Gym_bag_test.fbx");
		if (asset is null)
		{
			Logger.Error($"hat asset prefab was null.");
		}
		
		// TODO: hide drone when enabled.
		// After looking into this, we could just modify the drone and propeller mesh when SwarmDrone is selected. (maybe in constructor?) 
		
		// This is for testing without added mesh. ResourceManager.GetAllHats().ToArray()[0].hatMesh
		SwarmDroneMesh = asset?.GetComponent<MeshFilter>().mesh;
		if (SwarmDroneMesh is null)
		{
			Logger.Error($"error getting hat mesh.");
			return;
		}
		
		// We don't modify mesh data as we either do that in Unity import or Blender by changing the origin of the objects and combining them.
		HatSo?.hatMesh = SwarmDroneMesh;
		HatSo?.sound1 = ResourceManager.GetAllHats().ToArray()[0].sound1;
		HatSo?.sound2 = ResourceManager.GetAllHats().ToArray()[0].sound2;
		
		ResourceManager.hats.TryAdd("swarm_hat", HatSo);
		MainSim.Inst.UnlockHat(HatSo);
		GameObject.Find("Farm").GetComponent<FarmRenderer>().droneMesh = SwarmDroneMesh;
		// GameObject.Find("Farm").GetComponent<FarmRenderer>().propellerMesh
	}
}