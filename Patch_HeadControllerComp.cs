using HarmonyLib;
using Verse;
using FacialAnimation;
using System;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_HeadControllerComp
    {
        static Patch_HeadControllerComp()
        {
            try
            {
                var harmony = new Harmony("sd.geneticheads.patch");
                harmony.Patch(
                    original: AccessTools.Method(typeof(HeadControllerComp), nameof(HeadControllerComp.InitializeIfNeed)),
                    postfix: new HarmonyMethod(typeof(Patch_HeadControllerComp), nameof(InitializeIfNeed_Postfix))
                );
                Log.Message("[FA Heads] Harmony patch active.");
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Harmony patch failed: " + ex);
            }
        }

        public static void InitializeIfNeed_Postfix(HeadControllerComp __instance)
        {
            try
            {
                // get pawn reference via reflection (private field)
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn == null || pawn.genes == null)
                {
                    if (Prefs.DevMode)
                        Log.Warning("[FA Heads] Pawn or gene tracker was null. Skipping.");
                    return;
                }

                // match head based on override, genes, or fallback 
                var manual = ManualHeadOverrides.GetOverride(pawn);
                var match = manual ?? GeneHeadResolver.Match(pawn) ?? FallbackHeadUtility.GetFallbackHead(pawn);

                // get current faceType
                var current = Traverse.Create(__instance).Field("faceType").GetValue<FacialAnimation.HeadTypeDef>();

                // if the current head is different, assign the new one
                if (match != null && current != match)
                {
                    Traverse.Create(__instance).Field("faceType").SetValue(match);
                    __instance.ReloadIfNeed();

                    if (Prefs.DevMode)
                        Log.Message($"[FA Heads] Assigned head '{match.defName}' to {pawn.LabelShortCap}");
                }
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Error in HeadControllerComp patch: " + ex);
            }
        }
    }
}