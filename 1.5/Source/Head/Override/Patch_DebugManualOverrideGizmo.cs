using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
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
    		new Harmony("sd.geneticheads.debuggizmo").Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(Patch_DebugManualOverrideGizmo), "Postfix_GetGizmos"));
    	}

    	public static void Postfix_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
    	{
    		if (!Prefs.DevMode || !FAGeneticMod.settings.showManualOverrideGizmo || __instance == null || !__instance.RaceProps.Humanlike)
    		{
    			return;
    		}
    		List<Gizmo> list = __result.ToList();
    		list.Add(new Command_Action
    		{
    			defaultLabel = "Override Head (Dev)",
    			defaultDesc = "Manually assign a HeadTypeDef override to this pawn.",
    			icon = TexCommand.Attack,
    			action = delegate
    			{
    				List<FacialAnimation.HeadTypeDef> allDefsListForReading = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefsListForReading;
    				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
    				foreach (FacialAnimation.HeadTypeDef head in allDefsListForReading)
    				{
    					list2.Add(new FloatMenuOption("Apply " + head.defName, delegate
    					{
    						FacialAnimationUtil.ApplyManualHeadOverride(__instance, head);
    					}));
    				}
    				Find.WindowStack.Add(new FloatMenu(list2));
    			}
    		});
    		__result = list;
    	}
    }
}
