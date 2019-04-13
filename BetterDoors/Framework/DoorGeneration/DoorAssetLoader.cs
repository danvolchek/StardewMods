using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Loads door sprite assets so maps can access them.
    /// </summary>
    internal class DoorAssetLoader : IAssetLoader, IResetable
    {
        private readonly IDictionary<string, Texture2D> doorTextures = new Dictionary<string, Texture2D>();
        private readonly IContentHelper helper;

        public DoorAssetLoader(IContentHelper helper)
        {
            this.helper = helper;
            this.helper.AssetLoaders.Add(this);
        }

        public void AddTextures(IDictionary<string, Texture2D> textures)
        {
            foreach (KeyValuePair<string, Texture2D> texture in textures)
                this.doorTextures[texture.Key] = texture.Value;

            this.helper.InvalidateCache(asset => asset.DataType == typeof(Texture2D) && textures.Keys.Any(asset.AssetNameEquals));
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return this.doorTextures.Keys.Any(asset.AssetNameEquals);
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T) (object) this.doorTextures[asset.AssetName];
        }

        public void Reset()
        {
            this.doorTextures.Clear();
        }
    }
}