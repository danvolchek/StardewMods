using StardewModdingAPI;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BetterDoors.Framework.ContentPacks
{
    /*********
    ** Accessors
    *********/
    /// <summary> The format content packs that provide door animations must follow.</summary>
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty", Justification = "This class is loaded from a file by a JSON parser.")]
    internal class ContentPack
    {
        /// <summary>The version of the content pack.</summary>
        public ISemanticVersion Version { get; }

        /// <summary>The doors the content pack provides.</summary>
        public IList<ContentPackDoorEntry> Doors { get; }
    }
}