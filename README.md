# Genetic Heads Framework for Facial Animation
*Seamlessly match your pawns’ faces to their genes!
Automatic storytelling with total override control.*

## What This Mod Does
This mod connects Biotech genes to Facial Animation head types. If you have the head textures with the right code in your mods, this mod will automatically apply the appropriate head to your pawns.

## Features
- Automatic face assignment based on Biotech genes  
- Supports multiple required genes per head (furskin and heavy jaw, gaunt and furskin, etc)
- Fallback system assigns race/gender-appropriate heads if no genetic match  
- Supports multiple head textures for a single gene (like base game Biotech furskin's three textures)
- Gizmos for dev mode control–assign or clear any pawn’s head regardless of genes  
- Lightweight, extensible design–easily add your own head packs  

## Seamless Visual Storytelling
This mod provides *immersion* and a base game-like experience; a child inherits gaunt features from a Waster parent, a Yttakin tribe has slight variation, all without micromanagement. 

Check out my [Biotech Head Pack](https://steamcommunity.com/sharedfiles/filedetails/?id=3501317537) for vanilla Biotech heads edited to work with Facial Animation. Pairs very nicely with [Vanilla Textures Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=2816938779).

Also check out my [Mod Head Pack](https://steamcommunity.com/sharedfiles/filedetails/?id=3501317734) for some heads from mods, also in vanilla style (VRE, Roo's).

## Modder-Friendly
Make your own head packs! Make sure it's Facial Animation appropriate (no eyes, same folder system as Facial Animation, etc) and then add Defs with this XML:
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

## Plans
- more mods supported (see mod head pack for these)
- fancier screenshots & such

### A Note from Me
I'm open to feedback! I've tested this mod on my own game of 731 mods (not even joking about the number XD) and I didn't notice any slowdown or performance issues beyond what I'm used to in a heavily modded game.

And just a heads up, this is my first C# mod, so I'm almost entirely sure I've done something that isn't as efficient as it could be. If anyone who knows their way around the language wants to check it out, look at the Source folder here. I'm happy to update with improvements and credit appropriately.
