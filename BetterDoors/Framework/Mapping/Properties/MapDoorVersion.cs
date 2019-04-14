using StardewModdingAPI;
using SemanticVersion = StardewModdingAPI.Toolkit.SemanticVersion;

namespace BetterDoors.Framework.Mapping.Properties
{
    internal class MapDoorVersion
    {
        public static string PropertyKey = "DoorVersion";

        public ISemanticVersion PropertyVersion { get; }

        public MapDoorVersion(ISemanticVersion propertyVersion)
        {
            this.PropertyVersion = propertyVersion;
        }

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

            property = new MapDoorVersion(version);
            return true;
        }
    }
}
