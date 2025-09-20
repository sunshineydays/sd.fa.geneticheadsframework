using HarmonyLib;
using Verse;
using System.Reflection;
using FacialAnimation;
using System;

namespace FacialAnimationGeneticHeads.CompPatches
{
    [HarmonyPatch]
    public static class Patch_SkinControllerComp
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("FacialAnimation.SkinControllerComp"), "InitializeIfNeed");

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
                    Log.Warning($"[FA Genetic Heads] Missing faceType field on {__instance.GetType().FullName}");
                    return true;
                }

                if (fi.FieldType != typeof(SkinTypeDef))
                {
                   //Log.Warning($"[FA Genetic Heads] faceType field is not of type {typeof(SkinTypeDef).Name}");
                    return true;
                }

                var current = fi.GetValue(__instance) as SkinTypeDef;
                if (current != null) return true;

                var chosen = GeneFacePatchHelper.GetGeneMatchedDef<SkinTypeDef>(pawn, pawn.gender);
                if (chosen != null)
                {
                    fi.SetValue(__instance, chosen);
                    if (Prefs.DevMode)
                        Log.Message($"[FA Genetic Heads] {typeof(SkinTypeDef).Name}: assigned {chosen.defName} to {pawn.LabelShortCap}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FA Genetic Heads] Exception in patch for {__instance.GetType().FullName}: {ex}");
                return true; 
            }
        }
    }
}
