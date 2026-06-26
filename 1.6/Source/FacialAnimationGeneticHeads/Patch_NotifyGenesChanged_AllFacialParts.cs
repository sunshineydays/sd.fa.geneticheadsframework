using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
public static class Patch_NotifyGenesChanged_AllFacialParts
{
	private static void Postfix(Pawn_GeneTracker __instance)
	{
		Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
		if (value != null && value.Spawned)
		{
			DoPart<FacialAnimation.HeadTypeDef>(value, "FacialAnimation.HeadControllerComp", "Head");
			DoPart<EyeballTypeDef>(value, "FacialAnimation.EyeballControllerComp", "Eye");
			DoPart<BrowTypeDef>(value, "FacialAnimation.BrowControllerComp", "Brow");
			DoPart<LidTypeDef>(value, "FacialAnimation.LidControllerComp", "Lid");
			DoPart<MouthTypeDef>(value, "FacialAnimation.MouthControllerComp", "Mouth");
			DoPart<SkinTypeDef>(value, "FacialAnimation.SkinControllerComp", "Skin");
		}
	}

	private static void DoPart<TDef>(Pawn pawn, string compTypeName, string logLabel) where TDef : FaceTypeDef, new()
	{
		if (pawn == null)
		{
			return;
		}
		try
		{
			ThingComp thingComp = pawn.AllComps?.FirstOrDefault((ThingComp c) => c.GetType().FullName == compTypeName);
			if (thingComp == null)
			{
				return;
			}
			FieldInfo fieldInfo = AccessTools.Field(thingComp.GetType(), "faceType");
			if (fieldInfo == null || fieldInfo.FieldType != typeof(TDef))
			{
				return;
			}
			TDef val = fieldInfo.GetValue(thingComp) as TDef;
			TDef geneMatchedDef = GeneFacePatchHelper.GetGeneMatchedDef<TDef>(pawn, pawn.gender);
			if (geneMatchedDef == null)
			{
				return;
			}
			bool num = IsValidForGenes(pawn, val);
			bool num2 = HasRequiredGenes(geneMatchedDef);
			bool flag = HasRequiredGenes(val);
			int num3 = RequiredCount(geneMatchedDef);
			int num4 = RequiredCount(val);
			bool flag2 = num2 && (!flag || num3 > num4);
			if ((!num || flag2 || val == null) && val != geneMatchedDef)
			{
				fieldInfo.SetValue(thingComp, geneMatchedDef);
				MethodInfo methodInfo = AccessTools.Method(thingComp.GetType(), "SetDirty");
				if (methodInfo != null)
				{
					methodInfo.Invoke(thingComp, null);
				}
				else
				{
					AccessTools.Method(thingComp.GetType(), "InitializeIfNeed")?.Invoke(thingComp, null);
				}
				if (Prefs.DevMode)
				{
					Log.Message("[FA Genetic Heads] " + logLabel + ": genes changed -> " + pawn.LabelShortCap + ": " + (val?.defName ?? "<null>") + " → " + geneMatchedDef.defName);
				}
			}
		}
		catch (Exception arg)
		{
			Log.Warning(string.Format("[FA Genetic Heads] Failed to patch {0} comp on {1}: {2}", logLabel, pawn?.LabelShortCap ?? "null pawn", arg));
		}
	}

	private static bool HasRequiredGenes(FaceTypeDef def)
	{
		if (def?.targetGeneDefs != null)
		{
			return def.targetGeneDefs.Count > 0;
		}
		return false;
	}

	private static int RequiredCount(FaceTypeDef def)
	{
		return (def?.targetGeneDefs?.Count).GetValueOrDefault();
	}

	private static bool IsValidForGenes(Pawn pawn, FaceTypeDef def)
	{
		if (def == null)
		{
			return false;
		}
		if (def.targetGeneDefs == null || def.targetGeneDefs.Count == 0)
		{
			return true;
		}
		if (pawn.genes == null)
		{
			return false;
		}
		HashSet<string> hashSet = pawn.genes.GenesListForReading.Select((Gene g) => g.def.defName).ToHashSet();
		return def.targetGeneDefs.All(hashSet.Contains);
	}
}
