# Stack Everything


See [This link](http://www.nexusmods.com/stardewvalley/mods/2053?) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

This mod uses [Harmony](https://github.com/pardeike/Harmony) to replace the `StardewValley.Object.maximumStackSize` method to always return 999, and also uses a postfix on `StardewValley.Object.drawInMenu` to correctly draw the stack number.

Note that this repo does not contain the Harmony dll, and the ones found in the mentioned link are not compiled targeting .NET 4.5. You'll need to compile for .NET 4.5 yourself, or just copy the dll packaged in the release.