using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch]
public static class Patch_CreepJoinerGenerated_AllFacialParts
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return AccessTools.GetDeclaredMethods(typeof(CreepJoinerUtility))
			.Where((MethodInfo method) => method.Name == "GenerateAndSpawn");
	}

	private static void Postfix(Pawn __result, object[] __args)
	{
		try
		{
			if (__result == null)
			{
				return;
			}
			CreepJoinerFormKindDef generatedForm = __args?.OfType<CreepJoinerFormKindDef>().FirstOrDefault();
			if (__result.creepjoiner != null && __result.creepjoiner.form == null && generatedForm != null)
			{
				__result.creepjoiner.form = generatedForm;
			}
			if (Prefs.DevMode)
			{
				string formName = __result.creepjoiner?.form?.defName ?? generatedForm?.defName ?? "<null>";
				Log.Message("[FA Genetic Heads] Creepjoiner generated -> " + __result.LabelShortCap + ": " + formName);
			}
			FaceSelectionUtility.InvalidateAllCaches(__result);
			Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(__result, "creepjoiner generated");
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to refresh facial parts after creepjoiner generation: {arg}");
		}
	}
}
