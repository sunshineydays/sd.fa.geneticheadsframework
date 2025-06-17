using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads
{
    // Main mod class that sets up settings and mod UI
    public class FAHeadMod : Mod
    {
        public static FAHeadModSettings settings;

        // Constructor gets the settings instance when mod is initialized
        public FAHeadMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<FAHeadModSettings>();
        }

        // Name of the settings category in the Mod Settings menu
        public override string SettingsCategory() => "FA Genetic Heads";

        // Draw the settings UI
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("Enable debug logging", ref settings.enableDebugLogging);
            listing.CheckboxLabeled("Allow random head variant selection", ref settings.allowRandomSelection);
            listing.End();
        }
    }
}