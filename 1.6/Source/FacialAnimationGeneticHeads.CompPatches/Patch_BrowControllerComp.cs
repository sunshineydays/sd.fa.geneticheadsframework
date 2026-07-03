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
			FaceSelectionUtility.RefreshCompIfNeeded<BrowTypeDef>(__instance, pawn, "Brow", "initialized");
			return true;
		}
		catch (Exception arg)
		{
			Log.Error($"[FA Genetic Heads] Exception in patch for {__instance.GetType().FullName}: {arg}");
			return true;
		}
	}
}
