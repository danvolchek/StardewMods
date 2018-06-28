using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace BetterArtisanGoodIcons
{
    /// <summary>Handles alternate textures from content packs.</summary>
    internal class ContentPackManager
    {
        private readonly IDictionary<string, Tuple<IManifest, Texture2D>> loadedTextures = new Dictionary<string, Tuple<IManifest, Texture2D>>();

        internal ContentPackManager(IModHelper helper, IMonitor monitor, IEnumerable<string> imagesToLoad)
        {
            foreach (IContentPack pack in helper.GetContentPacks())
            {
                foreach (string image in imagesToLoad)
                {
                    try
                    {
                        Texture2D texture = pack.LoadAsset<Texture2D>($"assets/{image}.png");

                        if (this.loadedTextures.TryGetValue(image, out Tuple<IManifest, Texture2D> loaded))
                        {
                            monitor.Log($"Couldn't load {image} from {pack.Manifest.Name} ({pack.Manifest.UniqueID}) because {image} from {loaded.Item1.Name} ({loaded.Item1.UniqueID}) was already loaded.", LogLevel.Warn);
                        }
                        else
                        {
                            this.loadedTextures[image] = new Tuple<IManifest, Texture2D>(pack.Manifest, texture);
                            monitor.Log($"Loaded {image} from {pack.Manifest.Name} ({pack.Manifest.UniqueID}).", LogLevel.Trace);
                        }
                    }
                    catch
                    {
                        monitor.Log($"Couldn't load assets/{image}.png from {pack.Manifest.Name} ({pack.Manifest.UniqueID}).", LogLevel.Trace);
                    }
                }
            }

            foreach (string image in imagesToLoad)
            {
                if (!this.loadedTextures.ContainsKey(image))
                {
                    this.loadedTextures[image] = new Tuple<IManifest, Texture2D>(null, helper.Content.Load<Texture2D>($"assets/{image}.png"));
                    monitor.Log($"Loaded default {image}.", LogLevel.Trace);
                }
            }
        }

        /// <summary>Get loaded textures.</summary>
        internal IEnumerable<Tuple<string, Texture2D>> GetTextures()
        {
            foreach (KeyValuePair<string, Tuple<IManifest, Texture2D>> item in this.loadedTextures)
                yield return new Tuple<string, Texture2D>(item.Key, item.Value.Item2);
        }
    }
}