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
			bool affectsFaceType = FaceConditionResolver.UsesHediff(hediff?.def);
			bool affectsSkinShader = hediff?.def?.skinShader != null;
			if (!affectsFaceType && !affectsSkinShader)
			{
				return;
			}
			Pawn pawn = __instance?.pawn ?? hediff?.pawn;
			if (pawn == null)
			{
				return;
			}
			if (affectsFaceType)
			{
				FaceSelectionUtility.InvalidateConditionCaches(pawn);
				Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(pawn, "hediff added");
			}
			if (affectsSkinShader)
			{
				FaceSelectionUtility.MarkAllPartsDirty(pawn, "hediff skin shader changed");
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to refresh facial parts after hediff add: {arg}");
		}
	}
}
