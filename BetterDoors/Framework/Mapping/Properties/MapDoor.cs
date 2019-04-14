using System;
using BetterDoors.Framework.Enums;
using StardewModdingAPI;
using SemanticVersion = StardewModdingAPI.Toolkit.SemanticVersion;

namespace BetterDoors.Framework.Mapping.Properties
{
    /// <summary>
    /// A property value placed on a tile indicating which door to spawn.
    /// </summary>
    internal class MapDoorProperty
    {
        public static string PropertyKey = "Door";
        
        public string ModId { get; }
        public string DoorName { get; }
        public Orientation Orientation { get; }
        public OpeningDirection OpeningDirection { get; }

        public MapDoorProperty(string modId, string doorName, Orientation orientation, OpeningDirection openingDirection)
        {
            this.ModId = modId;
            this.DoorName = doorName;
            this.Orientation = orientation;
            this.OpeningDirection = openingDirection;
        }

        /// <summary>
        /// Parses properties of the form "versionNumber modID doorName orientation openingDirection"
        /// </summary>
        /// <returns></returns>
        public static bool TryParseString(string propertyString, ISemanticVersion version, out string error, out MapDoorProperty property)
        {
            error = null;
            property = null;

            string[] parts = propertyString.Split(' ');

            if (parts.Length != 4)
            {
                error = $"Must provide exactly 4 arguments. Found {parts.Length}";
                return false;
            }

            if (!version.Equals(new SemanticVersion(1, 0, 0)))
            {
                error = $"Property version {version} is not recognized";
                return false;
            }

            if (!Enum.TryParse(parts[2], true, out Orientation orientation))
            {
                error = $"{parts[2]} is an invalid door orientation. Must be one of [{string.Join(", ", Enum.GetNames(typeof(Orientation)))}]";
                return false;
            }

            if (!Enum.TryParse(parts[3], true, out OpeningDirection direction))
            {
                error = $"{parts[3]} is an invalid door opening direction. Must be one of [{string.Join(", ", Enum.GetNames(typeof(OpeningDirection)))}]";
                return false;
            }

            property = new MapDoorProperty(parts[0].ToLower(), parts[1], orientation, direction);
            return true;
        }
    }
}