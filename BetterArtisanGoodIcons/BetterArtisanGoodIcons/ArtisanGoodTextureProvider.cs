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
        private bool GetSourceName(SObject item, int sourceIndex, out string sourceName)
        {
            //If the item name is equivalent to the base good, return _Base.
            if (item.Name == this.good.ToString())
            {
                sourceName = "_Base";
                return true;
            }

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
            //Use preservedParentSheetIndex for wine, jelly, pickles, and juice
            if (item.preservedParentSheetIndex.Value != 0)
            {
                index = item.preservedParentSheetIndex.Value;
                return true;
            }

            //Use honeyType for honey
            //Hardcode Wild honey using the sunflower image, since it is not called "Sunflower honey"
            if (this.good == ArtisanGood.Honey && item.honeyType.Value.HasValue)
            {
                index = item.honeyType.Value.Value != SObject.HoneyType.Wild ? (int)item.honeyType.Value.Value : 421;
                return true;
            }

            index = -1;
            return false;
        }

        /// <summary>Gets the info needed to draw the right texture for the given item.</summary>
        internal bool GetDrawInfo(SObject item, ref Texture2D textureSheet, ref Rectangle mainPosition, ref Rectangle iconPosition)
        {
            //TODO: This actually disallows changing the base texture b/c it won't get past the second if statement,
            //TODO: also the != -1 check will also be false.

            //Only yield new textures for base items. If removed, everything *should* still work, but it needs more testing.
            if (item.ParentSheetIndex != (int)this.good)
                return false;

            //If the index of the source item can't be found, exit.
            if (!this.GetIndexOfSource(item, out int sourceIndex))
                return false;

            //CFR likes to set the preservedParentSheetIndex to the negative of the source parentSheetIndex, for some reason.
            if (sourceIndex < 0)
                sourceIndex *= -1;

            //Get the name of the item from its index, and from that, a new sprite.
            if (!this.GetSourceName(item, sourceIndex, out string sourceName) || !this.positions.TryGetValue(sourceName, out mainPosition))
                return false;

            textureSheet = this.spriteSheet;
            iconPosition = sourceIndex != -1
                ? Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, sourceIndex, 16, 16)
                : Rectangle.Empty;
            return true;
        }
    }
}