using System;
using BetterDoors.Framework.Enums;
using StardewModdingAPI;
using SemanticVersion = StardewModdingAPI.Toolkit.SemanticVersion;

namespace BetterDoors.Framework.Mapping
{
    /// <summary>
    /// A property value placed on a tile indicating which door to spawn.
    /// </summary>
    internal class MapDoorProperty
    {
        public ISemanticVersion PropertyVersion { get; }
        public string ModId { get; }
        public string DoorName { get; }
        public Orientation Orientation { get; }
        public OpeningDirection OpeningDirection { get; }

        public MapDoorProperty(ISemanticVersion propertyVersion, string modId, string doorName, Orientation orientation, OpeningDirection openingDirection)
        {
            this.PropertyVersion = propertyVersion;
            this.ModId = modId;
            this.DoorName = doorName;
            this.Orientation = orientation;
            this.OpeningDirection = openingDirection;
        }

        /// <summary>
        /// Parses properties of the form "versionNumber modID doorName orientation openingDirection"
        /// </summary>
        /// <param name="propertyString"></param>
        /// <param name="error"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool TryParseString(string propertyString, out string error, out MapDoorProperty property)
        {
            error = null;
            property = null;

            string[] parts = propertyString.Split(' ');

            if (!SemanticVersion.TryParse(parts[0], out ISemanticVersion version))
            {
                error = $"Invalid property version {parts[0]}";
                return false;
            }

            if (!version.Equals(new SemanticVersion(1, 0, 0)))
            {
                error = $"Property version {version} is not recognized";
                return false;
            }

            if (parts.Length != 5)
            {
                error = $"Must provide exactly 5 arguments. Found {parts.Length}";
                return false;
            }

            if (!Enum.TryParse(parts[3], true, out Orientation orientation))
            {
                error = $"{parts[3]} is an invalid door orientation. Must be one of [{string.Join(", ", Enum.GetNames(typeof(Orientation)))}]";
                return false;
            }

            if (!Enum.TryParse(parts[4], true, out OpeningDirection direction))
            {
                error = $"{parts[4]} is an invalid door opening direction. Must be one of [{string.Join(", ", Enum.GetNames(typeof(OpeningDirection)))}]";
                return false;
            }

            property = new MapDoorProperty(version, parts[1].ToLower(), parts[2], orientation, direction);
            return true;
        }
    }
}