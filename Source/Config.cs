using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAHeadModSettings : ModSettings
    {
        public bool enableDebugLogging = false;
        public bool allowRandomSelection = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableDebugLogging, "enableDebugLogging", false);
            Scribe_Values.Look(ref allowRandomSelection, "allowRandomSelection", true);
        }
    }
}
