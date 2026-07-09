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
			bool affectsFaceType = AffectsConditionalFaceType(__args);
			bool affectsEyeColor = AffectsEyeballColor(__args);
			bool affectsSkinShader = AffectsSkinShader(__args);
			if (!affectsFaceType && !affectsEyeColor && !affectsSkinShader)
			{
				return;
			}
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			if (pawn == null)
			{
				return;
			}
			if (affectsFaceType)
			{
				FaceSelectionUtility.InvalidateConditionCaches(pawn);
				Patch_NotifyGenesChanged_AllFacialParts.RefreshAllParts(pawn, "hediffs changed");
			}
			if (affectsEyeColor)
			{
				FaceSelectionUtility.MarkPartDirty(pawn, "FacialAnimation.EyeballControllerComp", "Eye", "hediff eye color changed");
			}
			if (affectsSkinShader)
			{
				FaceSelectionUtility.MarkAllPartsDirty(pawn, "hediff skin shader changed");
			}
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

	private static bool AffectsEyeballColor(object[] args)
	{
		if (args == null)
		{
			return true;
		}
		foreach (object arg in args)
		{
			if (arg is Hediff hediff)
			{
				return EyeballColorOverrideUtility.UsesHediff(hediff.def);
			}
			if (arg is HediffDef hediffDef)
			{
				return EyeballColorOverrideUtility.UsesHediff(hediffDef);
			}
		}
		return false;
	}

	private static bool AffectsSkinShader(object[] args)
	{
		if (args == null)
		{
			return true;
		}
		foreach (object arg in args)
		{
			if (arg is Hediff hediff)
			{
				return hediff.def?.skinShader != null;
			}
			if (arg is HediffDef hediffDef)
			{
				return hediffDef.skinShader != null;
			}
		}
		return false;
	}
}
