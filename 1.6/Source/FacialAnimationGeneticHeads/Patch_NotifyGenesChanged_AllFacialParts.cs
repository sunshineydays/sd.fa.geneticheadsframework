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
			FaceSelectionUtility.InvalidateAllCaches(value);
			RefreshGeneDrivenParts(value, "genes changed");
		}
	}

	public static void RefreshAllParts(Pawn pawn, string reason)
	{
		if (pawn == null)
		{
			return;
		}
		RefreshGeneDrivenParts(pawn, reason);
		FaceSelectionUtility.RefreshPartIfNeeded<EyeballTypeDef>(pawn, "FacialAnimation.EyeballControllerComp", "Eye", FacialAnimationGeneticHeadsMod.Settings.EyeballCompActive, reason);
	}

	private static void RefreshGeneDrivenParts(Pawn pawn, string reason)
	{
		FaceSelectionUtility.RefreshPartIfNeeded<FacialAnimation.HeadTypeDef>(pawn, "FacialAnimation.HeadControllerComp", "Head", FacialAnimationGeneticHeadsMod.Settings.HeadCompActive, reason);
		FaceSelectionUtility.RefreshPartIfNeeded<BrowTypeDef>(pawn, "FacialAnimation.BrowControllerComp", "Brow", FacialAnimationGeneticHeadsMod.Settings.BrowCompActive, reason);
		FaceSelectionUtility.RefreshPartIfNeeded<LidTypeDef>(pawn, "FacialAnimation.LidControllerComp", "Lid", FacialAnimationGeneticHeadsMod.Settings.LidCompActive, reason);
		FaceSelectionUtility.RefreshPartIfNeeded<MouthTypeDef>(pawn, "FacialAnimation.MouthControllerComp", "Mouth", FacialAnimationGeneticHeadsMod.Settings.MouthCompActive, reason);
		FaceSelectionUtility.RefreshPartIfNeeded<SkinTypeDef>(pawn, "FacialAnimation.SkinControllerComp", "Skin", FacialAnimationGeneticHeadsMod.Settings.SkinCompActive, reason);
	}
}
