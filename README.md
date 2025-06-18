[h1]Genetic Heads Framework for Facial Animation[/h1]
[i]Seamlessly match your pawns’ faces to their genes!
Automatic storytelling with total override control.[/i]

[h2]What This Mod Does[/h2]
This mod connects Biotech genes to Facial Animation head types. If you have the head textures with the right code in your mods, this mod will automatically apply the appropriate head to your pawns.

[h2]Features[/h2]
[list]
[*] Automatic face assignment based on Biotech genes  
[*] Supports multiple required genes per head (furskin and heavy jaw, gaunt and furskin, etc)
[*] Fallback system assigns race/gender-appropriate heads if no genetic match  
[*] Supports multiple head textures for a single gene (like base game Biotech furskin's three textures)
[*] Gizmos for dev mode control–assign or clear any pawn’s head regardless of genes  
[*] Lightweight, extensible design–easily add your own head packs  
[/list]

[h2]Seamless Visual Storytelling[/h2]
This mod provides [i]immersion[/i] and a base game-like experience; a child inherits gaunt features from a Waster parent, a Yttakin tribe has slight variation, all without micromanagement. 

Check out my [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3501317537]Biotech Head Pack[/url] for vanilla Biotech heads edited to work with Facial Animation. Pairs very nicely with [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2816938779]Vanilla Textures Expanded[/url].

Also check out my [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3501317734]Mod Head Pack[/url] for some heads from mods, also in vanilla style (VRE, Roo's).

[h2]Modder-Friendly[/h2]
Make your own head packs! Make sure it's Facial Animation appropriate (no eyes, same folder system as Facial Animation, etc) and then add Defs with this XML:
[code]
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
[/code]

[h2]Plans[/h2]
[list]
[*] 1.6 compatibility
[*] more mods supported (see mod head pack for these)
[*] fancier screenshots & such
[/list]

[h3]A Note from Me[/h3]
I'm open to feedback! I've tested this mod on my own game of 731 mods (not even joking about the number XD) and I didn't notice any slowdown or performance issues beyond what I'm used to in a heavily modded game.

And just a heads up, this is my first C# mod, so I'm almost entirely sure I've done something that isn't as efficient as it could be. If anyone who knows their way around the language wants to check it out, I've posted the framework code on GitHub. I'm happy to update with improvements and credit appropriately.
