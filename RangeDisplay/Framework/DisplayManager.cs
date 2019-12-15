using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RangeDisplay.Framework.RangeHandling;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RangeDisplay.Framework
{
    /// <summary>Draws ranges.</summary>
    internal class DisplayManager
    {
        /*********
        ** Fields
        *********/

        /// <summary>The border texture.</summary>
        private readonly Texture2D border;

        /// <summary>The filled in texture.</summary>
        private readonly Texture2D filledIn;

        /// <summary>A mapping from an item to the color it should show as.</summary>
        private readonly IReadOnlyDictionary<RangeItem, Color> colorMapping;

        /// <summary>The range items to draw for each tile.</summary>
        private readonly IDictionary<Vector2, ISet<RangeItem>> tilesToDraw = new Dictionary<Vector2, ISet<RangeItem>>();

        /// <summary>The range items to force draw for each tile. This item is shown instead of any others on that tile, even if it shouldn't be displayed.</summary>
        private readonly IDictionary<Vector2, RangeItem> tilesToForceDraw = new Dictionary<Vector2, RangeItem>();

        /// <summary>Whether an item should be shown.</summary>
        private readonly IDictionary<RangeItem, bool> shouldDisplayRangeItem = new Dictionary<RangeItem, bool>();

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="border">The border texture</param>
        /// <param name="filledIn">The filled in texture.</param>
        /// <param name="colorMapping">A mapping from an item to the color it should show as.</param>
        public DisplayManager(Texture2D border, Texture2D filledIn, IReadOnlyDictionary<RangeItem, Color> colorMapping)
        {
            this.border = border;
            this.filledIn = filledIn;
            this.colorMapping = colorMapping;

            foreach (RangeItem item in Enum.GetValues(typeof(RangeItem)))
            {
                this.shouldDisplayRangeItem[item] = false;
            }
        }

        /// <summary>Adds tiles to be drawn on screen.</summary>
        /// <param name="rangeItem">The item the tiles are for.</param>
        /// <param name="tileRange">Where on screen to draw the tiles.</param>
        /// <param name="force">Whether the tiles should be force drawn.</param>
        public void AddTilesToDisplay(RangeItem rangeItem, IEnumerable<Vector2> tileRange, bool force = false)
        {
            foreach (Vector2 tile in tileRange)
            {
                if (this.tilesToDraw.TryGetValue(tile, out ISet<RangeItem> items))
                {
                    items.Add(rangeItem);
                }
                else
                {
                    this.tilesToDraw[tile] = new HashSet<RangeItem>() { rangeItem };
                }

                if (force)
                {
                    this.tilesToForceDraw[tile] = rangeItem;
                }
            }
        }

        /// <summary>Draws tiles onto the given sprite batch.</summary>
        /// <param name="spriteBatch">The sprite batch to draw on.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // If there's nothing to draw, don't do anything.
            if (this.shouldDisplayRangeItem.Count(item => item.Value) == 0 && this.tilesToForceDraw.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<Vector2, ISet<RangeItem>> tileItem in this.tilesToDraw)
            {
                bool force = this.tilesToForceDraw.TryGetValue(tileItem.Key, out RangeItem forceItem);

                int numToDisplay = force ? 1 : Math.Min(this.shouldDisplayRangeItem.Count(item => item.Value), tileItem.Value.Count);

                int width = (int)(Game1.tileSize * (1.0 / numToDisplay));

                Vector2 screenPosition = this.ConvertTilePositionToScreenCoordinates(tileItem.Key);

                int i = 0;
                foreach (RangeItem currentItem in Enum.GetValues(typeof(RangeItem)))
                {
                    if (!tileItem.Value.Contains(currentItem) || (force && currentItem != forceItem) || (!force && !this.shouldDisplayRangeItem[currentItem]))
                    {
                        continue;
                    }

                    int x = (int)(screenPosition.X + i * width);
                    if (i == numToDisplay - 1 && x + width - screenPosition.X < Game1.tileSize)
                        width++;
                    i++;
                    spriteBatch.Draw(this.filledIn, new Rectangle(x, (int)screenPosition.Y, width, Game1.tileSize), new Rectangle(0, 0, width, Game1.tileSize), this.colorMapping[currentItem]);
                }

                if (i != 0)
                    spriteBatch.Draw(this.border, new Rectangle((int)screenPosition.X, (int)screenPosition.Y, Game1.tileSize, Game1.tileSize), new Color(0, 0, 0, 0.1f));
            }
        }

        /// <summary>Display only the given item.</summary>
        /// <param name="item">The item to display.</param>
        public void DisplayOnly(RangeItem item)
        {
            foreach (RangeItem key in this.shouldDisplayRangeItem.Keys.ToList())
            {
                this.shouldDisplayRangeItem[key] = key == item;
            }
        }

        /// <summary>Display or hide every item.</summary>
        /// <param name="shouldDisplay">Whether to display or hide all items.</param>
        public void DisplayAll(bool shouldDisplay)
        {
            foreach (RangeItem key in this.shouldDisplayRangeItem.Keys.ToList())
            {
                this.shouldDisplayRangeItem[key] = shouldDisplay;
            }
        }

        /// <summary>Discards all tile information.</summary>
        public void Clear()
        {
            this.tilesToDraw.Clear();
            this.tilesToForceDraw.Clear();
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Converts a tile position to screen coordinates.</summary>
        /// <param name="position">The tile position.</param>
        /// <returns>The screen coordinates.</returns>
        private Vector2 ConvertTilePositionToScreenCoordinates(Vector2 position)
        {
            return new Vector2((position.X * Game1.tileSize) - Game1.viewport.X, (position.Y * Game1.tileSize) - Game1.viewport.Y);
        }
    }
}
