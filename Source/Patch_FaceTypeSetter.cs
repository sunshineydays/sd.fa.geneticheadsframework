using HarmonyLib;
using Verse;
using FacialAnimation;
using RimWorld;

namespace FacialAnimationGeneticHeads
{
    // if a manual override exists, it will always be enforced
    [HarmonyPatch(typeof(HeadControllerComp))]
    [HarmonyPatch("set_FaceType")] 
    public static class Patch_FaceTypeSetter
    {
        // postfix runs AFTER FaceType is set by anything (UI, refresh, other mods)
        // if a manual override exists for this pawn, it will re-apply it
        public static void Postfix(HeadControllerComp __instance, FacialAnimation.HeadTypeDef value)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null) return;

            FacialAnimation.HeadTypeDef manual = ManualHeadOverrides.GetOverride(pawn);

            // prevent infinite loop: only override if the value is not already the manual one
            if (manual != null && value != manual)
            {
                // only set field directly, avoid calling setter again
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