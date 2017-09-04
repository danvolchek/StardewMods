using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeodeInfoMenu.Menus
{
    /// <summary>
    /// Represents a search tab.
    /// </summary>
    public class SearchTab : IClickableMenu, IScrollableMenu
    {
        /***
         * New fields
         ***/
        /// <summary>
        /// The search text box.
        /// </summary>
        private UpdatingTextBox searchBox;
        /// <summary>
        /// All search results that were found.
        /// </summary>
        private IList<OptionsElement> searchResults;
        /// <summary>
        /// Bounds of each visible search result.
        /// </summary>
        List<ClickableComponent> visibleSearchResults;
        /// <summary>
        /// The header text.
        /// </summary>
        private OptionsElement headerText;
        /// <summary>
        /// Delegate definition to be called when search box text changes.
        /// </summary>
        public delegate void TextChangedDelegate();
        /// <summary>
        /// Where to draw header text.
        /// </summary>
        private Point headerBounds;
        /// <summary>
        /// Mod config.
        /// </summary>
        private GeodeInfoMenuConfig config;
        /// <summary>
        /// Mod itself to create search results.
        /// </summary>
        private GeodeInfoMenuMod mod;

        /***
         * Existing Fields
         ***/
        private string hoverText = "";
        private int optionsSlotHeld = -1;
        int currentItemIndex;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private const int NUM_ITEMS = 6;

        /// <summary>
        /// Constructs an instance.
        /// </summary>
        /// <param name="mod">Mod to use to create search results</param>
        /// <param name="config">Mod config</param>
        /// <param name="x">X position of tab</param>
        /// <param name="y">Y position of tab</param>
        /// <param name="width">Tab width</param>
        /// <param name="height">Tab height</param>
        public SearchTab(GeodeInfoMenuMod mod, GeodeInfoMenuConfig config, int x, int y, int width, int height) : base(x, y, width, height, false)
        {
            this.config = config;
            this.mod = mod;
            searchResults = new List<OptionsElement>();
            visibleSearchResults = new List<ClickableComponent>();
            headerText = new OptionsElement("Search for a drop: ");
            this.headerBounds = new Point(this.xPositionOnScreen + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom);
            searchBox = new UpdatingTextBox(new TextChangedDelegate(SearchBoxTextChanged), Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
            {
                X = this.xPositionOnScreen + Game1.tileSize / 4 + Game1.tileSize * 8,
                Y = this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom + Game1.tileSize / 2,
                Text = ""
            };

            this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), (float)Game1.pixelZoom, false);
            this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), (float)Game1.pixelZoom, false);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + Game1.pixelZoom * 3, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), (float)Game1.pixelZoom, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, this.scrollBar.bounds.Width, height - Game1.tileSize * 2 - this.upArrow.bounds.Height - Game1.pixelZoom * 2);
            //height -= searchBox.Height + 20;
            for (int index = 0; index < SearchTab.NUM_ITEMS; ++index)
            {
                List<ClickableComponent> optionSlots = this.visibleSearchResults;
                ClickableComponent clickableComponent = new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom + (index + 1) * ((height - Game1.tileSize * 2) / (NUM_ITEMS + 1)), width - Game1.tileSize / 2, (height - Game1.tileSize * 2) / (NUM_ITEMS + 1) + Game1.pixelZoom), string.Concat((object)index));
                clickableComponent.myID = index;
                int num1 = index < SearchTab.NUM_ITEMS - 1 ? index + 1 : -7777;
                clickableComponent.downNeighborID = num1;
                int num2 = index > 0 ? index - 1 : -7777;
                clickableComponent.upNeighborID = num2;
                int num3 = 1;
                clickableComponent.fullyImmutable = num3 != 0;
                optionSlots.Add(clickableComponent);
            }

        }

        /// <summary>
        /// Called when the search box text is changed - re creates the search list.
        /// </summary>
        private void SearchBoxTextChanged()
        {
            this.searchResults.Clear();
            if (this.searchBox == null || this.searchBox.Text == "")
                return;
            IList<GeodeDrop> results = this.mod.GetItemsFromString(this.searchBox.Text.ToLower());
            foreach (GeodeDrop result in results)
                if (this.mod.GetInfoToBuildSearchResult(result, out Tuple<int, int>[] geodesToCrack, out bool showStar))
                    this.searchResults.Add(new SearchTabItem(result.ParentSheetIndex, geodesToCrack, showStar));

            this.currentItemIndex = 0;
            this.SetScrollBarToCurrentIndex();

        }

        /***
         * Getters/Setters
         ***/
        public string GetSearchBoxText()
        {
            return this.searchBox.Text;
        }

        public void SetCurrentIndex(int index)
        {
            this.currentItemIndex = index;
            this.SetScrollBarToCurrentIndex();
        }

        public int GetCurrentIndex()
        {
            return this.currentItemIndex;
        }

        public void SetSearchBoxSelectedStatus(bool b)
        {
            if (b)
                this.searchBox.SelectMe();
            else
                this.searchBox.Selected = b;
        }

        public bool GetSearchBoxSelectedStatus()
        {
            return this.searchBox.Selected;
        }

        public void SetSearchBoxText(string s)
        {
            this.searchBox.Text = s;
        }


        /***
         * Existing Methods
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
            if (this.searchResults.Count <= 0)
                return;
            this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.searchResults.Count - SearchTab.NUM_ITEMS + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + Game1.pixelZoom;
            if (this.currentItemIndex != this.searchResults.Count - SearchTab.NUM_ITEMS)
                return;
            this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - Game1.pixelZoom;
        }

        /***
         * Override Methods
         ***/

        public override void draw(SpriteBatch b)
        {
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            headerText.draw(b, headerBounds.X, headerBounds.Y);

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

            searchBox.Draw(b);

            for (int index = 0; index < this.visibleSearchResults.Count; ++index)
            {
                if (this.currentItemIndex >= 0 && this.currentItemIndex + index < this.searchResults.Count)
                    this.searchResults[this.currentItemIndex + index].draw(b, this.visibleSearchResults[index].bounds.X, this.visibleSearchResults[index].bounds.Y);
            }

            if (!GameMenu.forcePreventClose)
            {
                this.upArrow.draw(b);
                this.downArrow.draw(b);
                if (this.searchResults.Count > NUM_ITEMS)
                {
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, (float)Game1.pixelZoom, false);
                    this.scrollBar.draw(b);
                }
            }
            if (this.hoverText.Equals(""))
                return;
            IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);


        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentItemIndex = 0;
            base.snapToDefaultClickableComponent();
            this.currentlySnappedComponent = this.getComponentWithID(1);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override bool readyToClose()
        {
            return !this.searchBox.Selected;
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            base.customSnapBehavior(direction, oldRegion, oldID);
            if (oldID == SearchTab.NUM_ITEMS - 1 && direction == 2 && this.currentItemIndex < Math.Max(0, this.searchResults.Count - NUM_ITEMS))
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
                if (direction >= 0 || this.currentItemIndex >= Math.Max(0, this.searchResults.Count - NUM_ITEMS))
                    return;
                this.DownArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            if (this.currentlySnappedComponent != null && this.currentlySnappedComponent.myID < this.searchResults.Count)
            {
                if (this.searchResults[this.currentlySnappedComponent.myID + this.currentItemIndex] is OptionsInputListener)
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

        public override void gamePadButtonHeld(Buttons b)
        {
            base.gamePadButtonHeld(b);
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            //eventually needed for pinning items

            this.searchBox.Update();

            if (GameMenu.forcePreventClose)
                return;
            if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.searchResults.Count - NUM_ITEMS))
            {
                this.DownArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shwip");
            }

            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + Game1.tileSize * 2 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            this.currentItemIndex = Math.Max(0, Math.Min(this.searchResults.Count - NUM_ITEMS, this.currentItemIndex));
            for (int index = 0; index < this.visibleSearchResults.Count; ++index)
            {
                if (this.visibleSearchResults[index].bounds.Contains(x, y) && this.currentItemIndex + index < this.searchResults.Count && this.searchResults[this.currentItemIndex + index].bounds.Contains(x - this.visibleSearchResults[index].bounds.X, y - this.visibleSearchResults[index].bounds.Y))
                {
                    this.searchResults[this.currentItemIndex + index].receiveLeftClick(x - this.visibleSearchResults[index].bounds.X, y - this.visibleSearchResults[index].bounds.Y);
                    this.optionsSlotHeld = index;
                    break;
                }
            }
        }

        public override void leftClickHeld(int x, int y)
        {
        }

        public override void releaseLeftClick(int x, int y)
        {
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (config.RightClickOnOnlySearchBoxToClearText)
            {
                if (x >= this.searchBox.X && x <= this.searchBox.X + this.searchBox.Width)
                    if (y >= this.searchBox.Y && y <= this.searchBox.Y + this.searchBox.Height)
                    {
                        this.searchBox.Text = "";
                        this.searchBox.SelectMe();
                    }
            }
            else
            {
                this.searchBox.Text = "";
                this.searchBox.SelectMe();
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.searchBox.Selected && (key == Keys.Tab || key == Keys.Escape || key == Keys.Enter))
            {
                // this.farmnameBox.SelectMe();
                this.searchBox.Selected = false;
            }
            if (!Game1.options.SnappyMenus || Game1.options.doesInputListContain(Game1.options.menuButton, key) || ((IEnumerable<Keys>)Keyboard.GetState().GetPressedKeys()).Count<Keys>() != 0)
                return;
            base.receiveKeyPress(key);
        }

        public override void performHoverAction(int x, int y)
        {
            this.searchBox.Hover(x, y);
            //eventually needed for unpin/pin item buttons
        }

    }
}
