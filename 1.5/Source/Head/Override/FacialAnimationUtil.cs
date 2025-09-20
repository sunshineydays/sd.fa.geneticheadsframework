using FacialAnimation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public static class FacialAnimationUtil
    {
    	public static void ApplyManualHeadOverride(Pawn pawn, FacialAnimation.HeadTypeDef head)
    	{
    		if (pawn == null || head == null)
    		{
    			return;
    		}
    		HeadControllerComp comp = pawn.GetComp<HeadControllerComp>();
    		if (comp != null)
    		{
    			FAHeadOverrideManager.SetOverride(pawn, head);
    			Traverse.Create(comp).Field("faceType").SetValue(head);
    			comp.ReloadIfNeed();
    			PortraitsCache.SetDirty(pawn);
    			pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    			if (Prefs.DevMode)
    			{
    				Log.Message("[FA Heads] Manually applied " + head.defName + " to " + pawn.LabelShortCap);
    			}
    		}
    	}
    }
}
