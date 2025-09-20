// patch: updates face parts (head, eyes, brows, lids, mouth, skin) after gene changes
// keeps current if still valid and specific, otherwise upgrades or downgrades

using System.Linq;
using FacialAnimation; // FaceTypeDef & concrete *TypeDef classes
using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace FacialAnimationGeneticHeads
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class Patch_NotifyGenesChanged_AllFacialParts
    {
        static void Postfix(Pawn_GeneTracker __instance)
        {
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null || !pawn.Spawned) return;

            // one call per facial controller, simple and safe
            DoPart<FacialAnimation.HeadTypeDef>(pawn, "FacialAnimation.HeadControllerComp", "Head");
            DoPart<EyeballTypeDef>(pawn, "FacialAnimation.EyeballControllerComp", "Eye");
            DoPart<BrowTypeDef>(pawn, "FacialAnimation.BrowControllerComp", "Brow");
            DoPart<LidTypeDef>(pawn, "FacialAnimation.LidControllerComp", "Lid");
            DoPart<MouthTypeDef>(pawn, "FacialAnimation.MouthControllerComp", "Mouth");
            DoPart<SkinTypeDef>(pawn, "FacialAnimation.SkinControllerComp", "Skin");
        }

        // handles one facial part (like eyes or mouth)
        static void DoPart<TDef>(Pawn pawn, string compTypeName, string logLabel)
            where TDef : FaceTypeDef, new()
        {
            if (pawn == null)
                return;

            try
            {
                // get the comp by name since we can't use types directly
                var comp = pawn.AllComps?.FirstOrDefault(c => c.GetType().FullName == compTypeName);
                if (comp == null) return;

                // get the field holding the TDef-type faceType
                var fi = AccessTools.Field(comp.GetType(), "faceType");
                if (fi == null || fi.FieldType != typeof(TDef)) return;

                var current = fi.GetValue(comp) as TDef;

                // pick the best matching def for current genes (filters race/gender)
                var best = GeneFacePatchHelper.GetGeneMatchedDef<TDef>(pawn, pawn.gender);
                if (best == null) return;

                bool currentValid = IsValidForGenes(pawn, current);
                bool bestHasReq = HasRequiredGenes(best);
                bool currHasReq = HasRequiredGenes(current);
                int bestCount = RequiredCount(best);
                int currCount = RequiredCount(current);

                // upgrade if best has gene reqs and is more specific than current
                bool shouldUpgrade = bestHasReq && (!currHasReq || bestCount > currCount);

                // replace if current is invalid, should upgrade, or null
                bool shouldReplace = !currentValid || shouldUpgrade || current == null;

                if (shouldReplace && !ReferenceEquals(current, best))
                {
                    fi.SetValue(comp, best);

                    // try SetDirty(); fallback to InitializeIfNeed()
                    var setDirty = AccessTools.Method(comp.GetType(), "SetDirty");
                    if (setDirty != null) setDirty.Invoke(comp, null);
                    else AccessTools.Method(comp.GetType(), "InitializeIfNeed")?.Invoke(comp, null);

                    if (Prefs.DevMode)
                        Log.Message($"[FA GenePatch] {logLabel}: genes changed -> {pawn.LabelShortCap}: {current?.defName ?? "<null>"} → {best.defName}");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[FA GenePatch] Failed to patch {logLabel} comp on {pawn?.LabelShortCap ?? "null pawn"}: {ex}");
            }
        }

        // === shared helpers ===

        // true if def has any targetGeneDefs
        static bool HasRequiredGenes(FaceTypeDef def)
        {
            return def?.targetGeneDefs != null && def.targetGeneDefs.Count > 0;
        }

        // count of required genes
        static int RequiredCount(FaceTypeDef def)
        {
            return def?.targetGeneDefs?.Count ?? 0;
        }

        // true if pawn has all genes required by def
        static bool IsValidForGenes(Pawn pawn, FaceTypeDef def)
        {
            if (def == null) return false;
            if (def.targetGeneDefs == null || def.targetGeneDefs.Count == 0) return true;
            if (pawn.genes == null) return false;
            var active = pawn.genes.GenesListForReading.Select(g => g.def.defName).ToHashSet();
            return def.targetGeneDefs.All(active.Contains);
        }
    }
}
