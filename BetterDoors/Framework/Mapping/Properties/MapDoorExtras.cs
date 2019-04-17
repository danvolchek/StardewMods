using StardewModdingAPI;
using System;

namespace BetterDoors.Framework.Mapping.Properties
{
    internal class MapDoorExtras
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the door is a double door.</summary>
        public bool IsDoubleDoor { get; }

        /// <summary>Whether the door is an automatic door.</summary>
        public bool IsAutomaticDoor { get; }

        /*********
        ** Fields
        *********/
        /// <summary>The key to read this property with.</summary>
        public const string PropertyKey = "DoorExtras";

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isDoubleDoor">Whether the door is a double door.</param>
        /// <param name="isAutomaticDoor">Whether the door is an automatic door.</param>
        public MapDoorExtras(bool isDoubleDoor, bool isAutomaticDoor)
        {
            this.IsDoubleDoor = isDoubleDoor;
            this.IsAutomaticDoor = isAutomaticDoor;
        }

        /// <summary>Construct an instance with all values set to false.</summary>
        public MapDoorExtras() : this(false, false)
        {
        }

        /// <summary>Try to parse an instance from a string.</summary>
        /// <param name="propertyString">The string to parse.</param>
        /// <param name="version">The version the string follows.</param>
        /// <param name="error">The error while parsing, if any.</param>
        /// <param name="property">The resulting property, if no errors.</param>
        /// <returns>If parsing was successful.</returns>
        public static bool TryParseString(string propertyString, ISemanticVersion version, out string error, out MapDoorExtras property)
        {
            error = null;
            property = null;

            string[] parts = propertyString.Split(' ');

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

            property = new MapDoorExtras(isDoubleDoor, isAutomaticDoor);
            return true;
        }
    }
}
