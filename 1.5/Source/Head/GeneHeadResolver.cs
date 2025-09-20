using System;
using System.Collections.Generic;
using System.Linq;
using FacialAnimation;
using Verse;

namespace FacialAnimationGeneticHeads
{
    [StaticConstructorOnStartup]
    public static class GeneHeadResolver
    {
    	private class GeneSetComparer : IEqualityComparer<HashSet<GeneDef>>
    	{
    		public bool Equals(HashSet<GeneDef> x, HashSet<GeneDef> y)
    		{
    			return x.SetEquals(y);
    		}

    		public int GetHashCode(HashSet<GeneDef> obj)
    		{
    			int num = 17;
    			foreach (GeneDef item in obj.OrderBy((GeneDef g) => g.shortHash))
    			{
    				num = num * 31 + item.shortHash;
    			}
    			return num;
    		}
    	}

    	public static List<GeneHeadRule> GeneHeadMap;

    	private static Dictionary<HashSet<GeneDef>, FacialAnimation.HeadTypeDef> headCache;

    	static GeneHeadResolver()
    	{
    		GeneHeadMap = new List<GeneHeadRule>();
    		headCache = new Dictionary<HashSet<GeneDef>, FacialAnimation.HeadTypeDef>(new GeneSetComparer());
    		try
    		{
    			foreach (FacialAnimation.HeadTypeDef allDef in DefDatabase<FacialAnimation.HeadTypeDef>.AllDefs)
    			{
    				FARequiredGenes modExtension = allDef.GetModExtension<FARequiredGenes>();
    				if (modExtension == null || modExtension.requiredGenes == null)
    				{
    					continue;
    				}
    				HashSet<GeneDef> hashSet = new HashSet<GeneDef>();
    				foreach (string requiredGene in modExtension.requiredGenes)
    				{
    					GeneDef namedSilentFail = DefDatabase<GeneDef>.GetNamedSilentFail(requiredGene);
    					if (namedSilentFail != null)
    					{
    						hashSet.Add(namedSilentFail);
    					}
    				}
    				if (hashSet.Count == 0)
    				{
    					continue;
    				}
    				bool flag = false;
    				foreach (GeneHeadRule item in GeneHeadMap)
    				{
    					if (item.requiredGenes.SetEquals(hashSet))
    					{
    						item.headDefs.Add(allDef);
    						flag = true;
    						break;
    					}
    				}
    				if (!flag)
    				{
    					GeneHeadMap.Add(new GeneHeadRule(hashSet, allDef));
    				}
    			}
    			GeneHeadMap.Sort((GeneHeadRule a, GeneHeadRule b) => b.requiredGenes.Count.CompareTo(a.requiredGenes.Count));
    			Log.Message("[FA Heads] Loaded " + GeneHeadMap.Count + " gene-head definition groups.");
    		}
    		catch (Exception ex)
    		{
    			Log.Error("[FA Heads] Error loading gene map: " + ex);
    		}
    	}

    	public static FacialAnimation.HeadTypeDef Match(Pawn pawn)
    	{
    		if (pawn == null || pawn.genes == null)
    		{
    			return null;
    		}
    		HashSet<GeneDef> hashSet = new HashSet<GeneDef>(from g in pawn.genes.GenesListForReading
    			where g.Active
    			select g.def);
    		foreach (HashSet<GeneDef> key in headCache.Keys)
    		{
    			if (key.SetEquals(hashSet))
    			{
    				return headCache[key];
    			}
    		}
    		foreach (GeneHeadRule item in GeneHeadMap)
    		{
    			if (item.requiredGenes.IsSubsetOf(hashSet))
    			{
    				if (item.headDefs.Count == 1)
    				{
    					FacialAnimation.HeadTypeDef headTypeDef = item.headDefs[0];
    					headCache[hashSet] = headTypeDef;
    					return headTypeDef;
    				}
    				Rand.PushState();
    				int index;
    				try
    				{
    					Rand.Seed = pawn.thingIDNumber;
    					index = Rand.Range(0, item.headDefs.Count);
    				}
    				finally
    				{
    					Rand.PopState();
    				}
    				return item.headDefs[index];
    			}
    		}
    		headCache[hashSet] = null;
    		return null;
    	}
    }
}
