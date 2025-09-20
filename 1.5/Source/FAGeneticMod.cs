using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAGeneticMod : Mod
    {
    	public static FAGeneticModSettings settings;

    	public FAGeneticMod(ModContentPack content)
    		: base(content)
    	{
    		settings = GetSettings<FAGeneticModSettings>();
    	}

    	public override string SettingsCategory()
    	{
    		return "FA Genetic Heads";
    	}

    	public override void DoSettingsWindowContents(Rect inRect)
    	{
    		Listing_Standard listing_Standard = new Listing_Standard();
    		listing_Standard.Begin(inRect);
    		listing_Standard.CheckboxLabeled("Enable debug logging", ref settings.enableDebugLogging);
    		listing_Standard.CheckboxLabeled("Show manual head override gizmo in dev mode", ref settings.showManualOverrideGizmo);
    		listing_Standard.CheckboxLabeled("Show clear manual head override gizmo in dev mode", ref settings.showClearManualOverrideGizmo);
    		listing_Standard.End();
    	}
    }
}
