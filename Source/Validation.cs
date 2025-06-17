using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialAnimationGeneticHeads
{
    // Simple utility to check that a GeneHeadRule is valid
    public static class Validation
    {
        public static bool ValidateHeadRule(GeneHeadRule rule)
        {
            if (rule.requiredGenes == null || rule.requiredGenes.Count == 0)
                return false;
            if (rule.headDefs == null || rule.headDefs.Count == 0)
                return false;
            return true;
        }
    }
}