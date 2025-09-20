using HarmonyLib;
using Verse;
using System.Reflection;
using FacialAnimation; 
using System; 

namespace FacialAnimationGeneticHeads.CompPatches
{
    [HarmonyPatch]
    public static class Patch_MouthControllerComp
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("FacialAnimation.MouthControllerComp"), "InitializeIfNeed");

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

                if (fi.FieldType != typeof(MouthTypeDef)) 
                {
                   //Log.Warning($"[FA Genetic Heads] faceType field is not of type {typeof(MouthTypeDef).Name}");
                    return true;
                }

                var current = fi.GetValue(__instance) as MouthTypeDef;
                if (current != null) return true;

                var chosen = GeneFacePatchHelper.GetGeneMatchedDef<MouthTypeDef>(pawn, pawn.gender);
                if (chosen != null)
                {
                    fi.SetValue(__instance, chosen);
                    if (Prefs.DevMode)
                        Log.Message($"[FA Genetic Heads] {typeof(MouthTypeDef).Name}: assigned {chosen.defName} to {pawn.LabelShortCap}");
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
