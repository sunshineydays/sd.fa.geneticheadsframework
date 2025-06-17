using System.Collections.Generic;
using Verse;

namespace FacialAnimationGeneticHeads
{
    // Mod extension class that allows you to define required genes for a HeadTypeDef
    public class FARequiredGenes : DefModExtension
    {
        // List of gene defNames required to use the given head
        public List<string> requiredGenes;
        public FARequiredGenes() { }
    }
}