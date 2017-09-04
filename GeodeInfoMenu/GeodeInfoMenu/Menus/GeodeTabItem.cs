using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using SObject = StardewValley.Object;
namespace GeodeInfoMenu.Menus
{
    //Represents an item show in a GeodeTab's list. Is simply an icon, name, and possibly number and star.
    class GeodeTabItem : OptionsElement
    {
        /***
         * Protected Fields
         ***/

        /// <summary>
        /// The item id of this item.
        /// </summary>
        protected int parentSheetIndex;

        /// <summary>
        /// The name of this item.
        /// </summary>
        protected string name;

        /// <summary>
        /// The size of the icon to draw for this item.
        /// </summary>
        protected const int SPRITE_SIZE = 64;

        /// <summary>
        /// A number to draw before this item, if it is in a numbered list.
        /// </summary>
        protected int numInFront;

        /// <summary>
        /// Whether to draw a star on this item's icon.
        /// </summary>
        protected bool drawStar;

        /// <summary>
        /// Where to draw this item.
        /// </summary>
        protected new Rectangle bounds;

        /***
         * Public Members
         ***/

        /// <summary>
        /// Constructs an instance.
        /// </summary>
        /// <param name="parentSheetIndex">The item id of this item</param>
        /// <param name="numInFront">The number to be shown before this item</param>
        /// <param name="drawStar">Whether to draw a star in front of this item</param>
        public GeodeTabItem(int parentSheetIndex, int numInFront = -1, bool drawStar = false) : base("")
        {
            this.name = Game1.objectInformation[parentSheetIndex].Split('/')[0];
            this.parentSheetIndex = parentSheetIndex;
            this.bounds = new Rectangle(8 * Game1.pixelZoom, 4 * Game1.pixelZoom, 9 * Game1.pixelZoom, 9 * Game1.pixelZoom);
            this.numInFront = numInFront;
            this.drawStar = drawStar;
        }
        
        /// <summary>
        /// Draws this item on the screen.
        /// </summary>
        /// <param name="b">The sprite batch to use</param>
        /// <param name="slotX">The x position of where to draw it</param>
        /// <param name="slotY">The y position of where to draw it</param>
        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (this.numInFront != -1)
            {
                SpriteText.drawString(b, this.numInFront + "", slotX + this.bounds.X, slotY + this.bounds.Y + Game1.pixelZoom * 3, 99, -1, 99, 1f, 0.1f, false, -1, "", -1);

            }
            b.Draw(Game1.objectSpriteSheet, new Rectangle((int)(slotX + this.bounds.X + Game1.tileSize), slotY + this.bounds.Y, SPRITE_SIZE, SPRITE_SIZE), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.parentSheetIndex, 16, 16)), Color.White);

            int scaleSize = 3;
            if (this.drawStar)
                b.Draw(Game1.mouseCursors, new Vector2(slotX + this.bounds.X + Game1.tileSize + 12f /*+ Game1.tileSize/2*/, slotY + this.bounds.Y + (float)(Game1.tileSize - 12) + 4), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), Color.White * 1, 0.0f, new Vector2(4f, 4f), scaleSize, SpriteEffects.None, 1f);
            SpriteText.drawString(b, this.name, slotX + this.bounds.X + SPRITE_SIZE + (int)(Game1.tileSize*1.5), slotY + this.bounds.Y + Game1.pixelZoom * 3, 999, -1, 999, 1f, 0.1f, false, -1, "", -1);
        }


    }
}
