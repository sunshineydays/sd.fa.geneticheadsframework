using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch]
public static class Patch_NotifyHediffsChanged_AllFacialParts
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return AccessTools.GetDeclaredMethods(typeof(Pawn_HealthTracker))
			.Where((MethodInfo method) => method.Name == "AddHediff" || method.Name == "RemoveHediff");
	}

	private static void Postfix(object __instance, object[] __args)
	{
		try
		{
			if (!AffectsConditionalFaceType(__args))
			{
				return;
			}
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			FaceSelectionUtility.InvalidateConditionCaches(pawn);
			Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(pawn, "hediffs changed");
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to refresh facial parts after hediff change: {arg}");
		}
	}

	private static bool AffectsConditionalFaceType(object[] args)
	{
		if (args == null)
		{
			return true;
		}
		foreach (object arg in args)
		{
			if (arg is Hediff hediff)
			{
				return FaceConditionResolver.UsesHediff(hediff.def);
			}
			if (arg is HediffDef hediffDef)
			{
				return FaceConditionResolver.UsesHediff(hediffDef);
			}
		}
		return false;
	}
}
