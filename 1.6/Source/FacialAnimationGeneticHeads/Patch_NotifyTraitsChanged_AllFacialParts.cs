using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch]
public static class Patch_NotifyTraitsChanged_AllFacialParts
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return AccessTools.GetDeclaredMethods(typeof(TraitSet))
			.Where((MethodInfo method) => method.Name == "GainTrait" || method.Name == "RemoveTrait");
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
			if (pawn == null)
			{
				return;
			}
			FaceSelectionUtility.InvalidateConditionCaches(pawn);
			Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(pawn, "traits changed");
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to refresh facial parts after trait change: {arg}");
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
			if (arg is Trait trait)
			{
				return FaceConditionResolver.UsesTrait(trait.def);
			}
			if (arg is TraitDef traitDef)
			{
				return FaceConditionResolver.UsesTrait(traitDef);
			}
		}
		return false;
	}

}
