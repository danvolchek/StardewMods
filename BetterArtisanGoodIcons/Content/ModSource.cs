using StardewModdingAPI;

namespace BetterArtisanGoodIcons.Content
{
    /// <inheritdoc />
    /// <summary>A content source that comes from a mod. </summary>
    internal class ModSource : TextureDataContentSource
    {
        private readonly IModHelper helper;

        public override CustomTextureData TextureData { get; }

        public ModSource(IModHelper helper)
        {
            this.helper = helper;
            this.TextureData = helper.Data.ReadJsonFile<CustomTextureData>("assets/data.json");
        }

        public override T Load<T>(string path)
        {
            return this.helper.ModContent.Load<T>(path);
        }

        public override IManifest GetManifest()
        {
            return this.helper.ModRegistry.Get(this.helper.ModRegistry.ModID).Manifest;
        }
    }
}
