using HarmonyLib;
using Verse;
using FacialAnimation;
using RimWorld;

namespace FacialAnimationGeneticHeads
{
    // This Harmony patch ensures that if a manual override exists for a pawn, it will always be enforced—even if another system (like the UI or refresh) tries to set a different FaceType.
    [HarmonyPatch(typeof(HeadControllerComp))]
    [HarmonyPatch("set_FaceType")]  // Intercepts the FaceType property setter
    public static class Patch_FaceTypeSetter
    {
        // Postfix runs AFTER FaceType is set by anything (UI, refresh, other mods).
        // If a manual override exists for this pawn, it will re-apply it and force a reload.
        public static void Postfix(HeadControllerComp __instance, FacialAnimation.HeadTypeDef value)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null) return;

            FacialAnimation.HeadTypeDef manual = ManualHeadOverrides.GetOverride(pawn);

            // Prevent infinite loop: only override if the value is not already the manual one
            if (manual != null && value != manual)
            {
                // Only set field directly, avoid calling setter again
                Traverse.Create(__instance).Field("faceType").SetValue(manual);
                __instance.ReloadIfNeed();
                PortraitsCache.SetDirty(pawn);
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();

                if (Prefs.DevMode)
                    Log.Message($"[FA Heads] Re-applied manually assigned head '{manual.defName}' to {pawn.LabelShortCap}");
            }
        }
    }
}