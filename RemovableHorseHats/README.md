# Removable Horse Hats


See [This link](http://www.nexusmods.com/stardewvalley/mods/2223?) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

The mod uses [Harmony](https://github.com/pardeike/Harmony) to overwrite the horse's `checkAction` method to instead remove the hat the horse is wearing if it is wearing one and the player is holding down the activation key.

Note that this repo does not contain the Harmony dll, and the ones found in the mentioned link are not compiled targeting .NET 4.5. You'll need to compile for .NET 4.5 yourself, or just copy the dll packaged in the release.