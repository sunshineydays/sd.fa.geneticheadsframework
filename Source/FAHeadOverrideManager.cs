using System.Collections.Generic;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public class FAHeadOverrideManager : GameComponent
    {
        // dictionary that stores override assignments by unique pawn ID
        private static Dictionary<string, string> overrideLookup = new Dictionary<string, string>();

        public FAHeadOverrideManager(Game game) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref overrideLookup, "FA_HeadOverrides", LookMode.Value, LookMode.Value, ref keysWorkingList, ref valuesWorkingList);
        }

        private List<string> keysWorkingList;
        private List<string> valuesWorkingList;

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
            if (overrideLookup.TryGetValue(pawn.ThingID, out var defName))
            {
                var head = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamedSilentFail(defName);
                if (head != null)
                {
                    ManualHeadOverrides.SetOverride(pawn, head);
                }
            }
        }
    }

}