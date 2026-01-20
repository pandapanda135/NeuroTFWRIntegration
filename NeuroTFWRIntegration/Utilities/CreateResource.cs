using System.IO;
using System.Linq;
using BepInEx;
using NeuroTFWRIntegration.Unity;
using UnityEngine;

namespace NeuroTFWRIntegration.Utilities;

public static class CreateResource
{
	public static void CreateNewHat()
	{
		var path = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "gym-bag");
		AssetBundleHelper.GetAssetBundle(path);
		var asset = AssetBundleHelper.LoadBundle(path,"Assets/Models/Gym_bag_test.fbx");
		if (asset is null)
		{
			Logger.Error($"hat asset prefab was null.");
		}
		var hatSo = ScriptableObject.CreateInstance<HatSO>();
		hatSo.hatName = "swarm";
		hatSo.className = "Hat";
		hatSo.droneFlyHeight = 2f;
		hatSo.hidden = false;
		hatSo.isGolden = false;
		hatSo.preventWrapping = false;
		hatSo.rotateDroneToMove = false;
		// ResourceManager.GetAllHats().ToArray()[0].hatMesh
		Mesh? original = asset?.GetComponent<MeshFilter>().mesh;
		if (original is null)
		{
			Logger.Error($"error getting hat mesh.");
			return;
		}
		
		// We don't modify mesh data as we either do that in Unity import or Blender by changing the origin of the objects and combining them.
		// New idea is to hide the drone mesh and only show the hat and have it positioned to where it looks like the drone. The main issue here would be animating the blades on the drones.
		hatSo.hatMesh = original;
		hatSo.sound1 = ResourceManager.GetAllHats().ToArray()[0].sound1;
		hatSo.sound2 = ResourceManager.GetAllHats().ToArray()[0].sound2;
		
		ResourceManager.hats.TryAdd("swarm_hat", hatSo);
		MainSim.Inst.UnlockHat(hatSo);
	}
}