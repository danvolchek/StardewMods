using BetterDoors.Framework.Enums;
using System;
using System.Collections.Generic;

namespace BetterDoors.Framework.DoorGeneration
{
    internal class GeneratedDoorTileInfoManager
    {
        /*********
        ** Fields
        *********/

        /// <summary>Maps mod id -> door name -> door orientation -> opening direction -> generated tile info</summary>
        private readonly IDictionary<string, IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>> generatedTileInfo = new Dictionary<string, IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>Maps mod id -> door name -> door orientation -> opening direction -> generated tile info</summary>
        private readonly IDictionary<string, IDictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>>> doorRequests = new Dictionary<string, IDictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>>>(StringComparer.InvariantCultureIgnoreCase);

        /*********
        ** Public methods
        *********/

        /// <summary>Gets generated tile info.</summary>
        /// <param name="modId">The mod id.</param>
        /// <param name="doorName">The door name.</param>
        /// <param name="orientation">The door orientation.</param>
        /// <param name="openingDirection">The door opening direction.</param>
        /// <param name="error">Any errors found while getting the info, if any.</param>
        /// <param name="generatedDoorTileInfo">The generated tile info, if found.</param>
        /// <returns>Whether getting the info was successful.</returns>
        public bool GetGeneratedTileInfo(string modId, string doorName, Orientation orientation, OpeningDirection openingDirection, out string error, out GeneratedDoorTileInfo generatedDoorTileInfo)
        {
            error = null;
            generatedDoorTileInfo = null;

            if (!this.generatedTileInfo.TryGetValue(modId, out IDictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>> spritesByName))
            {
                error = $"{modId} is not a valid content pack to choose from. Must be one of {string.Join(", ", this.generatedTileInfo.Keys)}";
                return false;
            }

            if (!spritesByName.TryGetValue(doorName, out IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>> spritesByOrientation))
            {
                error = $"{modId} doesn't have a door named {doorName}. Must be one of {string.Join(", ", spritesByName.Keys)}";
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

        /// <summary>Register door tile info with the manager.</summary>
        /// <param name="modId">The mod id.</param>
        /// <param name="doorName">The door name.</param>
        /// <param name="orientation">The door orientation.</param>
        /// <param name="openingDirection">The door opening direction.</param>
        /// <param name="generatedDoorTileInfo">The generated tile info, if found.</param>
        public void RegisterGeneratedTileInfo(string modId, string doorName, Orientation orientation, OpeningDirection openingDirection, GeneratedDoorTileInfo generatedDoorTileInfo)
        {
            if (!this.generatedTileInfo.ContainsKey(modId))
                this.generatedTileInfo[modId] = new Dictionary<string, IDictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>>(StringComparer.InvariantCultureIgnoreCase);

            if (!this.generatedTileInfo[modId].ContainsKey(doorName))
                this.generatedTileInfo[modId][doorName] = new Dictionary<Orientation, IDictionary<OpeningDirection, GeneratedDoorTileInfo>>();

            if (!this.generatedTileInfo[modId][doorName].ContainsKey(orientation))
                this.generatedTileInfo[modId][doorName][orientation] = new Dictionary<OpeningDirection, GeneratedDoorTileInfo>();

            this.generatedTileInfo[modId][doorName][orientation][openingDirection] = generatedDoorTileInfo;
        }

        /// <summary>Register a door request with the manager.</summary>
        /// <param name="modId">The mod id.</param>
        /// <param name="doorName">The door name.</param>
        /// <param name="orientation">The door orientation.</param>
        /// <param name="openingDirection">The door opening direction.</param>
        public void RegisterDoorRequest(string modId, string doorName, Orientation orientation, OpeningDirection openingDirection)
        {
            if (!this.doorRequests.ContainsKey(modId))
                this.doorRequests[modId] = new Dictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>>(StringComparer.InvariantCultureIgnoreCase);

            if (!this.doorRequests[modId].ContainsKey(doorName))
                this.doorRequests[modId][doorName] = new Dictionary<Orientation, ISet<OpeningDirection>>();

            if (!this.doorRequests[modId][doorName].ContainsKey(orientation))
                this.doorRequests[modId][doorName][orientation] = new HashSet<OpeningDirection>();

            this.doorRequests[modId][doorName][orientation].Add(openingDirection);
        }

        /// <summary>Checks whether a door is requested or not.</summary>
        /// <param name="modId">The mod id.</param>
        /// <param name="doorName">The door name.</param>
        /// <param name="orientation">The door orientation.</param>
        /// <param name="openingDirection">The door opening direction.</param>
        /// <returns>Whether the door is requested.</returns>
        public bool IsDoorRequested(string modId, string doorName, Orientation orientation, OpeningDirection openingDirection)
        {
            return this.doorRequests.TryGetValue(modId, out IDictionary<string, IDictionary<Orientation, ISet<OpeningDirection>>> requestsByName) &&
                   requestsByName.TryGetValue(doorName, out IDictionary<Orientation, ISet<OpeningDirection>> requestsByOrientation) &&
                   requestsByOrientation.TryGetValue(orientation, out ISet<OpeningDirection> requestsByOpeningDirection) &&
                   requestsByOpeningDirection.Contains(openingDirection);
        }

        /// <summary>Resets the manager, resetting the registered doors and requests.</summary>
        public void Reset()
        {
            this.generatedTileInfo.Clear();
            this.doorRequests.Clear();
        }
    }
}
