using System;
using BetterDoors.Framework.Enums;
using System.Collections.Generic;

namespace BetterDoors.Framework.DoorGeneration
{
    internal class GeneratedSpriteManager
    {
        private readonly IDictionary<string, IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>> spritesByMod = new Dictionary<string, IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>>(StringComparer.InvariantCultureIgnoreCase);

        public bool GetDoorSprite(string modId, string spriteName, Orientation orientation, OpeningDirection openingDirection, out string error, out GeneratedDoorTileInfo generatedDoorTileInfo)
        {
            error = null;
            generatedDoorTileInfo = null;

            if (!this.spritesByMod.TryGetValue(modId, out IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>> spritesByName))
            {
                error = $"{modId} is not a valid content pack to choose from. Must be one of {string.Join(", ", this.spritesByMod.Keys)}";
                return false;
            }

            if (!spritesByName.TryGetValue(spriteName, out IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>> spritesByOrientation))
            {
                error = $"{modId} doesn't have a door named {spriteName}. Must be one of {string.Join(", ", spritesByName.Keys)}";
                return false;
            }

            if (!spritesByOrientation.TryGetValue(orientation, out IDictionary<OpeningDirection, GeneratedDoorTileInfo> spritesByOpeningDirection))
            {
                error = $"{orientation} isn't a valid orientation. Must be one of {string.Join(", ", spritesByOrientation.Keys)}";
                return false;
            }

            if (!spritesByOpeningDirection.TryGetValue(openingDirection, out generatedDoorTileInfo))
            {
                error = $"{openingDirection} isn't a valid flipped value. Must be one of {string.Join(", ", spritesByOpeningDirection.Keys)}";
                return false;
            }

            return true;
        }

        public void RegisterDoorSprite(string modId, string spriteName, Orientation orientation, OpeningDirection openingDirection, GeneratedDoorTileInfo generatedDoorTileInfo)
        {
            if (!this.spritesByMod.ContainsKey(modId))
                this.spritesByMod[modId] = new Dictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>(StringComparer.InvariantCultureIgnoreCase);

            if (!this.spritesByMod[modId].ContainsKey(spriteName))
                this.spritesByMod[modId][spriteName] = new Dictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>();

            if (!this.spritesByMod[modId][spriteName].ContainsKey(orientation))
                this.spritesByMod[modId][spriteName][orientation] = new Dictionary<OpeningDirection, GeneratedDoorTileInfo>();


            this.spritesByMod[modId][spriteName][orientation][openingDirection] = generatedDoorTileInfo;
        }
    }
}