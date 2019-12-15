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
        /// <summary>The sprite sheet to pull textures from.</summary>
        private readonly Texture2D spriteSheet;

        /// <summary>The rectangles that correspond to each item name.</summary>
        private readonly IDictionary<string, Rectangle> positions = new Dictionary<string, Rectangle>();

        /// <summary>The type of artisan good this provides texture for.</summary>
        private readonly ArtisanGood good;

        public ArtisanGoodTextureProvider(Texture2D texture, List<string> names, ArtisanGood good)
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
        private bool GetSourceName(int sourceIndex, out string sourceName)
        {
            //Lookup the name from the game's object information, or null if not found (a custom item that has its sourceIndex set incorrectly).
            if (Game1.objectInformation.TryGetValue(sourceIndex, out string information))
            {
                sourceName = information.Split('/')[0];
                return true;
            }

            sourceName = null;
            return false;
        }

        /// <summary>Gets the index of the source item used to create the given item name.</summary>
        private bool GetIndexOfSource(SObject item, out int index)
        {
            if (item.preservedParentSheetIndex.Value != 0)
            {
                index = item.preservedParentSheetIndex.Value;

                // CFR sets the preservedParentSheetIndex to the negative of the real value, for some reason.
                if (index < -1)
                {
                    index *= -1;
                }
                return true;
            }

            index = -1;
            return false;
        }

        private bool GetSourceName(SObject item, out int sourceIndex, out string sourceName)
        {
            sourceName = null;

            //If the item name is equivalent to the base good, return _Base.
            if (item.Name == this.good.ToString())
            {
                sourceName = "_Base";
                sourceIndex = (int)this.good;
                return true;
            }

            if (!this.GetIndexOfSource(item, out sourceIndex))
            {
                return false;
            }

            //If we're handling honey and the source index is -1, it's wild honey. Use the sunflower index for the icon position.
            if (this.good == ArtisanGood.Honey && sourceIndex == -1)
            {
                sourceIndex = 421; // Sunflower
                sourceName = "Wild";
                return true;
            }

            return this.GetSourceName(sourceIndex, out sourceName);
        }

        /// <summary>Gets the info needed to draw the right texture for the given item.</summary>
        public bool GetDrawInfo(SObject item, ref Texture2D textureSheet, ref Rectangle mainPosition, ref Rectangle iconPosition)
        {
            //Get the name of the item from its index, and from that, a new sprite.
            if (item.ParentSheetIndex != (int)this.good || !this.GetSourceName(item, out int sourceIndex, out string sourceName) || !this.positions.TryGetValue(sourceName, out mainPosition))
            {
                return false;
            }

            textureSheet = this.spriteSheet;
            iconPosition = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, sourceIndex, 16, 16);
            return true;
        }
    }
}
