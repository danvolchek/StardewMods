using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>Registers door tile sheets with SMAPI so maps can access them.</summary>
    internal class DoorAssetLoader : IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>Map of currently registered textures by asset key.</summary>
        private readonly IDictionary<string, Texture2D> doorTextures = new Dictionary<string, Texture2D>();

        /// <summary>Provides an API for loading content assets.</summary>
        private readonly IContentHelper helper;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="helper">Provides an API for loading content assets.</param>
        public DoorAssetLoader(IContentHelper helper)
        {
            this.helper = helper;
            this.helper.AssetLoaders.Add(this);
        }

        /// <summary>Add textures to be loaded.</summary>
        /// <param name="textures">The textures to add.</param>
        public void AddTextures(IDictionary<string, Texture2D> textures)
        {
            foreach (KeyValuePair<string, Texture2D> texture in textures)
                this.doorTextures[texture.Key] = texture.Value;

            this.InvalidateCache(textures);
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return this.doorTextures.Keys.Any(asset.AssetNameEquals);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return (T) (object) this.doorTextures[asset.AssetName];
        }

        /// <summary>Reset the asset loader, clearing loaded textures.</summary>
        public void Reset()
        {
            IDictionary<string, Texture2D> tempTextures = new Dictionary<string, Texture2D>(this.doorTextures);
            this.doorTextures.Clear();

            this.InvalidateCache(tempTextures);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Invalidates the cache.</summary>
        /// <param name="textures">The textures to invalidate.</param>
        private void InvalidateCache(IDictionary<string, Texture2D> textures)
        {
            this.helper.InvalidateCache(asset => asset.DataType == typeof(Texture2D) && textures.Keys.Any(asset.AssetNameEquals));
        }
    }
}