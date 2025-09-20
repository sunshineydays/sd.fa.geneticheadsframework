using System.Collections.Generic;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public class GeneHeadRule
    {
    	public HashSet<GeneDef> requiredGenes;

    	public List<FacialAnimation.HeadTypeDef> headDefs;

    	public GeneHeadRule(HashSet<GeneDef> genes, FacialAnimation.HeadTypeDef def)
    	{
    		requiredGenes = genes;
    		headDefs = new List<FacialAnimation.HeadTypeDef> { def };
    	}
    }
}
