using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads;

public class FacialAnimationGeneticHeadsMod : Mod
{
	public static FacialAnimationGeneticHeadsSettings Settings;

	public FacialAnimationGeneticHeadsMod(ModContentPack content)
		: base(content)
	{
		Settings = GetSettings<FacialAnimationGeneticHeadsSettings>();
	}

	public override string SettingsCategory()
	{
		return "FA Genetic Heads Patch Settings";
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(inRect);
		listing_Standard.Label("Enable individual facial render components:");
		listing_Standard.GapLine();
		listing_Standard.CheckboxLabeled("Enable Brow Patch", ref Settings.BrowCompActive);
		listing_Standard.CheckboxLabeled("Enable Eyeball Patch (disable for eye color problems)", ref Settings.EyeballCompActive);
		listing_Standard.CheckboxLabeled("Enable Head Patch", ref Settings.HeadCompActive);
		listing_Standard.CheckboxLabeled("Enable Lid Patch", ref Settings.LidCompActive);
		listing_Standard.CheckboxLabeled("Enable Mouth Patch", ref Settings.MouthCompActive);
		listing_Standard.CheckboxLabeled("Enable Skin Patch", ref Settings.SkinCompActive);
		listing_Standard.End();
	}
}
