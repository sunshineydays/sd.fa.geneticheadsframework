using System;
using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads;

public static class GeneFacePatchHelper
{
	public static T GetGeneMatchedDef<T>(Pawn pawn, Gender gender) where T : FaceTypeDef, new()
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
}
