using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;

namespace GeodeInfoMenu.Menus
{
    /// <summary>
    /// Represents an item show in the search tab.
    /// </summary>
    class SearchTabItem : GeodeTabItem
    {
        /// <summary>
        /// The geodes that can be broken and how many of them it will take to get this item.
        /// </summary>
        Tuple<int, int>[] breakableGeodes;

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="parentSheetIndex">This items item id</param>
        /// <param name="breakableGeodes">The geodes that can be broken to get this item</param>
        /// <param name="drawStar">Whether to draw a star or not</param>
        public SearchTabItem(int parentSheetIndex, Tuple<int, int>[] breakableGeodes, bool drawStar = false) : base(parentSheetIndex, -1, drawStar)
        {
            this.breakableGeodes = breakableGeodes;
        }

        /// <summary>
        /// Draws this item on screen.
        /// </summary>
        /// <param name="b">The sprite batch to use</param>
        /// <param name="slotX">X position of where to draw the item</param>
        /// <param name="slotY">Y position of where to draw the item</param>
        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            base.draw(b, slotX, slotY);
            float scaleSize = 1.5f;
            for (int i = this.breakableGeodes.Length - 1; i > -1; i--)
            {
                int numItems = this.breakableGeodes[i].Item2;
                int xPos = slotX + this.bounds.X + SPRITE_SIZE * 2 + (int)(SPRITE_SIZE * 1.5 * (i + (4 - this.breakableGeodes.Length))) + 300;
                int yPos = slotY + this.bounds.Y;
                b.Draw(Game1.objectSpriteSheet, new Rectangle(xPos, yPos, SPRITE_SIZE, SPRITE_SIZE), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.breakableGeodes[i].Item1, 16, 16)), Color.White);
            }

            for (int i = 0; i < this.breakableGeodes.Length; i++)
            {
                int numItems = this.breakableGeodes[i].Item2;
                int xPos = slotX + this.bounds.X + SPRITE_SIZE * 2 + (int)(SPRITE_SIZE * 1.5 * (i + (4 - this.breakableGeodes.Length))) + 300;
                int yPos = slotY + this.bounds.Y;
                Utility.drawTinyDigits(numItems, b, new Vector2((float)(xPos + Game1.tileSize - Utility.getWidthOfTinyDigitString(numItems, 3f * scaleSize)) + 3f * scaleSize, (float)(yPos + (double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
            }

        }
    }
}
