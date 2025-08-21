using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_CE_FacialToolHooks
    {
        static Patch_CE_FacialToolHooks()
        {
            try
            {
                var harmony = new Harmony("sd.geneticheads.patch");
                var ceFacialTool = AccessTools.TypeByName("CharacterEditor.FacialTool");

                if (ceFacialTool != null)
                {
                    // Patch FA_SetDefByName(this Pawn p, string controller, string defName)
                    var mSetByName = AccessTools.Method(ceFacialTool, "FA_SetDefByName", new[] { typeof(Pawn), typeof(string), typeof(string) });
                    if (mSetByName != null)
                    {
                        harmony.Patch(mSetByName,
                            postfix: new HarmonyMethod(typeof(Patch_CE_FacialToolHooks), nameof(FA_SetDefByName_Postfix)));
                    }

                    // Patch FA_SetDef(this Pawn p, string controller, bool next, bool random, bool keep=false)
                    var mSet = AccessTools.Method(ceFacialTool, "FA_SetDef", new[] { typeof(Pawn), typeof(string), typeof(bool), typeof(bool), typeof(bool) });
                    if (mSet != null)
                    {
                        harmony.Patch(mSet,
                            postfix: new HarmonyMethod(typeof(Patch_CE_FacialToolHooks), nameof(FA_SetDef_Postfix)));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Failed to patch CE FacialTool: " + ex);
            }
        }

        // If CE sets a specific head by name, capture it as a manual override
        public static void FA_SetDefByName_Postfix(Pawn p, string controller, string defName)
        {
            try
            {
                if (p == null || controller != "HeadControllerComp" || string.IsNullOrEmpty(defName)) return;

                var headDef = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamedSilentFail(defName);
                if (headDef != null)
                {
                    ManualHeadOverrides.SetOverride(p, headDef); // helper — persist this choice
                }
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Error in FA_SetDefByName_Postfix: " + ex);
            }
        }

        // If CE cycles/randomizes the head, also mark it as a manual override
        public static void FA_SetDef_Postfix(Pawn p, string controller, bool next, bool random, bool keep)
        {
            try
            {
                if (p == null || controller != "HeadControllerComp") return;

                // Read current head from FA HeadControllerComp and store as override
                var comp = p.AllComps?.FirstOrDefault(c => c.GetType().ToString().EndsWith("HeadControllerComp"));
                if (comp != null)
                {
                    var head = Traverse.Create(comp).Field("faceType").GetValue<FacialAnimation.HeadTypeDef>();
                    if (head != null) ManualHeadOverrides.SetOverride(p, head);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[FA Heads] Error in FA_SetDef_Postfix: " + ex);
            }
        }
    }
}
