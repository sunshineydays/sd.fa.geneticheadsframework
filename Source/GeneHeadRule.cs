using System.Collections.Generic;
using Verse;

namespace FacialAnimationGeneticHeads
{
    // represents a set of required genes and the head(s) it maps to
    public class GeneHeadRule
    {
        public HashSet<GeneDef> requiredGenes; // gene combo that must be active
        public List<FacialAnimation.HeadTypeDef> headDefs; // heads to choose from

        public GeneHeadRule(HashSet<GeneDef> genes, FacialAnimation.HeadTypeDef def)
        {
            requiredGenes = genes;
            headDefs = new List<FacialAnimation.HeadTypeDef> { def };
        }
    }
}