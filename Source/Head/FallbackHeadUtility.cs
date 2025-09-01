using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public static class FallbackHeadUtility
    {
    	private static readonly Dictionary<Pawn, FacialAnimation.HeadTypeDef> fallbackCache = new Dictionary<Pawn, FacialAnimation.HeadTypeDef>();

    	public static FacialAnimation.HeadTypeDef GetFallbackHead(Pawn pawn)
    	{
    		if (pawn == null || pawn.def == null)
    		{
    			return null;
    		}
    		if (fallbackCache.TryGetValue(pawn, out var value))
    		{
    			return value;
    		}
    		string raceName = pawn.def.defName;
    		Gender gender = pawn.gender;
    		List<FacialAnimation.HeadTypeDef> list = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs.Where((FacialAnimation.HeadTypeDef h) => h.raceName == raceName && (h.gender == gender || h.gender == Gender.None)).ToList();
    		if (list.Count == 0)
    		{
    			list = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs.Where((FacialAnimation.HeadTypeDef h) => h.gender == gender || h.gender == Gender.None).ToList();
    		}
    		if (list.Count == 0)
    		{
    			Log.Warning($"[FA Heads] No suitable fallback HeadTypeDef found for pawn {pawn.NameShortColored}.");
    			FacialAnimation.HeadTypeDef headTypeDef = DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs.FirstOrDefault();
    			fallbackCache[pawn] = headTypeDef;
    			return headTypeDef;
    		}
    		float num = list.Sum((FacialAnimation.HeadTypeDef h) => h.probability);
    		Rand.PushState();
    		float num2;
    		try
    		{
    			Rand.Seed = pawn.thingIDNumber;
    			num2 = Rand.Value * num;
    		}
    		finally
    		{
    			Rand.PopState();
    		}
    		foreach (FacialAnimation.HeadTypeDef item in list)
    		{
    			if (num2 < item.probability)
    			{
    				fallbackCache[pawn] = item;
    				return item;
    			}
    			num2 -= item.probability;
    		}
    		fallbackCache[pawn] = list.First();
    		return list.First();
    	}
    }
}
