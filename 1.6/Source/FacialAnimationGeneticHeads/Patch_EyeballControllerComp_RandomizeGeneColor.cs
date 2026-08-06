using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch(typeof(EyeballControllerComp), nameof(EyeballControllerComp.LoadTextures))]
public static class Patch_EyeballControllerComp_RandomizeGeneColor
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> codes = instructions.ToList();
		MethodInfo allDefsGetter = AccessTools.PropertyGetter(
			typeof(DefDatabase<EyeballColorDef>),
			nameof(DefDatabase<EyeballColorDef>.AllDefs));
		MethodInfo applyRandomGeneColor = AccessTools.Method(
			typeof(Patch_EyeballControllerComp_RandomizeGeneColor),
			nameof(ApplyRandomGeneColor));

		// LoadTextures checks gene colors first and hediff colors last. Inject
		// immediately before that final EyeballColorDef pass so a hediff can
		// still deliberately override the randomized gene color.
		int insertionIndex = -1;
		for (int index = 0; index < codes.Count; index++)
		{
			if (allDefsGetter != null && codes[index].Calls(allDefsGetter))
			{
				insertionIndex = index;
			}
		}

		if (insertionIndex < 0 || applyRandomGeneColor == null)
		{
			Log.Warning("[FA Genetic Heads] Could not patch Facial Animation's eye-color selection; matching eye colors will retain FA's normal priority.");
			return codes;
		}

		List<CodeInstruction> injected = new List<CodeInstruction>
		{
			new CodeInstruction(OpCodes.Ldarg_0),
			new CodeInstruction(OpCodes.Ldloca_S, 3),
			new CodeInstruction(OpCodes.Ldloca_S, 4),
			new CodeInstruction(OpCodes.Call, applyRandomGeneColor)
		};
		injected[0].labels.AddRange(codes[insertionIndex].labels);
		injected[0].blocks.AddRange(codes[insertionIndex].blocks);
		codes[insertionIndex].labels.Clear();
		codes[insertionIndex].blocks.Clear();
		codes.InsertRange(insertionIndex, injected);
		return codes;
	}

	private static void ApplyRandomGeneColor(
		EyeballControllerComp controller,
		ref Color leftColor,
		ref Color rightColor)
	{
		Pawn pawn = controller?.parent as Pawn;
		if (pawn?.genes?.GenesListForReading == null
			|| controller.FaceType?.GetModExtension<FAEyeRenderNodeExtension>()?.randomizeMatchingEyeColors != true)
		{
			return;
		}

		HashSet<string> pawnGeneNames = new HashSet<string>(
			pawn.genes.GenesListForReading
				.Where((Gene gene) => gene?.def != null)
				.Select((Gene gene) => gene.def.defName));

		List<EyeballColorDef> matchingColors = DefDatabase<EyeballColorDef>.AllDefsListForReading
			.Where((EyeballColorDef colorDef) =>
				colorDef != null
				&& !colorDef.geneDef.NullOrEmpty()
				&& pawnGeneNames.Contains(colorDef.geneDef))
			.OrderBy((EyeballColorDef colorDef) => colorDef.defName, StringComparer.Ordinal)
			.GroupBy((EyeballColorDef colorDef) => colorDef.geneDef)
			.Select((IGrouping<string, EyeballColorDef> group) => group.First())
			.ToList();

		if (matchingColors.Count <= 1)
		{
			return;
		}

		EyeballColorDef selected = matchingColors[StableIndex(pawn.thingIDNumber, matchingColors.Count)];
		leftColor = selected.eyeballColor;
		rightColor = selected.eyeballColor;
	}

	private static int StableIndex(int pawnId, int count)
	{
		uint hash = unchecked((uint)pawnId + 0x9E3779B9u);
		hash ^= hash >> 16;
		hash *= 0x7FEB352Du;
		hash ^= hash >> 15;
		hash *= 0x846CA68Bu;
		hash ^= hash >> 16;
		return (int)(hash % (uint)count);
	}
}
