using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch]
public static class Patch_SelectPartWindow_EnsureFacialParts
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("FacialAnimation.NL_SelectPartWindow"), "DrawAnimationPawnParamFacial");
	}

	private static bool Prefix(Pawn pawn)
	{
		try
		{
			FaceSelectionUtility.EnsureAllPartsHaveFaceTypes(pawn, "facial animation gui");
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to ensure facial parts before opening the Facial Animation GUI: {arg}");
		}
		return true;
	}
}
