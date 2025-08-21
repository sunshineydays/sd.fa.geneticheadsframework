using System.Collections.Generic;
using Verse;

namespace FacialAnimationGeneticHeads
{
    // mod extension class to define required genes for a HeadTypeDef
    public class FARequiredGenes : DefModExtension
    {
        // list of GeneDefs added by mods and patches
        public List<string> requiredGenes;
        public FARequiredGenes() { }
    }
}