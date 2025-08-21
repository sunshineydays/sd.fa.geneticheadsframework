using HarmonyLib;
using FacialAnimation;
using Verse;
using System;
using System.Reflection;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_FaceTypeSetter
    {
        static Patch_FaceTypeSetter()
        {
            var harmony = new Harmony("sd.geneticheads.setterpatch");

            try
            {
                // Get closed generic base type explicitly
                var closedType = typeof(ControllerBaseComp<FacialAnimation.HeadTypeDef, HeadShapeDef>);
                var setter = AccessTools.PropertySetter(closedType, "FaceType");
                var postfix = typeof(Patch_FaceTypeSetter).GetMethod(nameof(Postfix), BindingFlags.Static | BindingFlags.NonPublic);

                if (setter == null || postfix == null)
                {
                    Log.Error("[FA Heads] Failed to patch: Could not locate setter or postfix.");
                    return;
                }

                harmony.Patch(setter, postfix: new HarmonyMethod(postfix));
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Error patching FaceType setter: " + ex);
            }
        }

        // still matches: value, instance
        static void Postfix(FacialAnimation.HeadTypeDef value, ControllerBaseComp<FacialAnimation.HeadTypeDef, HeadShapeDef> __instance)
        {
            if (__instance == null || value == null) return;

            // only run for actual HeadControllerComp instances
            HeadControllerComp headComp = __instance as HeadControllerComp;
            if (headComp == null) return;

            Pawn pawn = Traverse.Create(headComp).Field("pawn").GetValue<Pawn>();
            if (pawn == null) return;

            var existing = ManualHeadOverrides.GetOverride(pawn);

            if (Prefs.DevMode)
                Log.Message($"[FA Heads] FaceType set to {value.defName} for {pawn.LabelShortCap}. Existing override: {existing?.defName ?? "none"}");

            if (existing != value)
            {
                Utility_ApplyManualHeadOverride.ApplyManualHeadOverride(pawn, value);

                if (Prefs.DevMode)
                    Log.Message($"[FA Heads] Saved {value.defName} as manual override for {pawn.LabelShortCap}");
            }
        }
    }
}
