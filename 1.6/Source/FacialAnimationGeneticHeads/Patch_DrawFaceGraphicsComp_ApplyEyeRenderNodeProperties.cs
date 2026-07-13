using System;
using System.Collections.Generic;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace FacialAnimationGeneticHeads;

[HarmonyPatch]
public static class Patch_DrawFaceGraphicsComp_ApplyEyeRenderNodeProperties
{
	private static System.Reflection.MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("FacialAnimation.DrawFaceGraphicsComp"), "CompRenderNodes");
	}

	private static void Postfix(ref List<PawnRenderNode> __result)
	{
		if (__result == null)
		{
			return;
		}
		try
		{
			PlaceEyesAndLidsAboveTattoos(__result);
			List<PawnRenderNode> replacement = new List<PawnRenderNode>(__result.Count);
			foreach (PawnRenderNode renderNode in __result)
			{
				if (renderNode is not NLFacialAnimationPartNode node)
				{
					replacement.Add(renderNode);
					continue;
				}
				Def faceType = GetEyeOrLidDef(node.controller);
				List<PawnRenderNodeProperties> sources = faceType?.GetModExtension<FAEyeRenderNodeExtension>()?.renderNodeProperties;
				if (sources.NullOrEmpty())
				{
					replacement.Add(renderNode);
					continue;
				}
				foreach (PawnRenderNodeProperties source in sources)
				{
					if (source != null && AppliesToLayer(source.side, node.layerType))
					{
						replacement.Add(CreateReplacement(node, source));
					}
				}
			}
			__result = replacement;
		}
		catch (Exception exception)
		{
			Log.Warning($"[FA Genetic Heads] Failed to apply FA eye render-node properties: {exception}");
		}
	}

	private static void PlaceEyesAndLidsAboveTattoos(List<PawnRenderNode> nodes)
	{
		foreach (PawnRenderNode renderNode in nodes)
		{
			if (renderNode is not NLFacialAnimationPartNode node)
			{
				continue;
			}
			node.Props.baseLayer = node.controller switch
			{
				LidControllerComp when node.layerType == NLFacialAnimationLayerType.Bottom => 53f,
				EyeballControllerComp when node.layerType is NLFacialAnimationLayerType.Basic
					or NLFacialAnimationLayerType.L_Basic
					or NLFacialAnimationLayerType.R_Basic => 54f,
				EyeballControllerComp => 54.25f,
				LidControllerComp => 54.75f,
				_ => node.Props.baseLayer
			};
		}
	}

	private static Def GetEyeOrLidDef(IFacialAnimationController controller)
	{
		return controller switch
		{
			EyeballControllerComp eyeballs => eyeballs.FaceType,
			LidControllerComp lids => lids.FaceType,
			_ => null
		};
	}

	private static bool AppliesToLayer(PawnRenderNodeProperties.Side side, NLFacialAnimationLayerType layerType)
	{
		return layerType switch
		{
			NLFacialAnimationLayerType.L_Basic or NLFacialAnimationLayerType.L_Option => side == PawnRenderNodeProperties.Side.Left,
			NLFacialAnimationLayerType.R_Basic or NLFacialAnimationLayerType.R_Option => side == PawnRenderNodeProperties.Side.Right,
			_ => true
		};
	}

	private static PawnRenderNode CreateReplacement(NLFacialAnimationPartNode original, PawnRenderNodeProperties source)
	{
		FAEyeRenderNodeProperties eyeSource = source as FAEyeRenderNodeProperties;
		FAEyeRenderNodeProperties properties = new FAEyeRenderNodeProperties
		{
			debugLabel = original.Props.debugLabel + "_" + source.side,
			baseLayer = original.Props.baseLayer,
			parentTagDef = original.Props.parentTagDef,
			drawSize = source.drawSize,
			drawData = source.drawData,
			rotDrawMode = source.rotDrawMode,
			visibleFacing = source.visibleFacing,
			skipFlag = source.skipFlag,
			anchorTag = source.anchorTag,
			side = source.side,
			flipGraphic = source.flipGraphic,
			oppositeFacingLayerWhenFlipped = source.oppositeFacingLayerWhenFlipped,
			narrowCrownHorizontalOffset = source.narrowCrownHorizontalOffset,
			rotateIndependently = source.rotateIndependently,
			subworkerClasses = source.subworkerClasses,
			overrideMeshSize = source.overrideMeshSize,
			maleForwardOffset = eyeSource?.maleForwardOffset ?? 0.017f
		};
		FAEyeRenderNode replacement = new FAEyeRenderNode(original.tree.pawn, properties, original.tree)
		{
			controller = original.controller,
			layerType = original.layerType,
			rotDrawMode = original.rotDrawMode,
			overrideHeadRot = original.overrideHeadRot
		};
		return replacement;
	}
}
