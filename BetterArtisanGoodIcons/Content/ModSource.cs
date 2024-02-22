using StardewModdingAPI;

namespace BetterArtisanGoodIcons.Content
{
    /// <inheritdoc />
    /// <summary>A content source that comes from a mod. </summary>
    internal class ModSource : TextureDataContentSource
    {
        public override CustomTextureData TextureData { get; }

        public ModSource()
        {
            this.TextureData = ArtisanGoodsManager.Mod.Helper.Data.ReadJsonFile<CustomTextureData>("assets/data.json");
        }

        public override T Load<T>(string path)
        {
            return ArtisanGoodsManager.Mod.Helper.ModContent.Load<T>(path);
        }

        public override IManifest GetManifest()
        {
            return ArtisanGoodsManager.Mod.Helper.ModRegistry.Get(ArtisanGoodsManager.Mod.Helper.ModRegistry.ModID).Manifest;
        }
    }
}
