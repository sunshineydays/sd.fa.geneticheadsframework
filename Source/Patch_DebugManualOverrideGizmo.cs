using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_DebugManualOverrideGizmo
    {
        static Patch_DebugManualOverrideGizmo()
        {
            // adds a gizmo to humanlike pawns to manually override heads via dev mode
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("sd.geneticheads.debuggizmo");
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyLib.HarmonyMethod(typeof(Patch_DebugManualOverrideGizmo), nameof(Postfix_GetGizmos))
            );
        }

        public static void Postfix_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!Prefs.DevMode || !FAHeadMod.settings.showManualOverrideGizmo || __instance == null || !__instance.RaceProps.Humanlike)
                return;

            List<Gizmo> list = __result.ToList();

            list.Add(new Command_Action
            {
                defaultLabel = "Override Head (Dev)",
                defaultDesc = "Manually assign a HeadTypeDef override to this pawn.",
                icon = TexCommand.Attack, // For now
                action = () =>
                {
                    var allHeads = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefsListForReading;
                    var options = new List<FloatMenuOption>();
                    foreach (var head in allHeads)
                    {
                        options.Add(new FloatMenuOption($"Apply {head.defName}", () =>
                        {
                            FacialAnimationUtil.ApplyManualHeadOverride(__instance, head);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            });

            __result = list;
        }
    }
}