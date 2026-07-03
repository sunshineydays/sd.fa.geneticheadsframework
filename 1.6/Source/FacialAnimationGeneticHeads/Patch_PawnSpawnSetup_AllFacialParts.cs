using System;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
public static class Patch_PawnSpawnSetup_AllFacialParts
{
	private static void Postfix(Pawn __instance, object[] __args)
	{
		try
		{
			if (__instance == null || __instance.Destroyed)
			{
				return;
			}
			bool respawningAfterLoad = __args != null && __args.Length > 1 && __args[1] is bool flag && flag;
			string reason = respawningAfterLoad ? "spawned after load" : "spawned";
			FaceSelectionUtility.InvalidateAllCaches(__instance);
			Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(__instance, reason);
			FaceSelectionUtility.EnsureAllPartsHaveFaceTypes(__instance, reason + " fallback");
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to refresh facial parts after pawn spawn: {arg}");
		}
	}
}
