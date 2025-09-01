using System.Collections.Generic;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAHeadOverrideManager : GameComponent
    {
    	private static Dictionary<string, string> overrideLookup = new Dictionary<string, string>();

    	private List<string> keysWorkingList;

    	private List<string> valuesWorkingList;

    	public FAHeadOverrideManager(Game game)
    	{
    	}

    	public override void ExposeData()
    	{
    		Scribe_Collections.Look(ref overrideLookup, "FA_HeadOverrides", LookMode.Value, LookMode.Value, ref keysWorkingList, ref valuesWorkingList);
    	}

    	public static void SetOverride(Pawn pawn, FacialAnimation.HeadTypeDef head)
    	{
    		ManualHeadOverrides.SetOverride(pawn, head);
    		overrideLookup[pawn.ThingID] = head.defName;
    	}

    	public static void ClearOverride(Pawn pawn)
    	{
    		ManualHeadOverrides.ClearOverride(pawn);
    		overrideLookup.Remove(pawn.ThingID);
    	}

    	public static void RestoreOverrideIfAvailable(Pawn pawn)
    	{
    		if (overrideLookup.TryGetValue(pawn.ThingID, out var value))
    		{
    			FacialAnimation.HeadTypeDef namedSilentFail = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamedSilentFail(value);
    			if (namedSilentFail != null)
    			{
    				ManualHeadOverrides.SetOverride(pawn, namedSilentFail);
    			}
    		}
    	}
    }
}
