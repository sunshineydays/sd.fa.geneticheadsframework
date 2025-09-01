using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAGeneticModSettings : ModSettings
    {
    	public bool enableDebugLogging;

    	public bool showManualOverrideGizmo = true;

    	public bool showClearManualOverrideGizmo = true;

    	public override void ExposeData()
    	{
    		Scribe_Values.Look(ref enableDebugLogging, "enableDebugLogging", defaultValue: false);
    		Scribe_Values.Look(ref showManualOverrideGizmo, "showManualOverrideGizmo", defaultValue: true);
    		Scribe_Values.Look(ref showClearManualOverrideGizmo, "showClearManualOverrideGizmo", defaultValue: true);
    	}
    }
}
