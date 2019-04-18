# Better Doors


See [This link](http://www.nexusmods.com/stardewvalley/mods/????) for the NexusMods mod page, which has a description, screenshots, and a download of the built mod.

## How it works

The mod:
- Loads content packs and vanilla assets to find provided door sprites.
- Reads loaded maps to look for requested doors sprites.
- Dynamically generates door sprites based on loaded content packs/vanilla assets.
  - Sprites are only generated if they're requested.
  - Sprite generation means users don't need to draw rotations/transformations.
- Attaches doors to the maps.

See the wiki for more info about creating custom door sprites or adding custom doors to maps: https://github.com/danvolchek/StardewMods/wiki/Better-Doors.