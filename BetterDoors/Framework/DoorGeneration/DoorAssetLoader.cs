using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Loads door sprite assets so maps can access them.
    /// </summary>
    internal class DoorAssetLoader : IAssetLoader
    {
        private IDictionary<string, Texture2D> doorTextures;
        private readonly IContentHelper helper;
        private bool attachedToSMAPI;

        public DoorAssetLoader(IContentHelper helper)
        {
            this.helper = helper;
        }

        public void SetTextures(IDictionary<string, Texture2D> textures)
        {
            this.doorTextures = textures;

            if (!this.attachedToSMAPI)
            {
                this.helper.AssetLoaders.Add(this);
                this.attachedToSMAPI = true;
            }
            else
            {
                this.helper.InvalidateCache(asset => asset.DataType == typeof(Texture2D) && this.doorTextures.Keys.Any(asset.AssetNameEquals));
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return this.doorTextures.Keys.Any(asset.AssetNameEquals);
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T) (object) this.doorTextures[asset.AssetName];
        }
    }
}