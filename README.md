# Genetic Heads Framework for Facial Animation
*Support for Facial Animation (FA) genetic head selection!*

## What This Mod Does
In 1.6, this mod adds logic to FA to correctly assign multi-gene heads, allow variation in heads like furskin, and adds eye color for vanilla Biotech. 

In 1.5, this mod connects genes to FA parts. If you have the textures with the right code in your mods, this mod will automatically apply the appropriate parts to your pawns.

## Features
- Automatic face assignment based on Biotech genes  
- Supports multiple genes per head (furskin and heavy jaw, gaunt and furskin, etc)
- Fallback system assigns race/gender-appropriate heads if no genetic match  
- Supports multiple head textures for a single gene (like base game Biotech furskin's three textures)
- Extensible design to easily add your own head packs

**Known Issues**
- Weird behavior with HAR
- Default FA right eye color not working. Not something I know how to fix, sorry. Using EyeGenes2 bypasses the problem

Check out my [Biotech Head Pack](https://steamcommunity.com/sharedfiles/filedetails/?id=3501317537) for vanilla Biotech heads edited to work with Facial Animation. Pairs very nicely with [Vanilla Textures Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=2816938779).

Also check out my [Mod Head Pack](https://steamcommunity.com/sharedfiles/filedetails/?id=3501317734) for some heads from mods, also in vanilla style (VRE, Roo's).

## Modder-Friendly
Make your own head packs or patch existing Facial Animation heads! Make sure art is FA appropriate (no eyes, same folder system as Facial Animation, etc) and then add Defs with gene logic. In 1.6, use FA's native targetGeneDefs. In 1.5, use:
```
<FacialAnimation.HeadTypeDef>
    <defName>HeadType_ExampleHead</defName>
    <texPath>ExampleTexture</texPath>
    <probability>0</probability>
    <shader>Map/CutoutSkin</shader>
    <shaderColorOverride>Map/CutoutSkinOverride</shaderColorOverride>
    <modExtensions>
      <li Class="FacialAnimationGeneticHeads.FARequiredGenes">
        <requiredGenes>
          <li>Example_Gene1</li>
          <li>Example_Gene2</li>
        </requiredGenes>
      </li>
    </modExtensions>
</FacialAnimation.HeadTypeDef>
```
Also be sure to add this mod as a dependency!
```
<modDependencies>
  <li>
    <packageId>sd.fa.geneticheadsframework</packageId>
    <displayName>Genetic Heads Framework for [NL] Facial Animation</displayName>
    <steamWorkshopUrl>https://steamcommunity.com/sharedfiles/filedetails/?id=3498759997</steamWorkshopUrl>
  </li>
</modDependencies>
```

### A Note from Me
I’ve been working on this mod for a long time, even started learning c# so I could make it. If I'm not active in the community and it needs to be updated somehow, feel free to do so. When I come back, I'll integrate it into the main mod! I'm happy to update with improvements and credit appropriately.

Have fun!
