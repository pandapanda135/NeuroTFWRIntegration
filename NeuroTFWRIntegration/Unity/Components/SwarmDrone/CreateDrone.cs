using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using NeuroTFWRIntegration.Utilities;
using TMPro;
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
	private static Material? _defaultDroneMaterial;
	public static Material? SwarmDroneMaterial;

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
	private static readonly string MaterialPath = Path.Combine(Paths.PluginPath, "NeuroTFWRIntegration", "AssetBundles", "drone-material");
	private static void SetHatInformation()
	{
		if (_hatSo is null)
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
		}
		_swarmDroneMesh = asset?.GetComponent<MeshFilter>().mesh;
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

		SwarmDroneMaterial = mat;
		
		// Utilities.Logger.Info($"mesh rotation: {mat?.transform.rotation}");
		
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
			// WorkspaceState.FarmRenderer.material = _defaultDroneMaterial;
			ModifyPropellers(true);
			return;
		}
		
		if (_swarmDroneMesh is null || SwarmDroneMaterial is null)
		{
			Utilities.Logger.Error($"hat asset prefab was null.");
			return;
		}

		WorkspaceState.FarmRenderer.droneMesh = _swarmDroneMesh;
		// WorkspaceState.FarmRenderer.material = SwarmDroneMaterial;
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
	
	[HarmonyPatch(typeof(FarmRenderer), nameof(FarmRenderer.Update))]
	public static class FarmRendererChangeDroneMaterialPatch
	{
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Utilities.Logger.Info($"running update transpiler");
			var codeInstructions = instructions.ToArray();
			foreach (var instruction in codeInstructions)
			{
				// set to swarm material if in loop and before being instanced
				if (instruction.opcode == OpCodes.Ldfld)
				{
					Utilities.Logger.Info($"opcode was valid");
					yield return Transpilers.EmitDelegate<Func<Material>>(GetDroneMaterial);
				}
				
				// after drone loop stop the 
				yield return instruction;
			}
			// 		new CodeMatch(OpCodes.Ldc_I4_0),
			// var codeMatcher = new CodeMatcher(instructions)
			// 	.MatchForward(false, // false = move at the start of the match, true = move at the end of the match
			// 		// get this
			// 		new CodeMatch(OpCodes.Ldarg_0),
			// 		// get the material from farm renderer
			// 		new CodeMatch(i => i.opcode == OpCodes.Ldfld))
			// 	.Advance(1)
			// 	
			// 	.InsertAndAdvance(
			// 		new CodeInstruction(OpCodes.Dup),
			// 		new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Foo), "Foo"))
			// 	);
		}

		[HarmonyPostfix]
		public static void PostFix()
		{
			if (WorkspaceState.FarmRenderer.material != SwarmDroneMaterial)
				return;
			
			Utilities.Logger.Info($"equals swarm drone: {WorkspaceState.FarmRenderer.material == SwarmDroneMaterial}");
		}

		private static Material GetDroneMaterial()
		{
			Utilities.Logger.Info($"drone material start");
			// if (WorkspaceState.FarmRenderer.material is null) return () => new();
			try
			{
				if (WorkspaceState.Farm.drones.Any(drone => drone.hat.hatSO.hatName == "swarm") &&
				    SwarmDroneMaterial is not null)
				{
					Utilities.Logger.Info($"set material");
					return SwarmDroneMaterial;
					WorkspaceState.FarmRenderer.material = SwarmDroneMaterial;
				}
				else
				{
					return _defaultDroneMaterial;
					WorkspaceState.FarmRenderer.material = _defaultDroneMaterial;
				}
			}
			catch (Exception e)
			{
				Utilities.Logger.Error($"{e}");
				return _defaultDroneMaterial;
			}
			
			return _defaultDroneMaterial;
		} 
	}
}