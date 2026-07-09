using System;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch(typeof(GraphicHelper), nameof(GraphicHelper.GetSkinShader))]
public static class Patch_GraphicHelper_GetSkinShader
{
	private const string CutoutSkinShader = "Map/CutoutSkin";
	private const string CutoutSkinOverrideShader = "Map/CutoutSkinOverride";

	private static void Postfix(Pawn pawn, FaceTypeDef def, ref string __result)
	{
		try
		{
			if (pawn == null || def == null || pawn.Drawer?.renderer?.StatueColor.HasValue == true || !UsesStandardSkinShader(def))
			{
				return;
			}
			if (TryGetHediffSkinShaderPath(pawn, out string shaderPath))
			{
				__result = shaderPath;
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to apply hediff skin shader compatibility: {arg}");
		}
	}

	private static bool UsesStandardSkinShader(FaceTypeDef def)
	{
		return def.shader == CutoutSkinShader ||
			def.shader == CutoutSkinOverrideShader ||
			def.shaderColorOverride == CutoutSkinOverrideShader;
	}

	private static bool TryGetHediffSkinShaderPath(Pawn pawn, out string shaderPath)
	{
		if (pawn.health?.hediffSet?.hediffs == null)
		{
			shaderPath = null;
			return false;
		}
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			shaderPath = hediff?.def?.skinShader?.shaderPath;
			if (!string.IsNullOrEmpty(shaderPath))
			{
				return true;
			}
		}
		shaderPath = null;
		return false;
	}
}
