using System;
using BetterDoors.Framework.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework.DoorGeneration
{
    internal class GeneratedSpriteManager : IResetable
    {
        private readonly IDictionary<string, IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>> spritesByMod = new Dictionary<string, IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>>(StringComparer.InvariantCultureIgnoreCase);
        private readonly IDictionary<string, IDictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>>> spriteRequests = new Dictionary<string, IDictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>>>(StringComparer.InvariantCultureIgnoreCase);

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

        public void RegisterDoorSpriteRequest(string modId, string spriteName, Orientation orientation, OpeningDirection openingDirection)
        {
            if (!this.spriteRequests.ContainsKey(modId))
                this.spriteRequests[modId] = new Dictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>>(StringComparer.InvariantCultureIgnoreCase);

            if (!this.spriteRequests[modId].ContainsKey(spriteName))
                this.spriteRequests[modId][spriteName] = new Dictionary<Orientation, ISet<OpeningDirection>>();

            if (!this.spriteRequests[modId][spriteName].ContainsKey(orientation))
                this.spriteRequests[modId][spriteName][orientation] = new HashSet<OpeningDirection>();

            this.spriteRequests[modId][spriteName][orientation].Add(openingDirection);
        }

        public bool IsSpriteRequested(string modId, string spriteName, Orientation orientation, OpeningDirection openingDirection)
        {
            return this.spriteRequests.TryGetValue(modId, out IDictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>> requestsByName) &&
                   requestsByName.TryGetValue(spriteName, out IDictionary<Orientation, ISet<OpeningDirection>> requestsByOrientation) &&
                   requestsByOrientation.TryGetValue(orientation, out ISet<OpeningDirection> requestsByOpeningDirection) &&
                   requestsByOpeningDirection.Contains(openingDirection);
        }

        public void Reset()
        {
            this.spritesByMod.Clear();
            this.spriteRequests.Clear();
        }
    }
}