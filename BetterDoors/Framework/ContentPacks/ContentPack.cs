using System.Collections.Generic;

namespace BetterDoors.Framework.ContentPacks
{
    /*********
    ** Accessors
    *********/

    /// <summary> The format content packs that provide door animations must follow.</summary>
    internal class ContentPack
    {
        /// <summary>The version of the content pack.</summary>
        public string Version { get; set; }

        /// <summary>The doors the content pack provides.</summary>
        public IDictionary<string, IList<string>> Doors { get; set; }
    }
}
