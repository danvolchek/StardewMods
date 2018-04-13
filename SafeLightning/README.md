# Safe Lightning


See [This link](http://www.nexusmods.com/stardewvalley/mods/2039?) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

The mod actually detects when lightning has hit something, i.e. a crop or tree, and then undoes the effects of that strike. This happens in four main phases:
- SDV's lightning strikes can be predicted ahead of time, so the `FeatureSaveDataFactory` may make a copy of the terrain feature that will be hit, based on what the associated resolver needs to undo the strike.
- An `IResultDetector` detects the specific result that was caused by the lightning strike on that feature (i.e. a crop is killed or a tree is starting to fall).
- The associated `IResultResolver` undoes the effects of the strike.
- The associated `IResultResolver` calls any necessary `ISideEffectHandler`s needed to undo side effects of the resolution (i.e. duplicate items).

The mod API allows other mods to let Safe Lightning know when a strike will happen, and then the procedure to handle them is exactly the same as above.