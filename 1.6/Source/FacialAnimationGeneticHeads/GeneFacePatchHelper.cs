using System;
using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads;

public static class GeneFacePatchHelper
{
	public static T GetMatchedDef<T>(Pawn pawn, Gender gender) where T : FaceTypeDef, new()
	{
		return FaceConditionResolver.Match<T>(pawn, gender) ?? GetGeneMatchedDef<T>(pawn, gender);
	}

	public static FacialAnimation.HeadTypeDef GetHeadMatchedDef(Pawn pawn, Gender gender)
	{
		return GetMatchedDef<FacialAnimation.HeadTypeDef>(pawn, gender);
	}

	public static T GetGeneMatchedDef<T>(Pawn pawn, Gender gender) where T : FaceTypeDef, new()
	{
		if (pawn == null)
		{
			return null;
		}
		int cacheKey = CacheKey(pawn, gender);
		if (GeneMatches<T>.Matches.TryGetValue(cacheKey, out T cachedMatch))
		{
			return cachedMatch;
		}
		if (GeneMatches<T>.Misses.Contains(cacheKey))
		{
			return null;
		}
		T match = GetGeneMatchedDefUncached<T>(pawn, gender);
		if (match == null)
		{
			GeneMatches<T>.Misses.Add(cacheKey);
			return null;
		}
		GeneMatches<T>.Matches[cacheKey] = match;
		return match;
	}

	public static void InvalidatePawn(Pawn pawn)
	{
		GeneMatches<FacialAnimation.HeadTypeDef>.Invalidate(pawn);
		GeneMatches<EyeballTypeDef>.Invalidate(pawn);
		GeneMatches<BrowTypeDef>.Invalidate(pawn);
		GeneMatches<LidTypeDef>.Invalidate(pawn);
		GeneMatches<MouthTypeDef>.Invalidate(pawn);
		GeneMatches<SkinTypeDef>.Invalidate(pawn);
	}

	private static T GetGeneMatchedDefUncached<T>(Pawn pawn, Gender gender) where T : FaceTypeDef, new()
	{
		if (pawn == null)
		{
			return null;
		}
		try
		{
			IEnumerable<T> applicableFaceTypeDefsForRaceConsideringGenes = FaceTypeGenerator<T>.GetApplicableFaceTypeDefsForRaceConsideringGenes(pawn);
			if (applicableFaceTypeDefsForRaceConsideringGenes == null || !applicableFaceTypeDefsForRaceConsideringGenes.Any())
			{
				return null;
			}
			HashSet<string> genes = pawn.genes?.GenesListForReading?.Select((Gene g) => g.def.defName).ToHashSet();
			List<T> list = (from def in applicableFaceTypeDefsForRaceConsideringGenes.Where(delegate(T def)
				{
					if (def.targetGeneDefs == null || def.targetGeneDefs.Count == 0)
					{
						return false;
					}
					return genes != null && def.targetGeneDefs.All(genes.Contains);
				})
				group def by def.targetGeneDefs.Count into g
				orderby g.Key descending
				select g).FirstOrDefault()?.ToList();
			if (list != null && list.Count > 0)
			{
				List<T> list2 = list.Where((T d) => d.gender == gender || d.gender == Gender.None).ToList();
				if (list2.Count == 0)
				{
					list2 = list;
				}
				Rand.PushState(pawn.thingIDNumber);
				try
				{
					if (list2.TryRandomElementByWeight((T d) => (!(d.probability > 0f)) ? 0f : d.probability, out var result))
					{
						return result;
					}
					return list2.RandomElement();
				}
				catch (Exception arg)
				{
					Log.Warning($"[FA Genetic Heads] Failed to pick random FaceTypeDef: {arg}");
					return list2.First();
				}
				finally
				{
					Rand.PopState();
				}
			}
			return applicableFaceTypeDefsForRaceConsideringGenes.GetRandomWithFaceProbability();
		}
		catch (Exception arg2)
		{
			Log.Error(string.Format("[FA Genetic Heads] Exception while getting {0} for {1}: {2}", typeof(T).Name, pawn?.LabelShortCap ?? "null pawn", arg2));
			return null;
		}
	}

	public static bool HasRequiredGenes(FaceTypeDef def)
	{
		if (def?.targetGeneDefs != null)
		{
			return def.targetGeneDefs.Count > 0;
		}
		return false;
	}

	public static int RequiredGeneCount(FaceTypeDef def)
	{
		return def?.targetGeneDefs?.Count ?? 0;
	}

	public static bool IsValidForGenes(Pawn pawn, FaceTypeDef def)
	{
		if (def == null)
		{
			return false;
		}
		if (def.targetGeneDefs == null || def.targetGeneDefs.Count == 0)
		{
			return true;
		}
		if (pawn?.genes == null)
		{
			return false;
		}
		HashSet<string> hashSet = new HashSet<string>(pawn.genes.GenesListForReading.Select((Gene g) => g.def.defName));
		return def.targetGeneDefs.All(hashSet.Contains);
	}

	private static int CacheKey(Pawn pawn, Gender gender)
	{
		return pawn.thingIDNumber * 10 + (int)gender;
	}

	private static class GeneMatches<T> where T : FaceTypeDef, new()
	{
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
