using Verse;
using FacialAnimation;
using RimWorld;
using HarmonyLib;
using FacialAnimationGeneticHeads;

namespace FacialAnimationGeneticHeads
{
    /// Utility to apply and force a manual override to the HeadControllerComp's faceType.
    /// This ensures both the visual update and that the override persists in memory.
    public static class FacialAnimationUtil
    {
        /// Applies a head override to a pawn by setting its FaceType and marking graphics dirty.
        public static void ApplyManualHeadOverride(Pawn pawn, FacialAnimation.HeadTypeDef head)
        {
            if (pawn == null || head == null) return;

            var comp = pawn.GetComp<HeadControllerComp>();
            if (comp != null)
            {
                // Use persistent system
                FAHeadOverrideManager.SetOverride(pawn, head);

                // Set directly to avoid triggering patch loop
                Traverse.Create(comp).Field("faceType").SetValue(head);
                comp.ReloadIfNeed(); // Triggers graphic update

                PortraitsCache.SetDirty(pawn);
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();

                if (Prefs.DevMode)
                    Log.Message($"[FA Heads] Manually applied {head.defName} to {pawn.LabelShortCap}");
            }
        }
    }
}