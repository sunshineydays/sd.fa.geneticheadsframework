using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAHeadMod : Mod
    {
        public static FAHeadModSettings settings;

        public FAHeadMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<FAHeadModSettings>();
        }

        public override string SettingsCategory() => "FA Genetic Heads";

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