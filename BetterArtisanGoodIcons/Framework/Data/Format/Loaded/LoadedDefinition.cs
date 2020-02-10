using Microsoft.Xna.Framework.Graphics;

namespace BetterArtisanGoodIcons.Framework.Data.Format.Loaded
{
    internal class LoadedDefinition
    {
        public ItemIndicator[] SourceItems { get; set; } = null;

        public ItemIndicator ArtisanGood { get; set; }

        public Texture2D Texture { get; set; } = null;
    }
}
