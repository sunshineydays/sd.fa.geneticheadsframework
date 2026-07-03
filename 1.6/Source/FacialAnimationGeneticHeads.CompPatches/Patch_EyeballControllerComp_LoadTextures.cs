using System;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads.CompPatches;

[HarmonyPatch(typeof(EyeballControllerComp), nameof(EyeballControllerComp.LoadTextures))]
public static class Patch_EyeballControllerComp_LoadTextures
{
	private static bool Prefix(EyeballControllerComp __instance)
	{
		try
		{
			return !EyeballColorOverrideUtility.TryLoadTextures(__instance);
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to override eyeball colors for {__instance?.parent?.LabelShortCap ?? "unknown pawn"}: {arg}");
			return true;
		}
	}
}
