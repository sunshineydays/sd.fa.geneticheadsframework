using System;
using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads;

public static class FaceConditionResolver
{
	private static readonly HashSet<string> RequiredHediffNames = new HashSet<string>(
		GetRequiredHediffNames<FacialAnimation.HeadTypeDef>()
			.Concat(GetRequiredHediffNames<EyeballTypeDef>())
			.Concat(GetRequiredHediffNames<BrowTypeDef>())
			.Concat(GetRequiredHediffNames<LidTypeDef>())
			.Concat(GetRequiredHediffNames<MouthTypeDef>())
			.Concat(GetRequiredHediffNames<SkinTypeDef>()));

	private static readonly HashSet<string> RequiredTraitNames = new HashSet<string>(
		GetRequiredTraitNames<FacialAnimation.HeadTypeDef>()
			.Concat(GetRequiredTraitNames<EyeballTypeDef>())
			.Concat(GetRequiredTraitNames<BrowTypeDef>())
			.Concat(GetRequiredTraitNames<LidTypeDef>())
			.Concat(GetRequiredTraitNames<MouthTypeDef>())
			.Concat(GetRequiredTraitNames<SkinTypeDef>()));

	public static T Match<T>(Pawn pawn, Gender gender) where T : FaceTypeDef, new()
	{
		if (pawn == null)
		{
			return null;
		}
		try
		{
			int cacheKey = CacheKey(pawn, gender);
			if (CachedMatches<T>.Matches.TryGetValue(cacheKey, out T cachedMatch))
			{
				return cachedMatch;
			}
			if (CachedMatches<T>.Misses.Contains(cacheKey) || CachedMatches<T>.ConditionalDefs.Count == 0)
			{
				return null;
			}
			HashSet<string> pawnHediffs = GetPawnHediffNames(pawn);
			string creepjoinerForm = GetPawnCreepjoinerFormName(pawn);
			HashSet<string> pawnTraits = GetPawnTraitNames(pawn);
			if (pawnHediffs.Count == 0 && string.IsNullOrEmpty(creepjoinerForm) && pawnTraits.Count == 0)
			{
				CachedMatches<T>.Misses.Add(cacheKey);
				return null;
			}
			HashSet<string> pawnGenes = GetPawnGeneNames(pawn);
			List<T> candidates = CachedMatches<T>.ConditionalDefs
				.Where((T def) => IsValidForPawn(pawn, gender, def, pawnHediffs, creepjoinerForm, pawnTraits, pawnGenes))
				.ToList();
			if (candidates.Count == 0)
			{
				CachedMatches<T>.Misses.Add(cacheKey);
				return null;
			}
			int highestConditionCount = candidates.Max(RequiredConditionCount);
			candidates = candidates.Where((T def) => RequiredConditionCount(def) == highestConditionCount).ToList();
			int highestGeneCount = candidates.Max(GeneFacePatchHelper.RequiredGeneCount);
			candidates = candidates.Where((T def) => GeneFacePatchHelper.RequiredGeneCount(def) == highestGeneCount).ToList();
			T match = PickStable(pawn, candidates);
			CachedMatches<T>.Matches[cacheKey] = match;
			return match;
		}
		catch (Exception arg)
		{
			Log.Error(string.Format("[FA Genetic Heads] Exception while matching conditional {0} for {1}: {2}", typeof(T).Name, pawn?.LabelShortCap ?? "null pawn", arg));
			return null;
		}
	}

	public static void InvalidatePawn(Pawn pawn)
	{
		CachedMatches<FacialAnimation.HeadTypeDef>.Invalidate(pawn);
		CachedMatches<EyeballTypeDef>.Invalidate(pawn);
		CachedMatches<BrowTypeDef>.Invalidate(pawn);
		CachedMatches<LidTypeDef>.Invalidate(pawn);
		CachedMatches<MouthTypeDef>.Invalidate(pawn);
		CachedMatches<SkinTypeDef>.Invalidate(pawn);
	}

	public static bool UsesHediff(HediffDef hediffDef)
	{
		return hediffDef != null && RequiredHediffNames.Contains(hediffDef.defName);
	}

	public static bool UsesTrait(TraitDef traitDef)
	{
		return traitDef != null && RequiredTraitNames.Contains(traitDef.defName);
	}

	public static bool HasRequiredConditions(FaceTypeDef def)
	{
		return RequiredConditionCount(def) > 0;
	}

	public static int RequiredConditionCount(FaceTypeDef def)
	{
		return RequiredHediffCount(def) + RequiredCreepjoinerFormCount(def) + RequiredTraitCount(def);
	}

	public static int RequiredHediffCount(FaceTypeDef def)
	{
		return def?.GetModExtension<FARequiredHediffs>()?.requiredHediffs?.Count ?? 0;
	}

	public static int RequiredTraitCount(FaceTypeDef def)
	{
		return def?.GetModExtension<FARequiredTraits>()?.requiredTraits?.Count ?? 0;
	}

	public static bool IsValidForPawn(Pawn pawn, FaceTypeDef def)
	{
		if (pawn == null || def == null)
		{
			return false;
		}
		return IsValidForPawn(pawn, pawn.gender, def, GetPawnHediffNames(pawn), GetPawnCreepjoinerFormName(pawn), GetPawnTraitNames(pawn), GetPawnGeneNames(pawn));
	}

	private static bool IsValidForPawn(Pawn pawn, Gender gender, FaceTypeDef def, HashSet<string> pawnHediffs, string creepjoinerForm, HashSet<string> pawnTraits, HashSet<string> pawnGenes)
	{
		if (pawn == null || def == null || !HasRequiredConditions(def))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(def.raceName) && def.raceName != pawn.def.defName)
		{
			return false;
		}
		if (def.gender != gender && def.gender != Gender.None)
		{
			return false;
		}
		FARequiredHediffs hediffExtension = def.GetModExtension<FARequiredHediffs>();
		if (hediffExtension?.requiredHediffs != null && hediffExtension.requiredHediffs.Count > 0 && !hediffExtension.requiredHediffs.All(pawnHediffs.Contains))
		{
			return false;
		}
		FARequiredCreepjoinerForms creepjoinerExtension = def.GetModExtension<FARequiredCreepjoinerForms>();
		if (creepjoinerExtension?.requiredCreepjoinerForms != null && creepjoinerExtension.requiredCreepjoinerForms.Count > 0)
		{
			if (string.IsNullOrEmpty(creepjoinerForm) || !creepjoinerExtension.requiredCreepjoinerForms.Contains(creepjoinerForm))
			{
				return false;
			}
		}
		FARequiredTraits traitExtension = def.GetModExtension<FARequiredTraits>();
		if (traitExtension?.requiredTraits != null && traitExtension.requiredTraits.Count > 0 && !traitExtension.requiredTraits.All(pawnTraits.Contains))
		{
			return false;
		}
		if (def.targetGeneDefs != null && def.targetGeneDefs.Count > 0)
		{
			return def.targetGeneDefs.All(pawnGenes.Contains);
		}
		return true;
	}

	private static int RequiredCreepjoinerFormCount(FaceTypeDef def)
	{
		FARequiredCreepjoinerForms modExtension = def?.GetModExtension<FARequiredCreepjoinerForms>();
		if (modExtension?.requiredCreepjoinerForms != null && modExtension.requiredCreepjoinerForms.Count > 0)
		{
			return 1;
		}
		return 0;
	}

	private static string GetPawnCreepjoinerFormName(Pawn pawn)
	{
		if (pawn == null)
		{
			return null;
		}
		string trackerForm = pawn.creepjoiner?.form?.defName;
		if (!string.IsNullOrEmpty(trackerForm))
		{
			return trackerForm;
		}
		if (pawn.kindDef is CreepJoinerFormKindDef formKind)
		{
			return formKind.defName;
		}
		return null;
	}

	private static HashSet<string> GetPawnHediffNames(Pawn pawn)
	{
		if (pawn?.health?.hediffSet?.hediffs == null)
		{
			return new HashSet<string>();
		}
		return new HashSet<string>(pawn.health.hediffSet.hediffs
			.Where((Hediff hediff) => hediff?.def != null)
			.Select((Hediff hediff) => hediff.def.defName));
	}

	private static HashSet<string> GetPawnTraitNames(Pawn pawn)
	{
		return new HashSet<string>(pawn?.story?.traits?.allTraits?
			.Where((Trait trait) => trait?.def != null)
			.Select((Trait trait) => trait.def.defName) ?? Enumerable.Empty<string>());
	}

	private static HashSet<string> GetPawnGeneNames(Pawn pawn)
	{
		return new HashSet<string>(pawn?.genes?.GenesListForReading?
			.Where((Gene gene) => gene?.def != null)
			.Select((Gene gene) => gene.def.defName) ?? Enumerable.Empty<string>());
	}

	private static T PickStable<T>(Pawn pawn, List<T> candidates) where T : FaceTypeDef
	{
		Rand.PushState(pawn.thingIDNumber);
		try
		{
			if (candidates.TryRandomElementByWeight((T def) => def.probability > 0f ? def.probability : 0f, out T result))
			{
				return result;
			}
			return candidates.RandomElement();
		}
		catch (Exception arg)
		{
			Log.Warning($"[FA Genetic Heads] Failed to pick random conditional FaceTypeDef: {arg}");
			return candidates.First();
		}
		finally
		{
			Rand.PopState();
		}
	}

	private static int CacheKey(Pawn pawn, Gender gender)
	{
		return pawn.thingIDNumber * 10 + (int)gender;
	}

	private static IEnumerable<string> GetRequiredHediffNames<T>() where T : FaceTypeDef, new()
	{
		return CachedMatches<T>.ConditionalDefs
			.SelectMany((T def) => def.GetModExtension<FARequiredHediffs>()?.requiredHediffs ?? Enumerable.Empty<string>());
	}

	private static IEnumerable<string> GetRequiredTraitNames<T>() where T : FaceTypeDef, new()
	{
		return CachedMatches<T>.ConditionalDefs
			.SelectMany((T def) => def.GetModExtension<FARequiredTraits>()?.requiredTraits ?? Enumerable.Empty<string>());
	}

	private static class CachedMatches<T> where T : FaceTypeDef, new()
	{
		public static readonly List<T> ConditionalDefs = DefDatabase<T>.AllDefs
			.Where(HasRequiredConditions)
			.ToList();

		public static readonly Dictionary<int, T> Matches = new Dictionary<int, T>();

		public static readonly HashSet<int> Misses = new HashSet<int>();

		public static void Invalidate(Pawn pawn)
		{
			if (pawn == null)
			{
				return;
			}
			int baseKey = pawn.thingIDNumber * 10;
			for (int i = 0; i < 10; i++)
			{
				Matches.Remove(baseKey + i);
				Misses.Remove(baseKey + i);
			}
		}
	}
}
