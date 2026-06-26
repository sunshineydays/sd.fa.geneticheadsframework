using System;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads.CompPatches;

[HarmonyPatch]
public static class Patch_LidControllerComp
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("FacialAnimation.LidControllerComp"), "InitializeIfNeed");
	}

	private static bool Prefix(object __instance)
	{
		try
		{
			if (!((__instance as ThingComp)?.parent is Pawn pawn))
			{
				return true;
			}
			if (!FacialAnimationGeneticHeadsMod.Settings.LidCompActive)
			{
				return true;
			}
			FieldInfo fieldInfo = AccessTools.Field(__instance.GetType(), "faceType");
			if (fieldInfo == null)
			{
				Log.Warning("[FA Genetic Heads] Missing faceType field on " + __instance.GetType().FullName);
				return true;
			}
			if (fieldInfo.FieldType != typeof(LidTypeDef))
			{
				return true;
			}
			if (fieldInfo.GetValue(__instance) is LidTypeDef)
			{
				return true;
			}
			LidTypeDef geneMatchedDef = GeneFacePatchHelper.GetGeneMatchedDef<LidTypeDef>(pawn, pawn.gender);
			if (geneMatchedDef != null)
			{
				fieldInfo.SetValue(__instance, geneMatchedDef);
				if (Prefs.DevMode)
				{
					Log.Message("[FA Genetic Heads] " + typeof(LidTypeDef).Name + ": assigned " + geneMatchedDef.defName + " to " + pawn.LabelShortCap);
				}
			}
			return true;
		}
		catch (Exception arg)
		{
			Log.Error($"[FA Genetic Heads] Exception in patch for {__instance.GetType().FullName}: {arg}");
			return true;
		}
	}
}
