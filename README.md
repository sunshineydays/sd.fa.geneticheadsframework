# Genetic Heads Framework for [NL] Facial Animation

A framework mod for RimWorld that helps [NL] Facial Animation part selection!

Despite the name, which I won't be changing because that was enough of a disaster the first time, this mod now supports more than gene matching. You can now link parts to genes, hediffs, traits, and creepjoiner forms!

## What This Framework Does

- Extends Facial Animation part selection for `HeadTypeDef`, `EyeballTypeDef`, `BrowTypeDef`, `LidTypeDef`, `MouthTypeDef`, and `SkinTypeDef`
- Uses Facial Animation's built-in `targetGeneDefs` field for gene matching
- Adds extra conditional matching through:
  - `FacialAnimationGeneticHeads.FARequiredHediffs`
  - `FacialAnimationGeneticHeads.FARequiredTraits`
  - `FacialAnimationGeneticHeads.FARequiredCreepjoinerForms`
- Refreshes affected facial parts when genes, hediffs, or traits change
- Refreshes creepjoiner pawns after generation so form-specific parts can apply immediately
- Supports conditional eyeball color overrides based on genes, hediffs, and traits
- Includes mod settings so each facial component patch can be enabled or disabled separately

## How Matching Works

- A face-part def can be gene-only, condition-only, or a mix of both
- `targetGeneDefs`, `requiredHediffs`, and `requiredTraits` are AND checks (as opposed to FA's native ANY for `targetGeneDefs`)
- `requiredCreepjoinerForms` is a list of allowed forms; matching any listed form is enough
- If multiple conditional defs match, it prefers:
  1. the def with more non-gene conditions
  2. the def with more required genes
  3. weighted random selection using `probability`
- If no conditional def matches, it falls back to normal gene matching and then regular FA selection

## Player Notes

- The mod settings menu lets you toggle the Brow, Eyeball, Head, Lid, Mouth, and Skin patches independently
- If another mod handles eye rendering and you see eye color conflicts, disable the Eyeball patch first
- This mod is mainly intended as a dependency for head packs and other FA-compatible part packs

## Modding Guide

### Examples

Use Facial Animation's `targetGeneDefs` for one or more gene requirements, then add this framework's mod extensions for the extra conditions you want. Here's examples:

```xml
	<FacialAnimation.HeadTypeDef>
		<defName>FurCoveredGaunt</defName>
		<texPath>FurCoveredGaunt</texPath>
		<probability>1</probability>
		<shader>Map/CutoutSkin</shader>
		<shaderColorOverride>Map/CutoutSkinOverride</shaderColorOverride>
		<targetGeneDefs>
					<li>Furskin</li>
					<li>Head_Gaunt</li>
				</targetGeneDefs>
	</FacialAnimation.HeadTypeDef>

  <FacialAnimation.BrowTypeDef>
    <defName>BrowDarkScholar</defName>
    <texPath>Blank</texPath>
    <shader>Map/Transparent</shader>
    <probability>0</probability>
    <modExtensions>
      <li Class="FacialAnimationGeneticHeads.FARequiredCreepjoinerForms">
        <requiredCreepjoinerForms>
          <li>DarkScholar</li>
        </requiredCreepjoinerForms>
      </li>    
    </modExtensions>
  </FacialAnimation.BrowTypeDef>

    <FacialAnimation.EyeballColorDef>
    <defName>ColorBodyMastery</defName>
    <modExtensions>
      <li Class="FacialAnimationGeneticHeads.FARequiredTraits">
        <requiredTraits>
          <li>BodyMastery</li>
        </requiredTraits>
      </li>    
    </modExtensions>
    <eyeballColor>RGB(255,255,255)</eyeballColor>
  </FacialAnimation.EyeballColorDef>

  <FacialAnimation.HeadTypeDef>
    <defName>HeadGhoulNormal</defName>
    <texPath>Heads/GhoulNormal</texPath>
    <shader>Map/CutoutSkin</shader>
    <shaderColorOverride>Map/CutoutSkinOverride</shaderColorOverride>
    <probability>0</probability>
    <modExtensions>
      <li Class="FacialAnimationGeneticHeads.FARequiredHediffs">
        <requiredHediffs>
          <li>Ghoul</li>
        </requiredHediffs>
      </li>    
    </modExtensions>
  </FacialAnimation.HeadTypeDef>

```

The same pattern works for all Type Defs!

## Dependency Cut + Paste

```xml
<modDependencies>
  <li>
    <packageId>sd.fa.geneticheadsframework</packageId>
    <displayName>Genetic Heads Framework for [NL] Facial Animation</displayName>
    <steamWorkshopUrl>https://steamcommunity.com/sharedfiles/filedetails/?id=3498759997</steamWorkshopUrl>
  </li>
</modDependencies>
```

## Game Version Notes

- `1.5` content is still included in as a legacy implementation
- The old `FARequiredGenes` examples in `1.5` are legacy-only; the 1.6 framework uses `targetGeneDefs` instead
