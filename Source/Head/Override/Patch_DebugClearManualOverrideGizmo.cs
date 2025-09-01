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
    		new Harmony("sd.geneticheads.debugclear").Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(Patch_DebugClearManualOverrideGizmo), "Postfix_GetGizmos"));
    	}

    	public static void Postfix_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
    	{
    		if (!Prefs.DevMode || !FAGeneticMod.settings.showClearManualOverrideGizmo || __instance == null || !__instance.RaceProps.Humanlike)
    		{
    			return;
    		}
    		List<Gizmo> list = __result.ToList();
    		list.Add(new Command_Action
    		{
    			defaultLabel = "Clear Head Override (Dev)",
    			defaultDesc = "Remove manual head override from this pawn.",
    			icon = TexCommand.ClearPrioritizedWork,
    			action = delegate
    			{
    				FAHeadOverrideManager.ClearOverride(__instance);
    				__instance.GetComp<HeadControllerComp>()?.ReloadIfNeed();
    				PortraitsCache.SetDirty(__instance);
    				__instance.Drawer?.renderer?.SetAllGraphicsDirty();
    				if (Prefs.DevMode)
    				{
    					Log.Message("[FA Heads] Cleared manual override for " + __instance.LabelCap);
    				}
    			}
    		});
    		__result = list;
    	}
    }
}
