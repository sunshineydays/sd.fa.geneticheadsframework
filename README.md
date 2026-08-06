# Genetic Heads Framework for [NL] Facial Animation

A framework mod for RimWorld that helps [NL] Facial Animation part selection!

Despite the name, which I won't be changing because that was enough of a disaster the first time, this mod now supports more than gene matching. You can now link parts to genes, hediffs, traits, and creepjoiner forms!

## What This Framework Does

- Extends Facial Animation part selection for `HeadTypeDef`, `EyeballTypeDef`, `BrowTypeDef`, `LidTypeDef`, `MouthTypeDef`, and `SkinTypeDef`
- Adds all-required (`AND`) matching for `targetGeneDefs` on head, brow, lid, mouth, and skin defs
- Leaves `EyeballTypeDef` gene matching and all `EyeballColorDef` handling to Facial Animation
- Adds extra conditional matching through:
  - `FacialAnimationGeneticHeads.FARequiredHediffs`
  - `FacialAnimationGeneticHeads.FARequiredTraits`
  - `FacialAnimationGeneticHeads.FARequiredCreepjoinerForms`
- Refreshes affected facial parts when genes, hediffs, or traits change
- Refreshes creepjoiner pawns after generation so form-specific parts can apply immediately
- Applies hediff skin shaders to Facial Animation parts that use standard skin shaders
- Includes mod settings so each facial component patch can be enabled or disabled separately

## How Matching Works

- A non-eyeball face-part def can be gene-only, condition-only, or a mix of both
- For head, brow, lid, mouth, and skin defs, `targetGeneDefs`, `requiredHediffs`, and `requiredTraits` are AND checks
- Facial Animation's native any-match gene behavior is used for `EyeballTypeDef`; condition-only eyeball defs can still use this framework
- `requiredCreepjoinerForms` is a list of allowed forms; matching any listed form is enough
- If multiple conditional defs match, it prefers:
  1. the def with more non-gene conditions
  2. the def with more required genes
  3. weighted random selection using `probability`
- If no conditional def matches, it falls back to this framework's all-required gene matching and then regular FA selection

## Player Notes

- The mod settings menu lets you toggle the Brow, Eyeball, Head, Lid, Mouth, and Skin patches independently
- The Eyeball setting controls only condition-based eyeball type selection; it does not patch gene matching or eye color
- This mod is mainly intended as a dependency for head packs and other FA-compatible part packs

## Modding Guide

### Examples

Use Facial Animation's `targetGeneDefs` for one or more gene requirements on head, brow, lid, mouth, and skin defs, then add this framework's mod extensions for extra conditions. For eyeball defs, use the extensions only on defs without `targetGeneDefs`.

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

`EyeballColorDef` is entirely owned by Facial Animation. Use its native `geneDef` and eye-linked `hediffDef` fields for eye colors.

The conditional extension pattern works for all face type defs. Do not combine `targetGeneDefs` with framework conditions on `EyeballTypeDef`, because eyeball genes are intentionally left to Facial Animation.

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
