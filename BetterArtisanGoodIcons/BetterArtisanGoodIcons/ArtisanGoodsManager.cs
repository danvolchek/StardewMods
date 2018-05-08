using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons
{
    /// <summary>Manages textures for all artisan goods.</summary>
    internal static class ArtisanGoodsManager
    {
        /// <summary>The artisan goods that we add unique icons foor.</summary>
        internal enum ArtisanGood
        {
            Jelly = 344,
            Pickle = 342,
            Wine = 348,
            Juice = 350,
            Honey = 340
        };

        /// <summary>Info needed to load and set up textures for each artisan good.</summary>
        private static readonly IDictionary<string, Tuple<ArtisanGood, int>> ArtisanGoodInfo = new Dictionary<string, Tuple<ArtisanGood, int>>()
        {
            {"jelly" , ArtisanGood.Jelly , SObject.FruitsCategory   },
            {"pickle", ArtisanGood.Pickle, SObject.VegetableCategory},
            {"wine"  , ArtisanGood.Wine  , SObject.FruitsCategory   },
            {"juice" , ArtisanGood.Juice , SObject.VegetableCategory},
            {"honey" , ArtisanGood.Honey , SObject.flowersCategory  }
        };

        /// <summary>Texture managers that get the correct texture for each item.</summary>
        private static readonly IList<ArtisanGoodTextureManager> TextureManagers = new List<ArtisanGoodTextureManager>();

        /// <summary>Mod config.</summary>
        private static BetterArtisanGoodIconsConfig config;

        /// <summary>Initializes the manager.</summary>
        internal static void Init(IModHelper helper, IMonitor monitor)
        {
            config = helper.ReadConfig<BetterArtisanGoodIconsConfig>();
            foreach (Tuple<string, Texture2D> textureInfo in new ContentPackManager(helper, monitor, ArtisanGoodInfo.Keys).GetTextures())
                TextureManagers.Add(new ArtisanGoodTextureManager(textureInfo.Item2, ArtisanGoodInfo[textureInfo.Item1].Item1, ArtisanGoodInfo[textureInfo.Item1].Item2));
        }

        /// <summary>Gets the info needed to draw the correct texture for the given artisan good.</summary>
        internal static bool GetDrawInfo(SObject item, out Texture2D mainTexture, out Rectangle mainPosition, out Rectangle iconPosition)
        {
            if (item.ParentSheetIndex == (int)ArtisanGood.Honey)
                return GetDrawInfoHoney(item.ParentSheetIndex, item.Name, out mainTexture, out mainPosition, out iconPosition);
            else
                return GetDrawInfoNonHoney(item.ParentSheetIndex, item.preservedParentSheetIndex.Value, out mainTexture, out mainPosition, out iconPosition);
        }

        /// <summary>Gets the info needed to draw the correct texture for non-honey artisan goods.</summary>
        private static bool GetDrawInfoNonHoney(int outputIndex, int regentIndex, out Texture2D mainTexture, out Rectangle mainPosition, out Rectangle iconPosition)
        {
            foreach (ArtisanGoodTextureManager manager in TextureManagers)
            {
                if (manager.CanHandle(outputIndex))
                {
                    mainTexture = manager.GetSpritesheet(regentIndex);
                    manager.GetPosition(regentIndex, out mainPosition, out iconPosition);
                    if (config.DisableSmallSourceIcons)
                        iconPosition = Rectangle.Empty;
                    return true;
                }
            }

            mainTexture = null;
            mainPosition = iconPosition = Rectangle.Empty;
            mainPosition = Rectangle.Empty;
            return false;
        }

        /// <summary>Gets the info needed to draw the correct texture for honey.</summary>
        private static bool GetDrawInfoHoney(int outputIndex, string honeyName, out Texture2D mainTexture, out Rectangle mainPosition, out Rectangle iconPosition)
        {
            foreach (ArtisanGoodTextureManager manager in TextureManagers)
            {
                if (manager.CanHandle(outputIndex))
                {
                    mainTexture = manager.GetSpritesheetHoney(honeyName);
                    manager.GetPositionHoney(honeyName, out mainPosition, out iconPosition);
                    if (config.DisableSmallSourceIcons)
                        iconPosition = Rectangle.Empty;
                    return true;
                }
            }

            mainTexture = null;
            mainPosition = iconPosition = Rectangle.Empty;
            return false;
        }
    }
}