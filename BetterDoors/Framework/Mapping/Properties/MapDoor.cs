using BetterDoors.Framework.Enums;
using StardewModdingAPI;
using System;

namespace BetterDoors.Framework.Mapping.Properties
{
    /// <summary>
    /// A property value placed on a tile indicating which door to spawn.
    /// </summary>
    internal class MapDoor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod id to get the sprite from.</summary>
        public string ModId { get; }

        /// <summary>The name of the door sprite.</summary>
        public string DoorName { get; }

        /// <summary>The orientation to place the sprite at.</summary>
        public Orientation Orientation { get; }

        /// <summary>The direction the door should open in.</summary>
        public OpeningDirection OpeningDirection { get; }

        /*********
        ** Fields
        *********/
        /// <summary>The key to read this property with.</summary>
        public const string PropertyKey = "Door";

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modId">The mod id to get the sprite from.</param>
        /// <param name="doorName">The name of the door sprite.</param>
        /// <param name="orientation">The orientation to place the sprite at.</param>
        /// <param name="openingDirection">The direction the door should open in.</param>
        public MapDoor(string modId, string doorName, Orientation orientation, OpeningDirection openingDirection)
        {
            this.ModId = modId;
            this.DoorName = doorName;
            this.Orientation = orientation;
            this.OpeningDirection = openingDirection;
        }

        /// <summary>Try to parse an instance from a string.</summary>
        /// <remarks>Parses properties of the form "modID doorName orientation openingDirection"</remarks>
        /// <param name="propertyString">The string to parse.</param>
        /// <param name="version">The version the string follows.</param>
        /// <param name="error">The error while parsing, if any.</param>
        /// <param name="property">The resulting property, if no errors.</param>
        /// <returns>If parsing was successful.</returns>
        public static bool TryParseString(string propertyString, ISemanticVersion version, out string error, out MapDoor property)
        {
            error = null;
            property = null;

            string[] parts = propertyString.Split(' ');

            if (parts.Length != 4)
            {
                error = $"Must provide exactly 4 arguments. Found {parts.Length}";
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

            property = new MapDoor(parts[0].ToLower(), parts[1], orientation, direction);
            return true;
        }
    }
}