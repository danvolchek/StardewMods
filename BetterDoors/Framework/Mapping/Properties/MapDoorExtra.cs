using System;
using StardewModdingAPI;
using SemanticVersion = StardewModdingAPI.Toolkit.SemanticVersion;

namespace BetterDoors.Framework.Mapping.Properties
{
    internal class MapDoorExtraProperty
    {
        public static string PropertyKey = "DoorExtra";
        
        public bool IsDoubleDoor { get; }
        public bool IsAutomaticDoor { get; }

        public MapDoorExtraProperty(bool isDoubleDoor, bool isAutomaticDoor)
        {
            this.IsDoubleDoor = isDoubleDoor;
            this.IsAutomaticDoor = isAutomaticDoor;
        }

        public MapDoorExtraProperty() : this(false, false)
        {
        }

        /// <summary>
        /// Parses properties of the form "versionNumber modID doorName orientation openingDirection"
        /// </summary>
        /// <returns></returns>
        public static bool TryParseString(string propertyString, ISemanticVersion version, out string error, out MapDoorExtraProperty property)
        {
            error = null;
            property = null;

            string[] parts = propertyString.Split(' ');

            if (parts.Length == 0)
            {
                error = "No extras provided";
                return false;
            }

            if (!version.Equals(new SemanticVersion(1, 0, 0)))
            {
                error = $"Property version {version} is not recognized";
                return false;
            }

            bool isDoubleDoor = false;
            bool isAutomaticDoor = false;

            foreach (string part in parts)
            {
                if (part.Equals("double", StringComparison.InvariantCultureIgnoreCase))
                {
                    isDoubleDoor = true;
                } else if (part.Equals("automatic", StringComparison.InvariantCultureIgnoreCase))
                {
                    isAutomaticDoor = true;
                }
                else
                {
                    error = $"Unrecognized door extra {part} using format version {version}";
                    return false;
                }
            }

            property = new MapDoorExtraProperty(isDoubleDoor, isAutomaticDoor);
            return true;
        }
    }
}
