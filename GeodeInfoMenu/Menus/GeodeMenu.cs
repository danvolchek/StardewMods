using GeodeInfoMenu.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeodeInfoMenu
{
    /// <summary>The main geode info menu.</summary>
    internal class GeodeMenu : IClickableMenu
    {
        /***
         * New Fields
         ***/

        /// <summary>The icons to draw as the tabs.</summary>
        public static Texture2D tabIcons;

        /// <summary>The mod config.</summary>
        private readonly ModConfig config;

        private readonly ModEntry modEntry;

        /***
         * Existing Fields
         ***/
        private string hoverText = "";
        private readonly List<ClickableComponent> tabs = new List<ClickableComponent>();
        private readonly List<IClickableMenu> pages = new List<IClickableMenu>();

        //all items, reg geode, frozen geode, magma geode, omni geode, artifact trove
        //15960    , 15961    , 15962       , 15963      , 15964     , 15965
        public int currentTab;

        public bool invisible;
        public static bool forcePreventClose;
        private bool wasSearchTextBoxSelectedWhenPageLeft;

        public GeodeMenu(ModEntry mod, ModConfig config, IList<Tuple<int[], bool[]>> items, GeodeMenuStateInfo savedState = null, bool forceReloadState = false)
      : base(Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true)
        {
            this.config = config;
            this.modEntry = mod;
            this.wasSearchTextBoxSelectedWhenPageLeft = false;

            this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize), "search", "Search for Drops")
            {
                myID = 15960,
                downNeighborID = 0,
                rightNeighborID = 15961,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            this.pages.Add((IClickableMenu)new SearchTab(mod, config, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
            this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 2, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize), "normal", "Geode")
            {
                myID = 15961,
                downNeighborID = 1,
                rightNeighborID = 15962,
                leftNeighborID = 15960,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            this.pages.Add((IClickableMenu)new GeodeTab("geode", items[0], this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
            this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 3, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize), "frozen", "Frozen Geode")
            {
                myID = 15962,
                downNeighborID = 2,
                rightNeighborID = 15963,
                leftNeighborID = 15961,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            this.pages.Add((IClickableMenu)new GeodeTab("frozen geode", items[1], this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
            this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 4, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize), "magma", "Magma Geode")
            {
                myID = 15963,
                downNeighborID = 3,
                rightNeighborID = 15964,
                leftNeighborID = 15962,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            this.pages.Add((IClickableMenu)new GeodeTab("magma geode", items[2], this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
            this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 5, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize), "omni", "Omni Geode")
            {
                myID = 15964,
                downNeighborID = 4,
                rightNeighborID = 15965,
                leftNeighborID = 15963,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            this.pages.Add((IClickableMenu)new GeodeTab("omni geode", items[3], this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));
            this.tabs.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 6, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize), "trove", "Artifact Trove")
            {
                myID = 15965,
                downNeighborID = 5,
                leftNeighborID = 15964,
                tryDefaultIfNoDownNeighborExists = true,
                fullyImmutable = true
            });
            this.pages.Add((IClickableMenu)new GeodeTab("artifact trove", items[4], this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height));

            if (Game1.activeClickableMenu == null)
                Game1.playSound("bigSelect");

            GeodeMenu.forcePreventClose = false;

            if (savedState != null)
            {
                (this.pages[0] as SearchTab).SetSearchBoxText(savedState.searchText);
                if (config.IfRememberingMenuStateAlsoRememberScrollBarPositions || forceReloadState)
                    for (int i = 0; i < 5; i++)
                        (this.pages[i] as IScrollableMenu).SetCurrentIndex(savedState.scrollBarIndicies[i]);
            }

            if (Game1.options.SnappyMenus)
            {
                this.pages[this.currentTab].populateClickableComponentList();
                this.pages[this.currentTab].allClickableComponents.AddRange((IEnumerable<ClickableComponent>)this.tabs);
                this.snapToDefaultClickableComponent();
            }

            if (savedState != null)
                this.ChangeTab(savedState.currentTab);
        }

        public GeodeMenu(int startingTab, ModEntry entry, ModConfig config, IList<Tuple<int[], bool[]>> items) : this(entry, config, items)
        {
            this.ChangeTab(startingTab);
        }

        /// <summary>Exists the menu if it was not opened during the geode cracking menu, otherwise opens the geode cracking menu.</summary>
        private void Exit()
        {
            IClickableMenu lastMenu = this.modEntry.GetLastMenu();
            if (lastMenu is StardewValley.Menus.GeodeMenu)
            {
                this.modEntry.SaveMenuState(this);
                Game1.activeClickableMenu = lastMenu;
            }
            else
                Game1.exitActiveMenu();
        }

        /***
         * New Public Methods
         ***/

        /// <summary>Saved this menu's state.</summary>
        /// <returns>The saved state.</returns>
        public GeodeMenuStateInfo SaveState()
        {
            int[] indicies = new int[this.pages.Count()];
            for (int i = 0; i < this.pages.Count(); i++)
                indicies[i] = (this.pages[i] as IScrollableMenu).GetCurrentIndex();
            return new GeodeMenuStateInfo((this.pages[0] as SearchTab).GetSearchBoxText(), this.currentTab, indicies);
        }

        /// <summary>Sets whether the search tab search box is selected or not.</summary>
        /// <param name="b">Selected or not.</param>
        public void SetSearchTabSearchBoxSelectedStatus(bool b)
        {
            (this.pages[0] as SearchTab).SetSearchBoxSelectedStatus(b);
        }

        /***
         * Existing Public Methods
         ***/

        public static string GetLabelOfTabFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return Game1.content.LoadString("Search for Drops");

                case 1:
                    return Game1.content.LoadString("Geode");

                case 2:
                    return Game1.content.LoadString("Frozen Geode");

                case 3:
                    return Game1.content.LoadString("Magma Geode");

                case 4:
                    return Game1.content.LoadString("Omni Geode");

                default:
                    return "";
            }
        }

        public int GetTabNumberFromName(string name)
        {
            switch (name)
            {
                case "search":
                    return 0;

                case "normal":
                    return 1;

                case "frozen":
                    return 2;

                case "magma":
                    return 3;

                case "omni":
                    return 4;

                case "trove":
                    return 5;

                default:
                    throw new ArgumentException($"Unknown name ${name}");
            }
        }

        public void ChangeTab(int whichTab)
        {
            this.currentTab = this.GetTabNumberFromName(this.tabs[whichTab].name);
            this.width = 800 + IClickableMenu.borderWidth * 2;
            this.initializeUpperRightCloseButton();
            this.invisible = false;

            Game1.playSound("smallSelect");
            if (!Game1.options.SnappyMenus)
                return;
            this.pages[this.currentTab].populateClickableComponentList();
            this.pages[this.currentTab].allClickableComponents.AddRange((IEnumerable<ClickableComponent>)this.tabs);
            this.SetTabNeighborsForCurrentPage();
            this.snapToDefaultClickableComponent();
        }

        public void SetTabNeighborsForCurrentPage()
        {
            foreach (ClickableComponent tab in this.tabs)
                tab.downNeighborID = -99999;
        }

        /***
         * Public Overrides
         ***/

        public override void snapToDefaultClickableComponent()
        {
            if (this.currentTab < this.pages.Count)
                this.pages[this.currentTab].snapToDefaultClickableComponent();

            return;
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b == Buttons.RightTrigger)
            {
                if (this.currentTab >= 7 || !this.pages[this.currentTab].readyToClose())
                    return;
                this.ChangeTab(this.currentTab + 1);
            }
            else if (b == Buttons.LeftTrigger)
            {
                if (this.currentTab <= 0 || !this.pages[this.currentTab].readyToClose())
                    return;
                this.ChangeTab(this.currentTab - 1);
            }
            else
            {
                if (b != Buttons.Back || this.currentTab != 0)
                    return;
                this.pages[this.currentTab].receiveGamePadButton(b);
            }
        }

        public override void setUpForGamePadMode()
        {
            base.setUpForGamePadMode();
            if (this.pages.Count <= this.currentTab)
                return;
            this.pages[this.currentTab].setUpForGamePadMode();
        }

        public override ClickableComponent getCurrentlySnappedComponent()
        {
            return this.pages[this.currentTab].getCurrentlySnappedComponent();
        }

        public override void setCurrentlySnappedComponentTo(int id)
        {
            this.pages[this.currentTab].setCurrentlySnappedComponentTo(id);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");
                this.Exit();
                return;
            }

            if (!this.invisible && !GeodeMenu.forcePreventClose)
            {
                for (int index = 0; index < this.tabs.Count; ++index)
                {
                    if (this.tabs[index].containsPoint(x, y) && this.currentTab != index && (this.currentTab == 0 || this.pages[this.currentTab].readyToClose()))
                    {
                        int newNumber = this.GetTabNumberFromName(this.tabs[index].name);
                        if (this.currentTab == 0)
                        {
                            this.wasSearchTextBoxSelectedWhenPageLeft = (this.pages[0] as SearchTab).GetSearchBoxSelectedStatus();
                            (this.pages[0] as SearchTab).SetSearchBoxSelectedStatus(false);
                        }
                        else if (newNumber == 0)
                        {
                            if (this.wasSearchTextBoxSelectedWhenPageLeft)
                                (this.pages[0] as SearchTab).SetSearchBoxSelectedStatus(true);
                        }
                        this.ChangeTab(newNumber);
                        return;
                    }
                }
            }
            this.pages[this.currentTab].receiveLeftClick(x, y, true);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.pages[this.currentTab].receiveRightClick(x, y, playSound);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            this.pages[this.currentTab].receiveScrollWheelAction(direction);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverText = "";
            this.pages[this.currentTab].performHoverAction(x, y);
            foreach (ClickableComponent tab in this.tabs)
            {
                if (tab.containsPoint(x, y))
                {
                    this.hoverText = tab.label;
                    return;
                }
            }
            return;
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            this.pages[this.currentTab].releaseLeftClick(x, y);
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            this.pages[this.currentTab].leftClickHeld(x, y);
        }

        public override bool readyToClose()
        {
            if (!GeodeMenu.forcePreventClose)
                return this.pages[this.currentTab].readyToClose();
            return false;
        }

        public override bool showWithoutTransparencyIfOptionIsSet()
        {
            return true;
        }

        public override void draw(SpriteBatch b)
        {
            if (!this.invisible)
            {
                if (!Game1.options.showMenuBackground)
                    b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
                Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.pages[this.currentTab].width, this.pages[this.currentTab].height, false, true, (string)null, false);
                this.pages[this.currentTab].draw(b);
                b.End();
                b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                if (!GeodeMenu.forcePreventClose)
                {
                    foreach (ClickableComponent tab in this.tabs)
                    {
                        int num = this.GetTabNumberFromName(tab.name);

                        b.Draw(GeodeMenu.tabIcons, new Vector2((float)tab.bounds.X, (float)(tab.bounds.Y + (this.currentTab == this.GetTabNumberFromName(tab.name) ? 8 : 0))), new Rectangle?(new Rectangle(num * 16, 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.0001f);
                    }
                    b.End();
                    b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

                    if (!this.hoverText.Equals(""))
                        IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
                }
            }
            else
                this.pages[this.currentTab].draw(b);
            if (!GeodeMenu.forcePreventClose)
                base.draw(b);
            if (Game1.options.hardwareCursor)
                return;
            b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getOldMouseX(), (float)Game1.getOldMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }

        public override bool areGamePadControlsImplemented()
        {
            return false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (((IEnumerable<InputButton>)Game1.options.menuButton).Contains<InputButton>(new InputButton(key)) && this.readyToClose())
            {
                this.Exit();
                Game1.playSound("bigDeSelect");
                return;
            }

            if (this.config.PressingEscapeWhileTypingInSearchBoxInstantlyClosesMenu && key == Keys.Escape)
            {
                this.Exit();
                Game1.playSound("bigDeSelect");
                return;
            }
            else
                this.pages[this.currentTab].receiveKeyPress(key);
        }
    }
}
