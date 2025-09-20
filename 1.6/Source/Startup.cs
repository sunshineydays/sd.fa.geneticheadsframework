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
                Log.Message("[FA GenePatch] Harmony patches applied");
            }
            catch (Exception ex)
            {
                Log.Error($"[FA GenePatch] Failed to patch: {ex}");
            }
        }
    }
}
