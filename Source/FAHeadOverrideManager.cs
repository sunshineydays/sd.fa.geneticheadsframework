using System.Collections.Generic;
using System.Linq;
using Verse;
using FacialAnimation;
using UnityEngine;
using HarmonyLib;
using RimWorld;

namespace FacialAnimationGeneticHeads
{
    public class FAHeadOverrideManager : GameComponent
    {
        // Dictionary that stores override assignments by unique pawn ID
        private static Dictionary<string, string> overrideLookup = new Dictionary<string, string>();

        public FAHeadOverrideManager(Game game) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref overrideLookup, "FA_HeadOverrides", LookMode.Value, LookMode.Value, ref keysWorkingList, ref valuesWorkingList);
        }

        private List<string> keysWorkingList;
        private List<string> valuesWorkingList;

        public static void SetOverride(Pawn pawn, FacialAnimation.HeadTypeDef head)
        {
            ManualHeadOverrides.SetOverride(pawn, head);
            overrideLookup[pawn.ThingID] = head.defName;
        }

        public static void ClearOverride(Pawn pawn)
        {
            ManualHeadOverrides.ClearOverride(pawn);
            overrideLookup.Remove(pawn.ThingID);
        }

        public static void RestoreOverrideIfAvailable(Pawn pawn)
        {
            if (overrideLookup.TryGetValue(pawn.ThingID, out var defName))
            {
                var head = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamedSilentFail(defName);
                if (head != null)
                {
                    ManualHeadOverrides.SetOverride(pawn, head);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class DebugClearManualOverrideGizmo
    {
        static DebugClearManualOverrideGizmo()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("sd.geneticheads.debugclear");
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyLib.HarmonyMethod(typeof(DebugClearManualOverrideGizmo), nameof(Postfix_GetGizmos))
            );
        }

        public static void Postfix_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!Prefs.DevMode || __instance == null || !__instance.RaceProps.Humanlike)
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

    // Integrate into pawn initialization
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