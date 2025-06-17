using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using FacialAnimation;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class GeneHeadResolver
    {
        // List of all valid gene-head rules loaded from defs
        public static List<GeneHeadRule> GeneHeadMap = new List<GeneHeadRule>();

        // Cache to avoid recomputing gene matches per pawn
        private static Dictionary<HashSet<GeneDef>, FacialAnimation.HeadTypeDef> headCache =
            new Dictionary<HashSet<GeneDef>, FacialAnimation.HeadTypeDef>(new GeneSetComparer());

        static GeneHeadResolver()
        {
            try
            {
                // Load all HeadTypeDefs that use FARequiredGenes and group them by gene set
                foreach (FacialAnimation.HeadTypeDef headDef in DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs)
                {
                    FARequiredGenes ext = headDef.GetModExtension<FARequiredGenes>();
                    if (ext == null || ext.requiredGenes == null)
                        continue;

                    HashSet<GeneDef> geneDefs = new HashSet<GeneDef>();
                    foreach (string defName in ext.requiredGenes)
                    {
                        GeneDef g = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
                        if (g != null)
                            geneDefs.Add(g);
                    }

                    if (geneDefs.Count == 0)
                        continue;

                    // Add headDef to an existing rule or create a new rule
                    bool added = false;
                    foreach (GeneHeadRule rule in GeneHeadMap)
                    {
                        if (rule.requiredGenes.SetEquals(geneDefs))
                        {
                            rule.headDefs.Add(headDef);
                            added = true;
                            break;
                        }
                    }

                    if (!added)
                        GeneHeadMap.Add(new GeneHeadRule(geneDefs, headDef));
                }

                // Sort rules by specificity (most genes first)
                GeneHeadMap.Sort((a, b) => b.requiredGenes.Count.CompareTo(a.requiredGenes.Count));
                Log.Message("[FA Heads] Loaded " + GeneHeadMap.Count + " gene-head definition groups.");
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Error loading gene map: " + ex);
            }
        }

        // Attempts to find the most appropriate head for a pawn's active genes
        public static FacialAnimation.HeadTypeDef Match(Pawn pawn)
        {
            if (pawn == null || pawn.genes == null) return null;

            HashSet<GeneDef> activeGenes = new HashSet<GeneDef>(
                pawn.genes.GenesListForReading.Where(g => g.Active).Select(g => g.def));

            // Return cached result if available
            foreach (var entry in headCache.Keys)
            {
                if (entry.SetEquals(activeGenes))
                    return headCache[entry];
            }

            // Try matching rules in order of specificity
            foreach (GeneHeadRule rule in GeneHeadMap)
            {
                if (rule.requiredGenes.IsSubsetOf(activeGenes))
                {
                    var head = rule.headDefs.Count == 1
                        ? rule.headDefs[0]
                        : rule.headDefs[Rand.Range(0, rule.headDefs.Count)];

                    headCache[activeGenes] = head;
                    return head;
                }
            }

            // No match found, cache null result
            headCache[activeGenes] = null;
            return null;
        }

        // Custom hash comparer for HashSet<GeneDef> to be used as dictionary keys
        private class GeneSetComparer : IEqualityComparer<HashSet<GeneDef>>
        {
            public bool Equals(HashSet<GeneDef> x, HashSet<GeneDef> y)
            {
                return x.SetEquals(y);
            }

            public int GetHashCode(HashSet<GeneDef> obj)
            {
                unchecked
                {
                    int hash = 17;
                    foreach (var gene in obj.OrderBy(g => g.shortHash))
                        hash = hash * 31 + gene.shortHash;
                    return hash;
                }
            }
        }
    }
}