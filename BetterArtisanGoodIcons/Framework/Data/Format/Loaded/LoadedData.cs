using StardewModdingAPI;

namespace BetterArtisanGoodIcons.Framework.Data.Format.Loaded
{
    internal class LoadedData
    {
        public IManifest Manifest { get; set; }

        public LoadedDefinition[] ArtisanGoods { get; set; } = null;

        public bool CanBeOverwritten { get; set; } = false;
    }
}
