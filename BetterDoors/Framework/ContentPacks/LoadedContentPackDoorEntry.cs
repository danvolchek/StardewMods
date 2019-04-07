using Microsoft.Xna.Framework.Graphics;

namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>
    /// A <see cref="ContentPackDoorEntry"/> with the texture loaded and some other info.
    /// </summary>
    internal class LoadedContentPackDoorEntry
    {
        public string ModId { get; }
        public Texture2D Texture { get; }
        public ContentPackDoorEntry Entry { get; }

        public LoadedContentPackDoorEntry(string modId, Texture2D texture, ContentPackDoorEntry entry)
        {
            this.ModId = modId;
            this.Texture = texture;
            this.Entry = entry;
        }
    }
}