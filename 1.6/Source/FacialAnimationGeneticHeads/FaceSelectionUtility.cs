using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads;

public static class FaceSelectionUtility
{
	private static readonly Dictionary<Type, FieldInfo> FaceTypeFields = new Dictionary<Type, FieldInfo>();

	private static readonly Dictionary<Type, MethodInfo> SetDirtyMethods = new Dictionary<Type, MethodInfo>();

	private static readonly Dictionary<Type, MethodInfo> InitializeMethods = new Dictionary<Type, MethodInfo>();

	public static void InvalidateAllCaches(Pawn pawn)
	{
		FaceConditionResolver.InvalidatePawn(pawn);
		GeneFacePatchHelper.InvalidatePawn(pawn);
	}

	public static void InvalidateConditionCaches(Pawn pawn)
	{
		FaceConditionResolver.InvalidatePawn(pawn);
	}

	public static void EnsureAllPartsHaveFaceTypes(Pawn pawn, string reason)
	{
		EnsurePartHasFaceType<FacialAnimation.HeadTypeDef>(pawn, "FacialAnimation.HeadControllerComp", "Head", reason);
		EnsurePartHasFaceType<EyeballTypeDef>(pawn, "FacialAnimation.EyeballControllerComp", "Eye", reason);
		EnsurePartHasFaceType<BrowTypeDef>(pawn, "FacialAnimation.BrowControllerComp", "Brow", reason);
		EnsurePartHasFaceType<LidTypeDef>(pawn, "FacialAnimation.LidControllerComp", "Lid", reason);
		EnsurePartHasFaceType<MouthTypeDef>(pawn, "FacialAnimation.MouthControllerComp", "Mouth", reason);
		EnsurePartHasFaceType<SkinTypeDef>(pawn, "FacialAnimation.SkinControllerComp", "Skin", reason);
	}

	public static void EnsurePartHasFaceType<T>(Pawn pawn, string compTypeName, string logLabel, string reason) where T : FaceTypeDef, new()
	{
		if (pawn == null)
		{
			return;
		}
		ThingComp comp = pawn.AllComps?.FirstOrDefault((ThingComp c) => c.GetType().FullName == compTypeName);
		if (comp == null)
		{
			return;
		}
		EnsureCompHasFaceType<T>(comp, pawn, logLabel, reason);
	}

	public static void EnsureCompHasFaceType<T>(object comp, Pawn pawn, string logLabel, string reason) where T : FaceTypeDef, new()
	{
		if (comp == null || pawn == null)
		{
			return;
		}
		Type compType = comp.GetType();
		FieldInfo fieldInfo = GetFaceTypeField(compType);
		if (fieldInfo == null || fieldInfo.FieldType != typeof(T))
		{
			return;
		}
		T current = fieldInfo.GetValue(comp) as T;
		if (current != null)
		{
			return;
		}
		T matched = GeneFacePatchHelper.GetMatchedDef<T>(pawn, pawn.gender);
		if (matched == null)
		{
			return;
		}
		fieldInfo.SetValue(comp, matched);
		MethodInfo methodInfo = GetSetDirtyMethod(compType);
		if (methodInfo != null)
		{
			methodInfo.Invoke(comp, null);
		}
		else
		{
			GetInitializeMethod(compType)?.Invoke(comp, null);
		}
		if (Prefs.DevMode)
		{
			Log.Message("[FA Genetic Heads] " + logLabel + ": " + reason + " -> " + pawn.LabelShortCap + ": <null> -> " + matched.defName);
		}
		MarkPawnGraphicsDirty(pawn, reason);
	}

	public static void RefreshPartIfNeeded<T>(Pawn pawn, string compTypeName, string logLabel, bool active, string reason) where T : FaceTypeDef, new()
	{
		if (pawn == null || !active)
		{
			return;
		}
		ThingComp comp = pawn.AllComps?.FirstOrDefault((ThingComp c) => c.GetType().FullName == compTypeName);
		if (comp == null)
		{
			return;
		}
		RefreshCompIfNeeded<T>(comp, pawn, logLabel, reason);
	}

	public static void RefreshCompIfNeeded<T>(object comp, Pawn pawn, string logLabel, string reason) where T : FaceTypeDef, new()
	{
		if (comp == null || pawn == null)
		{
			return;
		}
		Type compType = comp.GetType();
		FieldInfo fieldInfo = GetFaceTypeField(compType);
		if (fieldInfo == null || fieldInfo.FieldType != typeof(T))
		{
			return;
		}
		T current = fieldInfo.GetValue(comp) as T;
		T matched = GeneFacePatchHelper.GetMatchedDef<T>(pawn, pawn.gender);
		if (matched == null || current == matched || !ShouldReplaceCurrentPart(pawn, current, matched))
		{
			return;
		}
		fieldInfo.SetValue(comp, matched);
		MethodInfo methodInfo = GetSetDirtyMethod(compType);
		if (methodInfo != null)
		{
			methodInfo.Invoke(comp, null);
		}
		else
		{
			GetInitializeMethod(compType)?.Invoke(comp, null);
		}
		if (Prefs.DevMode)
		{
			Log.Message("[FA Genetic Heads] " + logLabel + ": " + reason + " -> " + pawn.LabelShortCap + ": " + (current?.defName ?? "<null>") + " -> " + matched.defName);
		}
		MarkPawnGraphicsDirty(pawn, reason);
	}

	private static void MarkPawnGraphicsDirty(Pawn pawn, string reason)
	{
		if (pawn == null)
		{
			return;
		}
		try
		{
			if (reason != "initialized")
			{
				pawn.Drawer?.renderer?.renderTree?.SetDirty();
			}
			PortraitsCache.SetDirty(pawn);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to mark graphics dirty for {pawn.LabelShortCap}: {arg}");
		}
	}

	private static bool ShouldReplaceCurrentPart(Pawn pawn, FaceTypeDef current, FaceTypeDef matched)
	{
		if (current == null)
		{
			return true;
		}
		bool matchedRequiresConditions = FaceConditionResolver.HasRequiredConditions(matched);
		bool currentRequiresConditions = FaceConditionResolver.HasRequiredConditions(current);
		if (matchedRequiresConditions)
		{
			return true;
		}
		if (currentRequiresConditions)
		{
			return !FaceConditionResolver.IsValidForPawn(pawn, current);
		}
		bool currentValidForGenes = GeneFacePatchHelper.IsValidForGenes(pawn, current);
		bool matchedRequiresGenes = GeneFacePatchHelper.HasRequiredGenes(matched);
		bool currentRequiresGenes = GeneFacePatchHelper.HasRequiredGenes(current);
		bool matchedIsMoreSpecific = matchedRequiresGenes && (!currentRequiresGenes || GeneFacePatchHelper.RequiredGeneCount(matched) > GeneFacePatchHelper.RequiredGeneCount(current));
		return !currentValidForGenes || matchedIsMoreSpecific;
	}

	private static FieldInfo GetFaceTypeField(Type compType)
	{
		if (!FaceTypeFields.TryGetValue(compType, out FieldInfo fieldInfo))
		{
			fieldInfo = AccessTools.Field(compType, "faceType");
			FaceTypeFields[compType] = fieldInfo;
		}
		return fieldInfo;
	}

	private static MethodInfo GetSetDirtyMethod(Type compType)
	{
		if (!SetDirtyMethods.TryGetValue(compType, out MethodInfo methodInfo))
		{
			methodInfo = AccessTools.Method(compType, "SetDirty");
			SetDirtyMethods[compType] = methodInfo;
		}
		return methodInfo;
	}

	private static MethodInfo GetInitializeMethod(Type compType)
	{
		if (!InitializeMethods.TryGetValue(compType, out MethodInfo methodInfo))
		{
			methodInfo = AccessTools.Method(compType, "InitializeIfNeed");
			InitializeMethods[compType] = methodInfo;
		}
		return methodInfo;
	}
}
