using GeodeInfoMenu.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace GeodeInfoMenu
{
    /// <summary>
    /// Represents a tab that shows geode drops in a list.
    /// </summary>
    class GeodeTab : IClickableMenu, IScrollableMenu
    {
        /***
         * Existing Fields
         ***/
        private string hoverText = "";
        public List<ClickableComponent> optionSlots = new List<ClickableComponent>();
        private readonly List<OptionsElement> options = new List<OptionsElement>();
        private int optionsSlotHeld = -1;
        public int currentItemIndex;
        private readonly ClickableTextureComponent upArrow;
        private readonly ClickableTextureComponent downArrow;
        private readonly ClickableTextureComponent scrollBar;
        private bool scrolling;
        private Rectangle scrollBarRunner;
        private const int NUM_ITEMS = 6;

        /***
         * New Fields
         ***/
        /// <summary>
        /// Where to draw the header text.
        /// </summary>
        private readonly Point headerBounds;
        /// <summary>
        /// The header itself.
        /// </summary>
        private readonly OptionsElement header;

        /// <summary>
        /// The name of the geode this tab represents.
        /// </summary>
        private string name;

        /// <summary>
        /// Constructs an instance.
        /// </summary>
        /// <param name="name">The name of this tab</param>
        /// <param name="items">The items this tab should show</param>
        /// <param name="x">X position of the tab</param>
        /// <param name="y">Y position of the tab</param>
        /// <param name="width">Tab width</param>
        /// <param name="height">Tab height</param>
        public GeodeTab(string name, Tuple<int[], bool[]> items, int x, int y, int width, int height)
      : base(x, y, width, height, true)
        {
            this.name = name;
            this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), (float)Game1.pixelZoom, false);
            this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), (float)Game1.pixelZoom, false);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + Game1.pixelZoom * 3, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), (float)Game1.pixelZoom, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, this.scrollBar.bounds.Width, height - Game1.tileSize * 2 - this.upArrow.bounds.Height - Game1.pixelZoom * 2);
            this.headerBounds = new Point(this.xPositionOnScreen + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom);
            for (int index = 0; index < GeodeTab.NUM_ITEMS; ++index)
            {
                List<ClickableComponent> optionSlots = this.optionSlots;
                ClickableComponent clickableComponent = new ClickableComponent(
                    new Rectangle(this.xPositionOnScreen + Game1.tileSize / 4,
                        this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom +
                        (index + 1) * ((height - Game1.tileSize * 2) / (NUM_ITEMS + 1)), width - Game1.tileSize / 2,
                        (height - Game1.tileSize * 2) / (NUM_ITEMS + 1) + Game1.pixelZoom),
                    string.Concat((object) index)) {myID = index};
                int num1 = index < GeodeTab.NUM_ITEMS - 1 ? index + 1 : -7777;
                clickableComponent.downNeighborID = num1;
                int num2 = index > 0 ? index - 1 : -7777;
                clickableComponent.upNeighborID = num2;
                int num3 = 1;
                clickableComponent.fullyImmutable = num3 != 0;
                optionSlots.Add(clickableComponent);
            }
            this.header = new OptionsElement("Next " + items.Item1.Length + " " + name + " drops: ");
            if (items != null)
                for (int i = 0; i < items.Item1.Length; i++)
                    this.options.Add(new GeodeTabItem(items.Item1[i], i + 1, items.Item2[i]));

        }

        /***
         * Existing Public Methods
         ***/

        public void SetCurrentIndex(int index)
        {
            this.currentItemIndex = index;
            this.SetScrollBarToCurrentIndex();
        }

        public int GetCurrentIndex()
        {
            return this.currentItemIndex;
        }

        /***
         * Existing Private Methods
         ***/

        private void DownArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            this.currentItemIndex = this.currentItemIndex + 1;
            this.SetScrollBarToCurrentIndex();
        }

        private void UpArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            this.currentItemIndex = this.currentItemIndex - 1;
            this.SetScrollBarToCurrentIndex();
        }

        private void SetScrollBarToCurrentIndex()
        {
            if (this.options.Count <= 0)
                return;
            this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.options.Count - GeodeTab.NUM_ITEMS + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + Game1.pixelZoom;
            if (this.currentItemIndex != this.options.Count - GeodeTab.NUM_ITEMS)
                return;
            this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - Game1.pixelZoom;
        }

        /***
         * Existing Overrides
         ***/

        public override void snapToDefaultClickableComponent()
        {
            this.currentItemIndex = 0;
            base.snapToDefaultClickableComponent();
            this.currentlySnappedComponent = this.getComponentWithID(1);
            this.snapCursorToCurrentSnappedComponent();
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            base.customSnapBehavior(direction, oldRegion, oldID);
            if (oldID == GeodeTab.NUM_ITEMS - 1 && direction == 2 && this.currentItemIndex < Math.Max(0, this.options.Count - NUM_ITEMS))
            {
                this.DownArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (oldID != 0 || direction != 0)
                    return;
                if (this.currentItemIndex > 0)
                {
                    this.UpArrowPressed();
                    Game1.playSound("shiny4");
                }
                else
                {
                    this.currentlySnappedComponent = this.getComponentWithID(12346);
                    if (this.currentlySnappedComponent != null)
                        this.currentlySnappedComponent.downNeighborID = 0;
                    this.snapCursorToCurrentSnappedComponent();
                }
            }
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            if (this.currentlySnappedComponent != null && this.currentlySnappedComponent.myID < this.options.Count)
            {
                if (this.options[this.currentlySnappedComponent.myID + this.currentItemIndex] is OptionsInputListener)
                    Game1.setMousePosition(this.currentlySnappedComponent.bounds.Right - Game1.tileSize * 3 / 4, this.currentlySnappedComponent.bounds.Center.Y - Game1.pixelZoom * 3);
                else
                    Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + Game1.tileSize * 3 / 4, this.currentlySnappedComponent.bounds.Center.Y - Game1.pixelZoom * 3);
            }
            else
            {
                if (this.currentlySnappedComponent == null)
                    return;
                base.snapCursorToCurrentSnappedComponent();
            }
        }

        public override ClickableComponent getCurrentlySnappedComponent()
        {
            return this.currentlySnappedComponent;
        }

        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.leftClickHeld(x, y);
            if (this.scrolling)
            {
                int y1 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - Game1.tileSize - Game1.pixelZoom * 3 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + Game1.pixelZoom * 5));
                this.currentItemIndex = Math.Min(this.options.Count - NUM_ITEMS, Math.Max(0, (int)((double)this.options.Count * (double)((float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height))));
                this.SetScrollBarToCurrentIndex();
                int y2 = this.scrollBar.bounds.Y;
                if (y1 == y2)
                    return;
                Game1.playSound("shiny4");
            }
            else
            {
                if (this.optionsSlotHeld == -1 || this.optionsSlotHeld + this.currentItemIndex >= this.options.Count)
                    return;
                this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickHeld(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
            }
        }

        public override void setCurrentlySnappedComponentTo(int id)
        {
            this.currentlySnappedComponent = this.getComponentWithID(id);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count || Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                if (this.currentlySnappedComponent != null && Game1.options.snappyMenus && (Game1.options.gamepadControls && this.options.Count > this.currentItemIndex + this.currentlySnappedComponent.myID) && this.currentItemIndex + this.currentlySnappedComponent.myID >= 0)
                    this.options[this.currentItemIndex + this.currentlySnappedComponent.myID].receiveKeyPress(key);
                else if (this.options.Count > this.currentItemIndex + this.optionsSlotHeld && this.currentItemIndex + this.optionsSlotHeld >= 0)
                    this.options[this.currentItemIndex + this.optionsSlotHeld].receiveKeyPress(key);
            }
            base.receiveKeyPress(key);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.currentItemIndex >= Math.Max(0, this.options.Count - NUM_ITEMS))
                    return;
                this.DownArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.releaseLeftClick(x, y);
            if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
                this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickReleased(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
            this.optionsSlotHeld = -1;
            this.scrolling = false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (GameMenu.forcePreventClose)
                return;
            if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.options.Count - NUM_ITEMS))
            {
                this.DownArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.scrollBar.containsPoint(x, y))
                this.scrolling = true;
            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + Game1.tileSize * 2 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            this.currentItemIndex = Math.Max(0, Math.Min(this.options.Count - NUM_ITEMS, this.currentItemIndex));
            for (int index = 0; index < this.optionSlots.Count; ++index)
            {
                if (this.optionSlots[index].bounds.Contains(x, y) && this.currentItemIndex + index < this.options.Count && this.options[this.currentItemIndex + index].bounds.Contains(x - this.optionSlots[index].bounds.X, y - this.optionSlots[index].bounds.Y))
                {
                    this.options[this.currentItemIndex + index].receiveLeftClick(x - this.optionSlots[index].bounds.X, y - this.optionSlots[index].bounds.Y);
                    this.optionsSlotHeld = index;
                    break;
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            for (int index = 0; index < this.optionSlots.Count; ++index)
            {
                if (this.currentItemIndex >= 0 && this.currentItemIndex + index < this.options.Count && this.options[this.currentItemIndex + index].bounds.Contains(x - this.optionSlots[index].bounds.X, y - this.optionSlots[index].bounds.Y))
                {
                    Game1.SetFreeCursorDrag();
                    break;
                }
            }
            if (this.scrollBarRunner.Contains(x, y))
                Game1.SetFreeCursorDrag();
            if (GameMenu.forcePreventClose)
                return;

            this.hoverText = "";
            this.upArrow.tryHover(x, y, 0.1f);
            this.downArrow.tryHover(x, y, 0.1f);
            this.scrollBar.tryHover(x, y, 0.1f);
            int num = this.scrolling ? 1 : 0;
        }

        public override void draw(SpriteBatch b)
        {
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            this.header.draw(b, this.headerBounds.X, this.headerBounds.Y);
            for (int index = 0; index < this.optionSlots.Count; ++index)
            {
                if (this.currentItemIndex >= 0 && this.currentItemIndex + index < this.options.Count)
                    this.options[this.currentItemIndex + index].draw(b, this.optionSlots[index].bounds.X, this.optionSlots[index].bounds.Y);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (!GameMenu.forcePreventClose)
            {
                this.upArrow.draw(b);
                this.downArrow.draw(b);
                if (this.options.Count > NUM_ITEMS)
                {
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, (float)Game1.pixelZoom, false);
                    this.scrollBar.draw(b);
                }
            }
            if (this.hoverText.Equals(""))
                return;
            IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
        }
    }


}
