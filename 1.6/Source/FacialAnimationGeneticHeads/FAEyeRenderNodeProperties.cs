using System.Collections.Generic;
using FacialAnimation;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialAnimationGeneticHeads;

public class FAEyeRenderNodeExtension : DefModExtension
{
	public List<PawnRenderNodeProperties> renderNodeProperties;

	public bool randomizeMatchingEyeColors;

	public override void ResolveReferences(Def parentDef)
	{
		base.ResolveReferences(parentDef);
		if (renderNodeProperties == null)
		{
			return;
		}
		foreach (PawnRenderNodeProperties properties in renderNodeProperties)
		{
			properties?.ResolveReferencesRecursive();
		}
	}
}

public class FAEyeRenderNodeProperties : PawnRenderNodeProperties_Eye
{
	public float maleForwardOffset;

	public FAEyeRenderNodeProperties()
	{
		workerClass = typeof(FAEyeRenderNodeWorker);
		nodeClass = typeof(FAEyeRenderNode);
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		workerClass = typeof(FAEyeRenderNodeWorker);
		nodeClass = typeof(FAEyeRenderNode);
	}
}

public class FAEyeRenderNode : NLFacialAnimationPartNode
{
	public FAEyeRenderNode(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		Graphic graphic = base.GraphicFor(pawn);
		if (controller is LidControllerComp lids
			&& layerType == NLFacialAnimationLayerType.Cover
			&& graphic is Graphic_RenderTexture
			&& !graphic.maskPath.NullOrEmpty())
		{
			Shader shader = ShaderDatabase.LoadShader(GraphicHelper.GetSkinShader(pawn, lids.FaceType));
			return GraphicDatabase.Get<Graphic_Multi>(graphic.maskPath, shader, Vector2.one, Color.white, Color.white);
		}
		return graphic;
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		return Props.overrideMeshSize.HasValue
			? base.MeshSetFor(pawn)
			: HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
	}
}

public class FAEyeRenderNodeWorker : NLFacialAnimationPartNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		parms.facing = ((NLFacialAnimationPartNode)node).overrideHeadRot;
		return base.CanDrawNow(node, parms);
	}

	public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
	{
		return node.Props.baseLayer + node.debugLayerOffset;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		parms.facing = ((NLFacialAnimationPartNode)node).overrideHeadRot;
		Vector3 result = base.OffsetFor(node, parms, out pivot);
		if (!UsesUnisexCoordinates(node)
			&& parms.pawn.gender == Gender.Male
			&& parms.facing.IsHorizontal
			&& node.Props is FAEyeRenderNodeProperties properties)
		{
			float direction = parms.facing == Rot4.East ? 1f : -1f;
			result.x += direction * properties.maleForwardOffset;
		}
		if (TryGetEyeAnchor(node.Props.anchorTag, parms, out BodyTypeDef.WoundAnchor anchor))
		{
			// This also resolves HeadTypeDef.eyeOffsetEastWest and allows
			// head-specific CalcAnchorData patches (such as SWX's south offsets)
			// to adjust the same anchor used by vanilla eye render nodes.
			PawnDrawUtility.CalcAnchorData(parms.pawn, anchor, parms.facing, out Vector3 anchorOffset, out _);
			result += anchorOffset;
		}
		return result;
	}

	private static bool UsesUnisexCoordinates(PawnRenderNode node)
	{
		return ((NLFacialAnimationPartNode)node).controller switch
		{
			EyeballControllerComp eyeballs => eyeballs.FaceType?.enableUnisexTexPath == true,
			LidControllerComp lids => lids.FaceType?.enableUnisexTexPath == true,
			_ => false
		};
	}

	public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
	{
		float eyeSizeFactor = parms.pawn.ageTracker?.CurLifeStage?.eyeSizeFactor ?? 1f;
		return base.ScaleFor(node, parms) * eyeSizeFactor;
	}

	private static bool TryGetEyeAnchor(string anchorTag, PawnDrawParms parms, out BodyTypeDef.WoundAnchor anchor)
	{
		anchor = null;
		if (anchorTag.NullOrEmpty() || parms.pawn?.story?.bodyType?.woundAnchors == null)
		{
			return false;
		}
		foreach (BodyTypeDef.WoundAnchor candidate in parms.pawn.story.bodyType.woundAnchors)
		{
			if (candidate.tag == anchorTag && PawnDrawUtility.AnchorUsable(parms.pawn, candidate, parms.facing))
			{
				anchor = candidate;
				return true;
			}
		}
		return false;
	}
}
