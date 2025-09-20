using System;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_HeadControllerComp
    {
    	static Patch_HeadControllerComp()
    	{
    		try
    		{
    			new Harmony("sd.geneticheads.patch").Patch(AccessTools.Method(typeof(HeadControllerComp), "InitializeIfNeed"), null, new HarmonyMethod(typeof(Patch_HeadControllerComp), "InitializeIfNeed_Postfix"));
    			Log.Message("[FA Heads] Harmony patch active.");
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Harmony patch failed: " + ex);
    		}
    	}

    	public static void InitializeIfNeed_Postfix(HeadControllerComp __instance)
    	{
    		try
    		{
    			Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
    			if (value == null || value.genes == null)
    			{
    				if (Prefs.DevMode)
    				{
    					Log.Warning("[FA Heads] Pawn or gene tracker was null. Skipping.");
    				}
    				return;
    			}
    			FacialAnimation.HeadTypeDef headTypeDef = ManualHeadOverrides.GetOverride(value) ?? GeneHeadResolver.Match(value) ?? FallbackHeadUtility.GetFallbackHead(value);
    			FacialAnimation.HeadTypeDef value2 = Traverse.Create(__instance).Field("faceType").GetValue<FacialAnimation.HeadTypeDef>();
    			if (headTypeDef != null && value2 != headTypeDef)
    			{
    				Traverse.Create(__instance).Field("faceType").SetValue(headTypeDef);
    				__instance.ReloadIfNeed();
    				if (Prefs.DevMode)
    				{
    					Log.Message("[FA Heads] Assigned head '" + headTypeDef.defName + "' to " + value.LabelShortCap);
    				}
    			}
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Error in HeadControllerComp patch: " + ex);
    		}
    	}
    }
}
