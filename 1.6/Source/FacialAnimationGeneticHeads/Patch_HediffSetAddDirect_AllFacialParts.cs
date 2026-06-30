using System;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch(typeof(HediffSet), "AddDirect")]
public static class Patch_HediffSetAddDirect_AllFacialParts
{
	private static void Postfix(HediffSet __instance, Hediff hediff)
	{
		try
		{
			if (!FaceConditionResolver.UsesHediff(hediff?.def))
			{
				return;
			}
			Pawn pawn = __instance?.pawn ?? hediff?.pawn;
			if (pawn == null)
			{
				return;
			}
			FaceSelectionUtility.InvalidateConditionCaches(pawn);
			Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(pawn, "hediff added");
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to refresh facial parts after hediff add: {arg}");
		}
	}
}
