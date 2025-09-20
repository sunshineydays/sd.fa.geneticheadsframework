using System.Reflection;
using FacialAnimation; 
using HarmonyLib;
using Verse;
using System;

namespace FacialAnimationGeneticHeads.CompPatches
{
    [HarmonyPatch]
    public static class Patch_LidControllerComp
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("FacialAnimation.LidControllerComp"), "InitializeIfNeed");

        static bool Prefix(object __instance)
        {
            try
            {
                var comp = __instance as ThingComp;
                var pawn = comp?.parent as Pawn;
                if (pawn == null) return true;

                var fi = AccessTools.Field(__instance.GetType(), "faceType");
                if (fi == null)
                {
                    Log.Warning($"[FA GenePatch] Missing faceType field on {__instance.GetType().FullName}");
                    return true;
                }

                if (fi.FieldType != typeof(LidTypeDef)) // e.g. EyeballTypeDef
                {
                   //Log.Warning($"[FA GenePatch] faceType field is not of type {typeof(LidTypeDef).Name}");
                    return true;
                }

                var current = fi.GetValue(__instance) as LidTypeDef;
                if (current != null) return true;

                var chosen = GeneFacePatchHelper.GetGeneMatchedDef<LidTypeDef>(pawn, pawn.gender);
                if (chosen != null)
                {
                    fi.SetValue(__instance, chosen);
                    if (Prefs.DevMode)
                        Log.Message($"[FA GenePatch] {typeof(LidTypeDef).Name}: assigned {chosen.defName} to {pawn.LabelShortCap}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FA GenePatch] Exception in patch for {__instance.GetType().FullName}: {ex}");
                return true; // Let vanilla run
            }
        }
    }
}
