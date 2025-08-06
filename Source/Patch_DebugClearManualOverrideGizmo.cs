using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_DebugClearManualOverrideGizmo
    {
        static Patch_DebugClearManualOverrideGizmo()
        {
            // adds a gizmo to humanlike pawns to CLEAR manual override heads via dev mode
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("sd.geneticheads.debugclear");
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyLib.HarmonyMethod(typeof(Patch_DebugClearManualOverrideGizmo), nameof(Postfix_GetGizmos))
            );
        }

        public static void Postfix_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!Prefs.DevMode || !FAHeadMod.settings.showClearManualOverrideGizmo || __instance == null || !__instance.RaceProps.Humanlike)
                return;

            List<Gizmo> list = __result.ToList();

            list.Add(new Command_Action
            {
                defaultLabel = "Clear Head Override (Dev)",
                defaultDesc = "Remove manual head override from this pawn.",
                icon = TexCommand.ClearPrioritizedWork,
                action = () =>
                {
                    FAHeadOverrideManager.ClearOverride(__instance);
                    __instance.GetComp<HeadControllerComp>()?.ReloadIfNeed();
                    PortraitsCache.SetDirty(__instance);
                    __instance.Drawer?.renderer?.SetAllGraphicsDirty();

                    if (Prefs.DevMode)
                        Log.Message($"[FA Heads] Cleared manual override for {__instance.LabelCap}");
                }
            });

            __result = list;
        }
    }
}