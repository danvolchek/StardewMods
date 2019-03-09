# Stack Everything


See [This link](http://www.nexusmods.com/stardewvalley/mods/2053?) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

This mod uses [Harmony](https://github.com/pardeike/Harmony) to:
 - replace `StardewValley.Object.maximumStackSize` to always return 999.
 - postfix `StardewValley.Object.drawInMenu` to correctly draw the stack number.
 - replace `StardewValley.Object.getStack` to return the correct stack amount.
 - replace `StardewValley.Object.addToStack` to add items correctly.
 - replace `DecoratableLocation.leftClick` to not overwrite items when picking up furniture with a full inventory.

It also replaces placed down tappers and furniture into new instances of those items so there isn't the same instance of the same item in multiple locations.