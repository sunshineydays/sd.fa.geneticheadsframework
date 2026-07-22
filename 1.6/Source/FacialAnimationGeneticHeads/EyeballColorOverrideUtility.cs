using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads;

public static class EyeballColorOverrideUtility
{
	private static readonly FieldInfo GraphicListField =
		AccessTools.Field(typeof(EyeballControllerComp), "graphicList") ??
		AccessTools.Field(typeof(ControllerBaseComp<EyeballTypeDef, EyeballShapeDef>), "graphicList");

	public static bool UsesHediff(HediffDef hediffDef)
	{
		string defName = hediffDef?.defName;
		if (defName.NullOrEmpty())
		{
			return false;
		}
		foreach (EyeballColorDef colorDef in GetColorDefs())
		{
			if (colorDef == null)
			{
				continue;
			}
			if (colorDef.hediffDef == hediffDef)
			{
				return true;
			}
			if (CanAffectColor(colorDef) && RequiredHediffs(colorDef).Contains(defName))
			{
				return true;
			}
		}
		return false;
	}

	public static bool UsesTrait(TraitDef traitDef)
	{
		string defName = traitDef?.defName;
		if (defName.NullOrEmpty())
		{
			return false;
		}
		foreach (EyeballColorDef colorDef in GetColorDefs())
		{
			if (colorDef != null && CanAffectColor(colorDef) && RequiredTraits(colorDef).Contains(defName))
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryLoadTextures(EyeballControllerComp comp)
	{
		if (!TryResolveEyeColors(comp, out Pawn pawn, out BodyPartRecord rightEyePart, out BodyPartRecord leftEyePart, out Color rightEyeColor, out Color leftEyeColor) || GraphicListField == null)
		{
			return false;
		}
		if (!(GraphicListField.GetValue(comp) is Dictionary<NLFacialAnimationLayerType, NLGraphic_Collection<EyeballShapeDef>> graphicList))
		{
			return false;
		}
		graphicList.Clear();
		float rightEfficiency = GetPartEfficiency(pawn, rightEyePart);
		float leftEfficiency = GetPartEfficiency(pawn, leftEyePart);
		Color rightMain = CvtMainColor(rightEyeColor, rightEfficiency);
		Color leftMain = CvtMainColor(leftEyeColor, leftEfficiency);
		Color rightHighlight = CvtHighlightColorFromMainColor(rightEyeColor, rightEfficiency);
		Color leftHighlight = CvtHighlightColorFromMainColor(leftEyeColor, leftEfficiency);
		if (FacialAnimationMod.Settings.SwitchLeftAndRightEyes)
		{
			SwapColors(ref rightMain, ref leftMain);
			SwapColors(ref rightHighlight, ref leftHighlight);
		}
		string texPathPrefix = GraphicHelper.GetTexPathPrefix(comp.FaceType, pawn.gender);
		string highlightTexPath = texPathPrefix + "_highlight";
		string shader = GraphicHelper.GetSkinShader(pawn, comp.FaceType);
		if (string.IsNullOrEmpty(comp.FaceType.altMaskPath))
		{
			NLGraphicParam baseParam = new NLGraphicParam(shader, texPathPrefix, null, rightMain, leftMain);
			NLGraphicParam highlightParam = new NLGraphicParam(shader, highlightTexPath, null, rightHighlight, leftHighlight);
			graphicList.Add(NLFacialAnimationLayerType.Basic, (NLGraphic_Collection<EyeballShapeDef>)GraphicDatabase.Get<NLGraphic_Collection<EyeballShapeDef>>(baseParam.ToXML()));
			graphicList.Add(NLFacialAnimationLayerType.Cover, (NLGraphic_Collection<EyeballShapeDef>)GraphicDatabase.Get<NLGraphic_Collection<EyeballShapeDef>>(highlightParam.ToXML()));
			return true;
		}
		string leftMaskPath = GraphicHelper.GetMaskPathPrefix(comp.FaceType, pawn.gender) + "_L";
		string rightMaskPath = GraphicHelper.GetMaskPathPrefix(comp.FaceType, pawn.gender) + "_R";
		NLGraphicParam leftBaseParam = new NLGraphicParam(shader, texPathPrefix, leftMaskPath, leftMain, Color.clear);
		NLGraphicParam rightBaseParam = new NLGraphicParam(shader, texPathPrefix, rightMaskPath, rightMain, Color.clear);
		NLGraphicParam leftHighlightParam = new NLGraphicParam(shader, highlightTexPath, leftMaskPath, leftHighlight, Color.clear);
		NLGraphicParam rightHighlightParam = new NLGraphicParam(shader, highlightTexPath, rightMaskPath, rightHighlight, Color.clear);
		graphicList.Add(NLFacialAnimationLayerType.L_Basic, (NLGraphic_Collection<EyeballShapeDef>)GraphicDatabase.Get<NLGraphic_Collection<EyeballShapeDef>>(leftBaseParam.ToXML()));
		graphicList.Add(NLFacialAnimationLayerType.R_Basic, (NLGraphic_Collection<EyeballShapeDef>)GraphicDatabase.Get<NLGraphic_Collection<EyeballShapeDef>>(rightBaseParam.ToXML()));
		graphicList.Add(NLFacialAnimationLayerType.L_Option, (NLGraphic_Collection<EyeballShapeDef>)GraphicDatabase.Get<NLGraphic_Collection<EyeballShapeDef>>(leftHighlightParam.ToXML()));
		graphicList.Add(NLFacialAnimationLayerType.R_Option, (NLGraphic_Collection<EyeballShapeDef>)GraphicDatabase.Get<NLGraphic_Collection<EyeballShapeDef>>(rightHighlightParam.ToXML()));
		return true;
	}

	private static bool TryResolveEyeColors(EyeballControllerComp comp, out Pawn pawn, out BodyPartRecord rightEyePart, out BodyPartRecord leftEyePart, out Color rightEyeColor, out Color leftEyeColor)
	{
		pawn = comp?.parent as Pawn;
		rightEyePart = null;
		leftEyePart = null;
		rightEyeColor = Color.clear;
		leftEyeColor = Color.clear;
		if (comp?.FaceType == null || pawn == null)
		{
			return false;
		}
		rightEyePart = FindEyePart(pawn, GraphicHelper.GetRightEyeLabel(pawn.def?.defName));
		leftEyePart = FindEyePart(pawn, GraphicHelper.GetLeftEyeLabel(pawn.def?.defName));
		return ResolveEyeColors(pawn, rightEyePart, leftEyePart, comp.GetCurrentColor(), comp.FaceSecondColor, out rightEyeColor, out leftEyeColor);
	}

	private static IEnumerable<EyeballColorDef> GetColorDefs()
	{
		return DefDatabase<EyeballColorDef>.AllDefsListForReading ?? Enumerable.Empty<EyeballColorDef>();
	}

	private static BodyPartRecord FindEyePart(Pawn pawn, string label)
	{
		if (pawn?.def?.race?.body?.AllParts == null || label.NullOrEmpty())
		{
			return null;
		}
		return pawn.def.race.body.AllParts.FirstOrDefault((BodyPartRecord part) => part.untranslatedCustomLabel == label);
	}

	private static float GetPartEfficiency(Pawn pawn, BodyPartRecord part)
	{
		if (pawn?.health?.hediffSet == null || part == null)
		{
			return 1f;
		}
		return Math.Min(1f, PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, part));
	}

	private static bool ResolveEyeColors(Pawn pawn, BodyPartRecord rightEyePart, BodyPartRecord leftEyePart, Color defaultRightEyeColor, Color defaultLeftEyeColor, out Color rightEyeColor, out Color leftEyeColor)
	{
		rightEyeColor = defaultRightEyeColor;
		leftEyeColor = defaultLeftEyeColor;

		bool usedFrameworkRequirements = false;

		foreach (EyeballColorDef colorDef in GetColorDefs()
			.Where((EyeballColorDef def) => def?.hediffDef != null))
		{
			List<Hediff> matchingHediffs = pawn?.health?.hediffSet?.hediffs?
				.Where((Hediff hediff) => hediff?.def == colorDef.hediffDef)
				.ToList();
			if (matchingHediffs == null || matchingHediffs.Count == 0)
			{
				continue;
			}
			if (!FrameworkRequirementsAllow(colorDef, pawn, ref usedFrameworkRequirements))
			{
				continue;
			}
			foreach (Hediff hediff in matchingHediffs)
			{
				if (hediff.Part == null)
				{
					rightEyeColor = colorDef.eyeballColor;
					leftEyeColor = colorDef.eyeballColor;
					usedFrameworkRequirements = true;
					continue;
				}
				if (rightEyePart != null && hediff.Part == rightEyePart)
				{
					rightEyeColor = colorDef.eyeballColor;
					continue;
				}
				if (leftEyePart != null && hediff.Part == leftEyePart)
				{
					leftEyeColor = colorDef.eyeballColor;
					continue;
				}
				rightEyeColor = colorDef.eyeballColor;
				leftEyeColor = colorDef.eyeballColor;
				usedFrameworkRequirements = true;
			}
		}

		foreach (EyeballColorDef colorDef in GetColorDefs()
			.Where((EyeballColorDef def) => def != null && def.geneDef.NullOrEmpty() && def.hediffDef == null && HasFrameworkRequirements(def)))
		{
			if (!FrameworkRequirementsAllow(colorDef, pawn, ref usedFrameworkRequirements))
			{
				continue;
			}
			rightEyeColor = colorDef.eyeballColor;
			leftEyeColor = colorDef.eyeballColor;
		}
		return usedFrameworkRequirements;
	}

	private static bool CanAffectColor(EyeballColorDef colorDef)
	{
		return colorDef != null && (colorDef.hediffDef != null || HasFrameworkRequirements(colorDef));
	}

	private static bool FrameworkRequirementsAllow(EyeballColorDef colorDef, Pawn pawn, ref bool usedFrameworkRequirements)
	{
		if (!HasFrameworkRequirements(colorDef))
		{
			return true;
		}
		usedFrameworkRequirements = true;
		return PawnHasRequiredHediffs(pawn, colorDef) && PawnHasRequiredTraits(pawn, colorDef);
	}

	private static bool HasFrameworkRequirements(EyeballColorDef colorDef)
	{
		return RequiredHediffs(colorDef).Any() || RequiredTraits(colorDef).Any();
	}

	private static bool PawnHasRequiredHediffs(Pawn pawn, EyeballColorDef colorDef)
	{
		List<string> requiredHediffs = RequiredHediffs(colorDef);
		if (requiredHediffs.Count == 0)
		{
			return true;
		}
		HashSet<string> pawnHediffs = new HashSet<string>(pawn?.health?.hediffSet?.hediffs?
			.Where((Hediff hediff) => hediff?.def != null)
			.Select((Hediff hediff) => hediff.def.defName) ?? Enumerable.Empty<string>());
		return requiredHediffs.All(pawnHediffs.Contains);
	}

	private static bool PawnHasRequiredTraits(Pawn pawn, EyeballColorDef colorDef)
	{
		List<string> requiredTraits = RequiredTraits(colorDef);
		if (requiredTraits.Count == 0)
		{
			return true;
		}
		HashSet<string> pawnTraits = new HashSet<string>(pawn?.story?.traits?.allTraits?
			.Where((Trait trait) => trait?.def != null)
			.Select((Trait trait) => trait.def.defName) ?? Enumerable.Empty<string>());
		return requiredTraits.All(pawnTraits.Contains);
	}

	private static List<string> RequiredHediffs(EyeballColorDef colorDef)
	{
		return colorDef?.GetModExtension<FARequiredHediffs>()?.requiredHediffs ?? new List<string>();
	}

	private static List<string> RequiredTraits(EyeballColorDef colorDef)
	{
		return colorDef?.GetModExtension<FARequiredTraits>()?.requiredTraits ?? new List<string>();
	}

	private static Color CvtMainColor(Color baseColor, float efficiency)
	{
		return Color.Lerp(Color.white, baseColor, efficiency);
	}

	private static void SwapColors(ref Color first, ref Color second)
	{
		Color color = first;
		first = second;
		second = color;
	}

	private static Color CvtHighlightColorFromMainColor(Color baseColor, float efficiency)
	{
		efficiency = Mathf.Round(efficiency * 10f) / 10f;
		efficiency = Mathf.Clamp(efficiency, 0f, 1f);
		return Color.Lerp(Color.clear, Color.white, efficiency);
	}

}
