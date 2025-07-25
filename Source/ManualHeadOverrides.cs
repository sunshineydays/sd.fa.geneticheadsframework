using System.Collections.Generic;
using Verse;

namespace FacialAnimationGeneticHeads
{
    public static class ManualHeadOverrides
    {
        private static readonly Dictionary<Pawn, FacialAnimation.HeadTypeDef> manualOverrides = new Dictionary<Pawn, FacialAnimation.HeadTypeDef>();

        public static void SetOverride(Pawn pawn, FacialAnimation.HeadTypeDef head)
        {
            if (pawn != null && head != null)
                manualOverrides[pawn] = head;
        }

        public static void ClearOverride(Pawn pawn)
        {
            if (pawn != null)
                manualOverrides.Remove(pawn);
        }

        public static FacialAnimation.HeadTypeDef GetOverride(Pawn pawn)
        {
            if (pawn != null && manualOverrides.TryGetValue(pawn, out var head))
                return head;
            return null;
        }

        public static bool HasOverride(Pawn pawn) => pawn != null && manualOverrides.ContainsKey(pawn);
    }
}