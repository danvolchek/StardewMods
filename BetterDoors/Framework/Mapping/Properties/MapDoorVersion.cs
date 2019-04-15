using StardewModdingAPI;
using SemanticVersion = StardewModdingAPI.Toolkit.SemanticVersion;

namespace BetterDoors.Framework.Mapping.Properties
{
    /// <summary>Represents a version tile property.</summary>
    internal class MapDoorVersion
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The parsed version value.</summary>
        public ISemanticVersion PropertyVersion { get; }

        /*********
        ** Fields
        *********/
        /// <summary>The key to read this property with.</summary>
        public const string PropertyKey = "DoorVersion";

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="propertyVersion">The parsed version value.</param>
        public MapDoorVersion(ISemanticVersion propertyVersion)
        {
            this.PropertyVersion = propertyVersion;
        }

        /// <summary>Try to parse an instance from a string.</summary>
        /// <param name="propertyString">The string to parse.</param>
        /// <param name="error">The error while parsing, if any.</param>
        /// <param name="property">The resulting property, if no errors.</param>
        /// <returns>If parsing was successful.</returns>
        public static bool TryParseString(string propertyString, out string error, out MapDoorVersion property)
        {
            error = null;
            property = null;

            string[] parts = propertyString.Split(' ');

            if (parts.Length == 0)
            {
                error = "No property version specified";
                return false;
            }

            if (parts.Length > 1)
            {
                error = "Invalid version format";
                return false;
            }

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

            property = new MapDoorVersion(version);
            return true;
        }
    }
}
