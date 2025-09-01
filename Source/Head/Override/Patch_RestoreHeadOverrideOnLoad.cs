using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_RestoreHeadOverrideOnLoad
    {
    	static Patch_RestoreHeadOverrideOnLoad()
    	{
    		new Harmony("sd.geneticheads.restoreload").Patch(AccessTools.Method(typeof(HeadControllerComp), "InitializeIfNeed"), null, new HarmonyMethod(typeof(Patch_RestoreHeadOverrideOnLoad), "Postfix_InitializeIfNeed"));
    	}

    	public static void Postfix_InitializeIfNeed(HeadControllerComp __instance)
    	{
    		Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
    		if (value != null && ManualHeadOverrides.GetOverride(value) == null)
    		{
    			FAHeadOverrideManager.RestoreOverrideIfAvailable(value);
    		}
    	}
    }
}
