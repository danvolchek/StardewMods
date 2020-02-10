using System;
using BetterArtisanGoodIcons.Framework.Data.Format.Loaded;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using BetterArtisanGoodIcons.Framework.Data.Format;
using BetterArtisanGoodIcons.Framework.Data.Format.Unloaded;
using StardewModdingAPI;

namespace BetterArtisanGoodIcons.Framework.Data.Loading
{
    internal class Loader
    {
        private readonly IMonitor monitor;

        public Loader(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public LoadedData Load(IDataSource source)
        {
            return new LoadedData
            {
                Manifest = source.Manifest,
                ArtisanGoods = this.LoadDefinitions(source, this.CollapseUnloadedDefinitions(source)).ToArray(),
                CanBeOverwritten = source.UnloadedData.CanBeOverwritten
            };
        }

        private IEnumerable<LoadedDefinition> LoadDefinitions(IDataSource source, IEnumerable<UnloadedDefinitions> unloadedDefinitions)
        {
            IList<Tuple<ItemIndicator, string, Exception>> failedLoads = new List<Tuple<ItemIndicator, string, Exception>>();
            IList<Tuple<ItemIndicator, string, ItemIndicator[], Texture2D>> tooSmallTextures = new List<Tuple<ItemIndicator, string, ItemIndicator[], Texture2D>>();

            foreach (UnloadedDefinitions unloadedDefinition in unloadedDefinitions)
            {
                foreach (UnloadedTextureInfo unloadedTextureInfo in unloadedDefinition.TextureInfo)
                {
                    LoadedDefinition definition = null;

                    try
                    {
                        Texture2D texture = source.Load<Texture2D>(unloadedTextureInfo.TextureFilePath);

                        if (this.GetMaxSprites(texture) < unloadedDefinition.SourceItems.Length)
                        {
                            tooSmallTextures.Add(new Tuple<ItemIndicator, string, ItemIndicator[], Texture2D>(unloadedTextureInfo.ArtisanGood, unloadedTextureInfo.TextureFilePath, unloadedDefinition.SourceItems, texture));
                        }
                        else
                        {
                            definition = new LoadedDefinition
                            {
                                SourceItems = unloadedDefinition.SourceItems,
                                Texture = texture,
                                ArtisanGood = unloadedTextureInfo.ArtisanGood
                            };
                        }

                    }
                    catch (Exception e)
                    {
                        failedLoads.Add(new Tuple<ItemIndicator, string, Exception>(unloadedTextureInfo.ArtisanGood, unloadedTextureInfo.TextureFilePath, e));
                    }

                    if (definition != null)
                        yield return definition;
                }
            }

            if (failedLoads.Any())
            {
                this.monitor.Log($"Failed to load some textures for content pack ${source.Manifest.Name} (${source.Manifest.UniqueID}). BAGI won't draw textures for these images:", LogLevel.Error);
                foreach (Tuple<ItemIndicator, string, Exception> failedLoad in failedLoads)
                {
                    this.monitor.Log($"\t-${failedLoad.Item1} ({failedLoad.Item2}): ${failedLoad.Item3.Message}.", LogLevel.Error);
                }
            }

            if (tooSmallTextures.Any())
            {
                this.monitor.Log($"Some textures were too small for their source list for content pack ${source.Manifest.Name} (${source.Manifest.UniqueID}). BAGI won't draw textures for these images:", LogLevel.Error);
                foreach (Tuple<ItemIndicator, string, ItemIndicator[], Texture2D> tooSmall in tooSmallTextures)
                {
                    this.monitor.Log($"\t-${tooSmall.Item1} ({tooSmall.Item2}): {tooSmall.Item3} has {tooSmall.Item3.Length} items, but texture is {tooSmall.Item4.Width}x{tooSmall.Item4.Height} and only supports {this.GetMaxSprites(tooSmall.Item4)}.", LogLevel.Error);
                }
            }
        }

        private IEnumerable<UnloadedDefinitions> CollapseUnloadedDefinitions(IDataSource source)
        {
            if(source.UnloadedData.Honey != null)
                yield return new UnloadedDefinitions
                {
                    SourceItems = source.UnloadedData.Flowers,
                    TextureInfo = new[]
                    {
                        new UnloadedTextureInfo
                        {
                            ArtisanGood = VanillaArtisanGood.Honey,
                            TextureFilePath = source.UnloadedData.Honey
                        }
                    }
                };

            if (source.UnloadedData.Jelly != null)
                yield return new UnloadedDefinitions
                {
                    SourceItems = source.UnloadedData.Fruits,
                    TextureInfo = new[]
                    {
                        new UnloadedTextureInfo
                        {
                            ArtisanGood = VanillaArtisanGood.Jelly,
                            TextureFilePath = source.UnloadedData.Jelly
                        }
                    }
                };

            if (source.UnloadedData.Wine != null)
                yield return new UnloadedDefinitions
                {
                    SourceItems = source.UnloadedData.Fruits,
                    TextureInfo = new[]
                    {
                        new UnloadedTextureInfo
                        {
                            ArtisanGood = VanillaArtisanGood.Wine,
                            TextureFilePath = source.UnloadedData.Wine
                        }
                    }
                };

            if (source.UnloadedData.Juice != null)
                yield return new UnloadedDefinitions
                {
                    SourceItems = source.UnloadedData.Vegetables,
                    TextureInfo = new[]
                    {
                        new UnloadedTextureInfo
                        {
                            ArtisanGood = VanillaArtisanGood.Juice,
                            TextureFilePath = source.UnloadedData.Juice
                        }
                    }
                };

            if (source.UnloadedData.Pickles != null)
                yield return new UnloadedDefinitions
                {
                    SourceItems = source.UnloadedData.Vegetables,
                    TextureInfo = new[]
                    {
                        new UnloadedTextureInfo
                        {
                            ArtisanGood = VanillaArtisanGood.Pickles,
                            TextureFilePath = source.UnloadedData.Pickles
                        }
                    }
                };

            if (source.UnloadedData.CustomArtisanGoods != null)
            {
                foreach (UnloadedDefinitions definitions in source.UnloadedData.CustomArtisanGoods)
                {
                    yield return definitions;
                }
            }
        }

        private int GetMaxSprites(Texture2D texture)
        {
            return (texture.Width / VanillaArtisanGood.SpriteSize) * (texture.Height / VanillaArtisanGood.SpriteSize);
        }
    }
}
