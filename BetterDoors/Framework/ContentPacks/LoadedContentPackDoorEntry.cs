using Microsoft.Xna.Framework.Graphics;

namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>A <see cref="ContentPackDoorEntry"/> with the texture loaded and some other info.</summary>
    internal class LoadedContentPackDoorEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod id.</summary>
        public string ModId { get; }

        /// <summary>The loaded texture.</summary>
        public Texture2D Texture { get; }

        /// <summary>The content pack entry this was loaded from.</summary>
        public ContentPackDoorEntry Entry { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modId">The mod id.</param>
        /// <param name="texture">The loaded texture.</param>
        /// <param name="entry">The content pack entry this was loaded from.</param>
        public LoadedContentPackDoorEntry(string modId, Texture2D texture, ContentPackDoorEntry entry)
        {
            this.ModId = modId;
            this.Texture = texture;
            this.Entry = entry;
        }
    }
}