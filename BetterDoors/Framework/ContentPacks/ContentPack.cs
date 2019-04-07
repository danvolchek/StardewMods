using StardewModdingAPI;
using System.Collections.Generic;

namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>
    /// The format content packs must follow.
    /// </summary>
    internal class ContentPack
    {
        public ISemanticVersion Version { get; }
        public IList<ContentPackDoorEntry> Doors { get; }
    }
}