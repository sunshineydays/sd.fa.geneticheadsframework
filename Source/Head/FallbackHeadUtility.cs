using Verse;
using System.Collections.Generic;
using System.Linq;

namespace FacialAnimationGeneticHeads
{
    public static class FallbackHeadUtility
    {
        // cache fallback heads per pawn to keep selection consistent
        private static readonly Dictionary<Pawn, FacialAnimation.HeadTypeDef> fallbackCache = new Dictionary<Pawn, FacialAnimation.HeadTypeDef>();

        // determines the fallback head for pawns with no gene-matched head
        public static FacialAnimation.HeadTypeDef GetFallbackHead(Pawn pawn)
        {
            if (pawn == null || pawn.def == null) return null;

            if (fallbackCache.TryGetValue(pawn, out var cached))
                return cached;

            string raceName = pawn.def.defName;
            Gender gender = pawn.gender;

            // try to find heads specifically for this race and gender (HAR-compatible?)
            var candidates = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs
                .Where(h => h.raceName == raceName && (h.gender == gender || h.gender == Gender.None))
                .ToList();

            // fallback to just gender-compatible heads
            if (candidates.Count == 0)
            {
                candidates = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs
                    .Where(h => h.gender == gender || h.gender == Gender.None)
                    .ToList();
            }

            // final fallback to any defined head
            if (candidates.Count == 0)
            {
                Log.Warning($"[FA Heads] No suitable fallback HeadTypeDef found for pawn {pawn.NameShortColored}.");
                var defaultHead = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs.FirstOrDefault();
                fallbackCache[pawn] = defaultHead;
                return defaultHead;
            }

            // weighted selection, now deterministic per pawn by seeding with thingIDNumber
            float totalWeight = candidates.Sum(h => h.probability);

            float rand;
            Rand.PushState();
            try
            {
                Rand.Seed = pawn.thingIDNumber; // deterministic per pawn
                rand = Rand.Value * totalWeight; 
            }
            finally
            {
                Rand.PopState();
            }

            foreach (var head in candidates)
            {
                if (rand < head.probability)
                {
                    fallbackCache[pawn] = head;
                    return head;
                }
                rand -= head.probability;
            }

            // in case rounding leaves none selected
            fallbackCache[pawn] = candidates.First();
            return candidates.First();
        }
    }
}
