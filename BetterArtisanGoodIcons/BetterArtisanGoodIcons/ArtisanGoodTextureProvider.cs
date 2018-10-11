using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons
{
    /// <summary>Provides textures for certain artisan goods.</summary>
    internal class ArtisanGoodTextureProvider
    {
        /// <summary>The spritesheet to pull textures from.</summary>
        private readonly Texture2D spriteSheet;

        /// <summary>The rectangles that correspond to each item name.</summary>
        private readonly IDictionary<string, Rectangle> positions = new Dictionary<string, Rectangle>();

        /// <summary>The type of artisan good this provides texture for.</summary>
        private readonly ArtisanGood good;

        internal ArtisanGoodTextureProvider(Texture2D texture, List<string> names, ArtisanGood good)
        {
            this.spriteSheet = texture;
            this.good = good;

            //Get sprite positions assuming names go left to right
            int x = 0;
            int y = 0;
            foreach (string item in names)
            {
                this.positions[item] = new Rectangle(x, y, 16, 16);
                x += 16;
                if (x >= texture.Width)
                {
                    x = 0;
                    y += 16;
                }
            }

        }

        /// <summary>Gets the name of the source item used to create the given item.</summary>
        private string GetSourceName(SObject item, int sourceIndex)
        {
            //If we failed to find the source of the item, and it happens to be the base item, call it Base. Otherwise return null for failure, which nothing will match.
            if (sourceIndex == -1)
            {
                if (item.Name == this.good.ToString())
                    return "_Base";
                return null;
            }
            //Lookup the name from the game's object information.
            return Game1.objectInformation[sourceIndex].Split('/')[0];
        }

        /// <summary>Gets the index of the source item used to create the given item name.</summary>
        private int GetIndexOfSource(SObject item)
        {
            //Use preservedParentSheetIndex for wine, jelly, pickles, and juice
            if (item.preservedParentSheetIndex.Value != 0)
                return item.preservedParentSheetIndex.Value;

            //Use honeyType for honey
            //Hardcode Wild honey using the sunflower image, since it is not called "Sunflower honey"
            if (this.good == ArtisanGood.Honey && item.honeyType.Value.HasValue)
                return item.honeyType.Value.Value != SObject.HoneyType.Wild ? (int)item.honeyType.Value.Value : 421;

            //Return -1 on failure
            return -1;
        }

        /// <summary>Gets the info needed to draw the right texture for the given item.</summary>
        internal bool GetDrawInfo(SObject item, ref Texture2D textureSheet, ref Rectangle mainPosition, ref Rectangle iconPosition)
        {
            int sourceIndex = this.GetIndexOfSource(item);
            string sourceName = this.GetSourceName(item, sourceIndex);
            if (item.ParentSheetIndex != (int)this.good || sourceName == null || !this.positions.TryGetValue(sourceName, out mainPosition))
                return false;

            textureSheet = this.spriteSheet;
            iconPosition = sourceIndex != -1 ? Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, sourceIndex, 16, 16): Rectangle.Empty;

            return true;
        }
    }
}