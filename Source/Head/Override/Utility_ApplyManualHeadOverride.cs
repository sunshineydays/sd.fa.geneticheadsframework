using Verse;
using FacialAnimation;
using RimWorld;
using HarmonyLib;

namespace FacialAnimationGeneticHeads
{
    // utility to apply and force a manual override to the HeadControllerComp's faceType.
    public static class Utility_ApplyManualHeadOverride
    {
        // applies a head override to a pawn by setting its FaceType and marking graphics dirty
        public static void ApplyManualHeadOverride(Pawn pawn, FacialAnimation.HeadTypeDef head)
        {
            if (pawn == null || head == null) return;

            var comp = pawn.GetComp<HeadControllerComp>();
            if (comp != null)
            {
                // use persistent system
                FAHeadOverrideManager.SetOverride(pawn, head);

                // set directly to avoid triggering patch loop
                Traverse.Create(comp).Field("faceType").SetValue(head);
                comp.ReloadIfNeed(); 

                PortraitsCache.SetDirty(pawn);
                pawn.Drawer?.renderer?.SetAllGraphicsDirty();

                if (Prefs.DevMode)
                    Log.Message($"[FA Heads] Manually applied {head.defName} to {pawn.LabelShortCap}");
            }
        }
    }
}