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

	private static readonly Dictionary<int, EyeColorPair> LastResolvedColors = new Dictionary<int, EyeColorPair>();

	public static bool HasGeneColorDefs()
	{
		return GetColorDefs().Any((EyeballColorDef def) => def != null && !def.geneDef.NullOrEmpty());
	}

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
			FARequiredHediffs modExtension = colorDef.GetModExtension<FARequiredHediffs>();
			if (modExtension?.requiredHediffs != null && modExtension.requiredHediffs.Contains(defName))
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
			FARequiredTraits modExtension = colorDef?.GetModExtension<FARequiredTraits>();
			if (modExtension?.requiredTraits != null && modExtension.requiredTraits.Contains(defName))
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

	public static bool CheckLoadTextures(EyeballControllerComp comp)
	{
		if (!TryResolveEyeColors(comp, out Pawn pawn, out BodyPartRecord _, out BodyPartRecord _, out Color rightEyeColor, out Color leftEyeColor))
		{
			return false;
		}
		int cacheKey = CacheKey(pawn);
		EyeColorPair current = new EyeColorPair(rightEyeColor, leftEyeColor);
		if (LastResolvedColors.TryGetValue(cacheKey, out EyeColorPair previous) && previous.Equals(current))
		{
			return false;
		}
		LastResolvedColors[cacheKey] = current;
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
		ResolveEyeColors(pawn, rightEyePart, leftEyePart, comp.GetCurrentColor(), comp.FaceSecondColor, out rightEyeColor, out leftEyeColor);
		return true;
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

	private static void ResolveEyeColors(Pawn pawn, BodyPartRecord rightEyePart, BodyPartRecord leftEyePart, Color defaultRightEyeColor, Color defaultLeftEyeColor, out Color rightEyeColor, out Color leftEyeColor)
	{
		rightEyeColor = defaultRightEyeColor;
		leftEyeColor = defaultLeftEyeColor;
		HashSet<string> activeGenes = new HashSet<string>(pawn?.genes?.GenesListForReading?
			.Where((Gene gene) => gene?.Active == true && gene.def != null)
			.Select((Gene gene) => gene.def.defName) ?? Enumerable.Empty<string>());
		HashSet<string> activeHediffs = new HashSet<string>(pawn?.health?.hediffSet?.hediffs?
			.Where((Hediff hediff) => hediff?.def != null)
			.Select((Hediff hediff) => hediff.def.defName) ?? Enumerable.Empty<string>());
		HashSet<string> activeTraits = new HashSet<string>(pawn?.story?.traits?.allTraits?
			.Where((Trait trait) => trait?.def != null)
			.Select((Trait trait) => trait.def.defName) ?? Enumerable.Empty<string>());
		foreach (EyeballColorDef colorDef in GetColorDefs())
		{
			if (colorDef == null || HasConditionalRequirements(colorDef) || !MatchesGeneRequirement(colorDef, activeGenes))
			{
				continue;
			}
			rightEyeColor = colorDef.eyeballColor;
			leftEyeColor = colorDef.eyeballColor;
		}
		foreach (EyeballColorDef colorDef2 in GetColorDefs())
		{
			if (colorDef2 == null || !HasConditionalRequirements(colorDef2) || !MatchesAllRequirements(colorDef2, pawn, activeGenes, activeHediffs, activeTraits))
			{
				continue;
			}
			ApplyConditionalColor(colorDef2, pawn, rightEyePart, leftEyePart, ref rightEyeColor, ref leftEyeColor);
		}
	}

	private static bool HasConditionalRequirements(EyeballColorDef colorDef)
	{
		if (colorDef?.hediffDef != null)
		{
			return true;
		}
		if (colorDef?.GetModExtension<FARequiredHediffs>()?.requiredHediffs?.Count > 0)
		{
			return true;
		}
		return colorDef?.GetModExtension<FARequiredTraits>()?.requiredTraits?.Count > 0;
	}

	private static bool MatchesAllRequirements(EyeballColorDef colorDef, Pawn pawn, HashSet<string> activeGenes, HashSet<string> activeHediffs, HashSet<string> activeTraits)
	{
		if (pawn == null)
		{
			return false;
		}
		if (!MatchesGeneRequirement(colorDef, activeGenes))
		{
			return false;
		}
		if (colorDef?.hediffDef != null && !activeHediffs.Contains(colorDef.hediffDef.defName))
		{
			return false;
		}
		FARequiredHediffs hediffExtension = colorDef?.GetModExtension<FARequiredHediffs>();
		if (hediffExtension?.requiredHediffs != null && hediffExtension.requiredHediffs.Count > 0 && !hediffExtension.requiredHediffs.All(activeHediffs.Contains))
		{
			return false;
		}
		FARequiredTraits traitExtension = colorDef?.GetModExtension<FARequiredTraits>();
		if (traitExtension?.requiredTraits != null && traitExtension.requiredTraits.Count > 0 && !traitExtension.requiredTraits.All(activeTraits.Contains))
		{
			return false;
		}
		return true;
	}

	private static bool MatchesGeneRequirement(EyeballColorDef colorDef, HashSet<string> activeGenes)
	{
		if (colorDef == null || colorDef.geneDef.NullOrEmpty())
		{
			return true;
		}
		return activeGenes.Contains(colorDef.geneDef);
	}

	private static void ApplyConditionalColor(EyeballColorDef colorDef, Pawn pawn, BodyPartRecord rightEyePart, BodyPartRecord leftEyePart, ref Color rightEyeColor, ref Color leftEyeColor)
	{
		if (colorDef == null)
		{
			return;
		}
		if (colorDef.hediffDef == null)
		{
			rightEyeColor = colorDef.eyeballColor;
			leftEyeColor = colorDef.eyeballColor;
			return;
		}
		List<Hediff> matchingHediffs = pawn?.health?.hediffSet?.hediffs?
			.Where((Hediff hediff) => hediff?.def == colorDef.hediffDef)
			.ToList();
		if (matchingHediffs == null || matchingHediffs.Count == 0)
		{
			return;
		}
		bool applyToBothEyes = rightEyePart == null || leftEyePart == null;
		bool applyToRightEye = false;
		bool applyToLeftEye = false;
		foreach (Hediff hediff2 in matchingHediffs)
		{
			if (hediff2?.Part == null)
			{
				applyToBothEyes = true;
				break;
			}
			if (hediff2.Part == rightEyePart)
			{
				applyToRightEye = true;
				continue;
			}
			if (hediff2.Part == leftEyePart)
			{
				applyToLeftEye = true;
				continue;
			}
			applyToBothEyes = true;
			break;
		}
		if (applyToBothEyes)
		{
			rightEyeColor = colorDef.eyeballColor;
			leftEyeColor = colorDef.eyeballColor;
			return;
		}
		if (applyToRightEye)
		{
			rightEyeColor = colorDef.eyeballColor;
		}
		if (applyToLeftEye)
		{
			leftEyeColor = colorDef.eyeballColor;
		}
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

	private static int CacheKey(Pawn pawn)
	{
		return pawn?.thingIDNumber ?? 0;
	}

	private readonly struct EyeColorPair
	{
		private readonly Color right;

		private readonly Color left;

		public EyeColorPair(Color right, Color left)
		{
			this.right = right;
			this.left = left;
		}

		public bool Equals(EyeColorPair other)
		{
			return right == other.right && left == other.left;
		}
	}
}
