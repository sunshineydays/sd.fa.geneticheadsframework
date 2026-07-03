using Verse;

namespace FacialAnimationGeneticHeads;

public class FacialAnimationGeneticHeadsSettings : ModSettings
{
	public bool BrowCompActive = true;

	public bool EyeballCompActive = true;

	public bool HeadCompActive = true;

	public bool LidCompActive = true;

	public bool MouthCompActive = true;

	public bool SkinCompActive = true;

	public override void ExposeData()
	{
		Scribe_Values.Look(ref BrowCompActive, "BrowCompActive", defaultValue: true);
		Scribe_Values.Look(ref EyeballCompActive, "EyeballCompActive", defaultValue: true);
		Scribe_Values.Look(ref HeadCompActive, "HeadCompActive", defaultValue: true);
		Scribe_Values.Look(ref LidCompActive, "LidCompActive", defaultValue: true);
		Scribe_Values.Look(ref MouthCompActive, "MouthCompActive", defaultValue: true);
		Scribe_Values.Look(ref SkinCompActive, "SkinCompActive", defaultValue: true);
	}
}
