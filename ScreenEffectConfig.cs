using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScreenEffectConfig
{
	[BepInPlugin(modGUID, "ScreenEffectConfig", "1.0.0")]
	public class ScreenEffectConfigBase : BaseUnityPlugin
	{
		private static float flashMultiplier = 0.5f;
		private static float pingScreenMultiplier = 0.5f;
		private static float pingSphereMultiplier = 0.5f;

		private static Transform? scanSphere = null;
		private static Volume? scanVolume = null;

		private readonly Harmony harmony = new(modGUID);
		private static ManualLogSource? log = null;

		private void Awake()
		{
			log = Logger;
			log.LogInfo("Starting ScreenEffectConfig");

			harmony.PatchAll();
			flashMultiplier = Config.Bind("GENERAL", "flashMultiplier", 0.25f, "The multiplier to apply to the brightness of the screen effect for flashbangs. Value can be between 0 - 1").Value;
			pingScreenMultiplier = Config.Bind("GENERAL", "pingScreenMultiplier", 0.25f, "The multiplier to apply to the brightness of the screen effect when pinging. Value can be between 0 - 1").Value;
			pingSphereMultiplier = Config.Bind("GENERAL", "pingSphereMultiplier", 0.5f, "The multiplier to apply to the brightness of the outwards sphere when pinging. Value can be between 0 - 1").Value;

			log.LogInfo("ScreenEffectConfig started successfully!");
		}

		[HarmonyPatch(typeof(HUDManager))]
		class HUDManagerPatch
		{
			[HarmonyPatch("PingScan_performed")]
			[HarmonyPostfix]
			public static void PingScan_performed_Postfix()
			{
				if (scanSphere == HUDManager.Instance.scanEffectAnimator.transform)
				{
					return;
				}

				scanSphere = HUDManager.Instance.scanEffectAnimator.transform;
       	Material mat = scanSphere.GetComponent<Renderer>().material;
				Color color = mat.color;
				color.a *= pingSphereMultiplier;
				mat.color = color;

				if (scanVolume == null)
				{
					scanVolume = scanSphere.GetChild(0).GetComponent<Volume>();
				}
			}
		}

		[HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB))]
		class PlayerControllerBPatch
		{
			[HarmonyPatch("LateUpdate")]
			[HarmonyPostfix]
			public static void LateUpdate_Postfix()
			{
				// The scan volume is modified by an Animation, so LateUpdate is called to modify the value from the last keyframe
				if (scanVolume != null)
				{
					scanVolume.weight *= 0.5f + (0.5f * pingScreenMultiplier);
				}
			}
		}

		[HarmonyPatch(typeof(StunGrenadeItem))]
		class StunGrenadeItemPatch
		{
			[HarmonyPatch("StunExplosion")]
			[HarmonyPostfix]
			public static void StunExplosion_Postfix()
			{
				HUDManager.Instance.flashbangScreenFilter.weight *= flashMultiplier;
			}
		}

		private const string modGUID = "joshcantcode.ScreenEffectConfig";
	}
}