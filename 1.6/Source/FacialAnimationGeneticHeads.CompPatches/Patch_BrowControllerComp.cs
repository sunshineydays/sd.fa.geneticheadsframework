using System;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads.CompPatches;

[HarmonyPatch]
public static class Patch_BrowControllerComp
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("FacialAnimation.BrowControllerComp"), "InitializeIfNeed");
	}

	private static bool Prefix(object __instance)
	{
		try
		{
			if (!((__instance as ThingComp)?.parent is Pawn pawn))
			{
				return true;
			}
			if (!FacialAnimationGeneticHeadsMod.Settings.BrowCompActive)
			{
				return true;
			}
			FieldInfo fieldInfo = AccessTools.Field(__instance.GetType(), "faceType");
			if (fieldInfo == null)
			{
				Log.Warning("[FA Genetic Heads] Missing faceType field on " + __instance.GetType().FullName);
				return true;
			}
			if (fieldInfo.FieldType != typeof(BrowTypeDef))
			{
				return true;
			}
			if (fieldInfo.GetValue(__instance) is BrowTypeDef)
			{
				return true;
			}
			BrowTypeDef geneMatchedDef = GeneFacePatchHelper.GetGeneMatchedDef<BrowTypeDef>(pawn, pawn.gender);
			if (geneMatchedDef != null)
			{
				fieldInfo.SetValue(__instance, geneMatchedDef);
				if (Prefs.DevMode)
				{
					Log.Message("[FA Genetic Heads] " + typeof(BrowTypeDef).Name + ": assigned " + geneMatchedDef.defName + " to " + pawn.LabelShortCap);
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
