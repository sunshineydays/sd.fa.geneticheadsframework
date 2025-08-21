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
            new HarmonyLib.Harmony("sd.geneticheads.restoreload").Patch(
                original: AccessTools.Method(typeof(HeadControllerComp), nameof(HeadControllerComp.InitializeIfNeed)),
                postfix: new HarmonyLib.HarmonyMethod(typeof(Patch_RestoreHeadOverrideOnLoad), nameof(Postfix_InitializeIfNeed))
            );
        }

        public static void Postfix_InitializeIfNeed(HeadControllerComp __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null && ManualHeadOverrides.GetOverride(pawn) == null)
            {
                FAHeadOverrideManager.RestoreOverrideIfAvailable(pawn);
            }
        }
    }
}
