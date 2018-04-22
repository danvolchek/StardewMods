# Stack Everything


See [This link](http://www.nexusmods.com/stardewvalley/mods/2053?) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

This mod uses [Harmony](https://github.com/pardeike/Harmony) to:
 - replace the `StardewValley.Object.maximumStackSize` method to always return 999
 - postfix `StardewValley.Object.drawInMenu` to correctly draw the stack number.
 - replace `StardewValley.Object.getStack` to return the correct stack amount
 - replace `StardewValley.Object.addToStack` to add items correctly
 - replace `DecoratableLocation.leftClick` to not overwrite items when picking up furniture with a full inventory

It also replaces placed down tappers and furniture into new instances of those items so there aren't multiple instances in different locations

Note that this repo does not contain the Harmony dll, and the ones found in the mentioned link are not compiled targeting .NET 4.5. You'll need to compile for .NET 4.5 yourself, or just copy the dll packaged in the release.