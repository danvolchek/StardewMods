using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RangeDisplay.RangeHandling;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RangeDisplay
{
    internal class DisplayManager
    {
        private readonly Texture2D border;
        private readonly Texture2D filledin;
        private IReadOnlyDictionary<RangeItem, Color> colorMapping;
        private IDictionary<Vector2, ISet<RangeItem>> tilesToDraw = new Dictionary<Vector2, ISet<RangeItem>>();
        private IDictionary<Vector2, RangeItem> tilesToForceDraw = new Dictionary<Vector2, RangeItem>();
        private IDictionary<RangeItem, bool> shouldDisplayRangeItem = new Dictionary<RangeItem, bool>();

        public DisplayManager(Texture2D border, Texture2D filledin, IReadOnlyDictionary<RangeItem, Color> colorMapping)
        {
            this.border = border;
            this.filledin = filledin;
            this.colorMapping = colorMapping;

            foreach (RangeItem item in Enum.GetValues(typeof(RangeItem)))
                this.shouldDisplayRangeItem[item] = false;
        }

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
                    this.tilesToForceDraw[tile] = rangeItem;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (this.shouldDisplayRangeItem.Count(item => item.Value) == 0 && this.tilesToForceDraw.Count == 0)
                return;

            foreach (KeyValuePair<Vector2, ISet<RangeItem>> tileItem in this.tilesToDraw)
            {
                bool force = this.tilesToForceDraw.TryGetValue(tileItem.Key, out RangeItem forceItem);

                int numToDisplay = force ? 1 :
                    Math.Min(this.shouldDisplayRangeItem.Count(item => item.Value), tileItem.Value.Count);

                int width = (int)(Game1.tileSize * (1.0 / numToDisplay));
                int height = Game1.tileSize;

                Vector2 screenPosition = ConvertTilePositionToScreenCoordinates(tileItem.Key);

                int i = 0;
                foreach (RangeItem currentItem in Enum.GetValues(typeof(RangeItem)))
                {
                    if (!tileItem.Value.Contains(currentItem) || (force && currentItem != forceItem) || (!force && !this.shouldDisplayRangeItem[currentItem]))
                        continue;

                    int x = (int)(screenPosition.X + i * width);
                    if (i == numToDisplay - 1 && x + width - screenPosition.X < Game1.tileSize)
                        width++;
                    i++;
                    spriteBatch.Draw(this.filledin, new Rectangle(x, (int)screenPosition.Y, width, height), new Rectangle(0, 0, width, height), this.colorMapping[currentItem]);
                }

                if (i != 0)
                    spriteBatch.Draw(this.border, new Rectangle((int)screenPosition.X, (int)screenPosition.Y, Game1.tileSize, Game1.tileSize), new Color(0, 0, 0, 0.1f));
            }
        }

        public void DisplayOnly(RangeItem item)
        {
            for (int i = 0; i < this.shouldDisplayRangeItem.Count(); i++)
            {
                this.shouldDisplayRangeItem[this.shouldDisplayRangeItem.ElementAt(i).Key] = this.shouldDisplayRangeItem.ElementAt(i).Key == item;
            }
        }

        public void DisplayAll(bool shouldDisplay)
        {
            for (int i = 0; i < this.shouldDisplayRangeItem.Count(); i++)
            {
                this.shouldDisplayRangeItem[this.shouldDisplayRangeItem.ElementAt(i).Key] = shouldDisplay;
            }
        }

        public void Clear()
        {
            this.tilesToDraw.Clear();
            this.tilesToForceDraw.Clear();
        }

        private Vector2 ConvertTilePositionToScreenCoordinates(Vector2 position)
        {
            return new Vector2((position.X * Game1.tileSize) - Game1.viewport.X, (position.Y * Game1.tileSize) - Game1.viewport.Y);
        }
    }
}