using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAGeneticModSettings : Verse.ModSettings
    {
        public bool enableDebugLogging = false;
        //public bool allowRandomSelection = true;
        public bool showManualOverrideGizmo = true;
        public bool showClearManualOverrideGizmo = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableDebugLogging, "enableDebugLogging", false);
            //Scribe_Values.Look(ref allowRandomSelection, "allowRandomSelection", true);
            Scribe_Values.Look(ref showManualOverrideGizmo, "showManualOverrideGizmo", true);
            Scribe_Values.Look(ref showClearManualOverrideGizmo, "showClearManualOverrideGizmo", true);
        }
    }
}
