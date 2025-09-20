using System;
using System.Reflection;
using FacialAnimation;
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
    			Harmony harmony = new Harmony("sd.geneticheads.patch");
    			Type type = AccessTools.TypeByName("CharacterEditor.FacialTool");
    			if (type != null)
    			{
    				MethodInfo methodInfo = AccessTools.Method(type, "FA_SetDefByName", new Type[3]
    				{
    					typeof(Pawn),
    					typeof(string),
    					typeof(string)
    				});
    				if (methodInfo != null)
    				{
    					harmony.Patch(methodInfo, null, new HarmonyMethod(typeof(Patch_CE_FacialToolHooks), "FA_SetDefByName_Postfix"));
    				}
    				MethodInfo methodInfo2 = AccessTools.Method(type, "FA_SetDef", new Type[5]
    				{
    					typeof(Pawn),
    					typeof(string),
    					typeof(bool),
    					typeof(bool),
    					typeof(bool)
    				});
    				if (methodInfo2 != null)
    				{
    					harmony.Patch(methodInfo2, null, new HarmonyMethod(typeof(Patch_CE_FacialToolHooks), "FA_SetDef_Postfix"));
    				}
    			}
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Failed to patch CE FacialTool: " + ex);
    		}
    	}

    	public static void FA_SetDefByName_Postfix(Pawn p, string controller, string defName)
    	{
    		try
    		{
    			if (p != null && !(controller != "HeadControllerComp") && !string.IsNullOrEmpty(defName))
    			{
    				FacialAnimation.HeadTypeDef namedSilentFail = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamedSilentFail(defName);
    				if (namedSilentFail != null)
    				{
    					ManualHeadOverrides.SetOverride(p, namedSilentFail);
    				}
    			}
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Error in FA_SetDefByName_Postfix: " + ex);
    		}
    	}

    	public static void FA_SetDef_Postfix(Pawn p, string controller, bool next, bool random, bool keep)
    	{
    		try
    		{
    			if (p == null || controller != "HeadControllerComp")
    			{
    				return;
    			}
    			ThingComp thingComp = p.AllComps?.FirstOrDefault((ThingComp c) => c.GetType().ToString().EndsWith("HeadControllerComp"));
    			if (thingComp != null)
    			{
    				FacialAnimation.HeadTypeDef value = Traverse.Create(thingComp).Field("faceType").GetValue<FacialAnimation.HeadTypeDef>();
    				if (value != null)
    				{
    					ManualHeadOverrides.SetOverride(p, value);
    				}
    			}
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Error in FA_SetDef_Postfix: " + ex);
    		}
    	}
    }
}
