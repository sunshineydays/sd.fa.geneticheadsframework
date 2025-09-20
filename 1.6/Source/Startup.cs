using System;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    static class Startup
    {
        static Startup()
        {
            try
            {
                new Harmony("sd.fageneticheads.startup").PatchAll();
                Log.Message("[FA Genetic Heads] Harmony patches applied");
            }
            catch (Exception ex)
            {
                Log.Error($"[FA Genetic Heads] Failed to patch: {ex}");
            }
        }
    }
}
