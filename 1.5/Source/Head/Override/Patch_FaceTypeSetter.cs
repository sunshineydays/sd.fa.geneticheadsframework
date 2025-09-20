using System;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class Patch_FaceTypeSetter
    {
    	static Patch_FaceTypeSetter()
    	{
    		Harmony harmony = new Harmony("sd.geneticheads.setterpatch");
    		try
    		{
    			MethodInfo methodInfo = AccessTools.PropertySetter(typeof(ControllerBaseComp<FacialAnimation.HeadTypeDef, HeadShapeDef>), "FaceType");
    			MethodInfo method = typeof(Patch_FaceTypeSetter).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
    			if (methodInfo == null || method == null)
    			{
    				Log.Error("[FA Heads] Failed to patch: Could not locate setter or postfix.");
    			}
    			else
    			{
    				harmony.Patch(methodInfo, null, new HarmonyMethod(method));
    			}
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Error patching FaceType setter: " + ex);
    		}
    	}

    	private static void Postfix(FacialAnimation.HeadTypeDef value, ControllerBaseComp<FacialAnimation.HeadTypeDef, HeadShapeDef> __instance)
    	{
    		if (__instance == null || value == null || !(__instance is HeadControllerComp root))
    		{
    			return;
    		}
    		Pawn value2 = Traverse.Create(root).Field("pawn").GetValue<Pawn>();
    		if (value2 == null)
    		{
    			return;
    		}
    		FacialAnimation.HeadTypeDef headTypeDef = ManualHeadOverrides.GetOverride(value2);
    		if (Prefs.DevMode)
    		{
    			Log.Message("[FA Heads] FaceType set to " + value.defName + " for " + value2.LabelShortCap + ". Existing override: " + (headTypeDef?.defName ?? "none"));
    		}
    		if (headTypeDef != value)
    		{
    			FacialAnimationUtil.ApplyManualHeadOverride(value2, value);
    			if (Prefs.DevMode)
    			{
    				Log.Message("[FA Heads] Saved " + value.defName + " as manual override for " + value2.LabelShortCap);
    			}
    		}
    	}
    }
}
