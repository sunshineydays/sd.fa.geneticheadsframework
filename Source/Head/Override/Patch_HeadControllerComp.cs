using System;
using FacialAnimation;
using HarmonyLib;
using Verse;

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

                // Patch the closed generic base, not the subclass
                var baseType = typeof(ControllerBaseComp<FacialAnimation.HeadTypeDef, HeadShapeDef>);
                var method = AccessTools.Method(baseType, "InitializeIfNeed");

                if (method == null)
                {
                    Log.Error("[FA Heads] Could not find InitializeIfNeed for heads.");
                    return;
                }

                harmony.Patch(
                    original: method,
                    postfix: new HarmonyMethod(typeof(Patch_HeadControllerComp), nameof(InitializeIfNeed_Postfix))
                );

                Log.Message("[FA Heads] Head patch active.");
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Harmony patch failed: " + ex);
            }
        }

        public static void InitializeIfNeed_Postfix(object __instance)
        {
            try
            {
                var headComp = __instance as HeadControllerComp;
                if (headComp == null) return;

                Pawn pawn = Traverse.Create(headComp).Field("pawn").GetValue<Pawn>();
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
                var current = Traverse.Create(headComp).Field("faceType").GetValue<FacialAnimation.HeadTypeDef>();

                // if the current head is different, assign the new one
                if (match != null && current != match)
                {
                    Traverse.Create(headComp).Field("faceType").SetValue(match);
                    headComp.ReloadIfNeed();

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