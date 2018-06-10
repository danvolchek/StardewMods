# Custom Transparency


See [This link](http://www.nexusmods.com/stardewvalley/mods/2359?) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

The mod uses [Harmony](https://github.com/pardeike/Harmony) to transpile the methods which control the minimum transparency setting (which is hardcoded to 0.4) to what the user specifies.

Note that this repo does not contain the Harmony dll, and the ones found in the mentioned link are not compiled targeting .NET 4.5. You'll need to compile for .NET 4.5 yourself, or just copy the dll packaged in the release.