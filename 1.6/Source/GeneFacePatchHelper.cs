// helper: finds the best matching FaceTypeDef for a pawn based on active genes
// used by FA gene patch logic for each facial part

using System;
using System.Linq;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public static class GeneFacePatchHelper
    {
        public static T GetGeneMatchedDef<T>(Pawn pawn, Gender gender) where T : FaceTypeDef, new()
        {
            if (pawn == null)
                return null;

            try
            {
                // grab all defs that match this pawn's race and genes
                var candidates = FaceTypeGenerator<T>.GetApplicableFaceTypeDefsForRaceConsideringGenes(pawn);
                if (candidates == null || !candidates.Any())
                    return null;

                // collect active gene defNames for fast matching
                var genes = pawn.genes?.GenesListForReading?.Select(g => g.def.defName).ToHashSet();

                // filter to defs that require genes and all are present
                var matches = candidates
                    .Where(def =>
                    {
                        if (def.targetGeneDefs == null || def.targetGeneDefs.Count == 0) return false;
                        return genes != null && def.targetGeneDefs.All(genes.Contains);
                    })
                    .GroupBy(def => def.targetGeneDefs.Count)   // more required genes = more specific
                    .OrderByDescending(g => g.Key)
                    .FirstOrDefault()
                    ?.ToList();

                if (matches != null && matches.Count > 0)
                {
                    // prefer gender-matching defs, fallback to any
                    var pool = matches.Where(d => d.gender == gender || d.gender == Gender.None).ToList();
                    if (pool.Count == 0) pool = matches;

                    // pick random from pool using weighted probability
                    Rand.PushState(pawn.thingIDNumber);
                    try
                    {
                        if (pool.TryRandomElementByWeight(d => d.probability > 0f ? d.probability : 0f, out var picked))
                            return picked;
                        return pool.RandomElement();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"[FA GenePatch] Failed to pick random FaceTypeDef: {ex}");
                        return pool.First(); // safe fallback
                    }
                    finally
                    {
                        Rand.PopState();
                    }
                }

                // fallback to any matching type if no gene match found
                return candidates.GetRandomWithFaceProbability();
            }
            catch (Exception ex)
            {
                Log.Error($"[FA GenePatch] Exception while getting {typeof(T).Name} for {pawn?.LabelShortCap ?? "null pawn"}: {ex}");
                return null;
            }
        }
    }
}
