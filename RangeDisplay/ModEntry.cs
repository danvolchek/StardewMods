using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RangeDisplay.Framework;
using RangeDisplay.Framework.RangeHandling;
using RangeDisplay.Framework.RangeHandling.RangeCreators;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace RangeDisplay
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The display manager.</summary>
        private DisplayManager displayManager;

        /// <summary>The hud message manager.</summary>
        private readonly HudMessageManager hudMessageManager = new HudMessageManager();

        /// <summary>Object range creators.</summary>
        private IList<IRangeCreator<SObject>> objectRangeCreators;

        /// <summary>Building range creators.</summary>
        private IList<IRangeCreator<Building>> buildingRangeCreators;

        /// <summary>Mod registry listeners.</summary>
        private IList<IModRegistryListener> modRegistryListeners;

        /// <summary>All possible range items.</summary>
        private readonly RangeItem[] allRangeItems = (RangeItem[])Enum.GetValues(typeof(RangeItem));

        /// <summary>The current display index.</summary>
        private int displayIndex = -1;

        /// <summary>The current active object.</summary>
        private SObject activeObject;

        /// <summary>Whether the modifier key is down or not.</summary>
        private bool isModifierKeyDown;

        /// <summary>The mod configuration.</summary>
        private ModConfig config;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.displayManager = new DisplayManager(helper.Content.Load<Texture2D>("assets/border.png"), helper.Content.Load<Texture2D>("assets/filled_in.png"),
                new Dictionary<RangeItem, Color>
                {
                    {RangeItem.Sprinkler, Color.LightSkyBlue},
                    {RangeItem.Scarecrow, Color.SaddleBrown},
                    {RangeItem.BeeHouse, Color.Yellow},
                    {RangeItem.JunimoHut, Color.LimeGreen}
                });

            SprinklerRangeCreator sprinklerRangeCreator = new SprinklerRangeCreator();
            ScarecrowRangeCreator scarecrowRangeCreator = new ScarecrowRangeCreator();
            this.objectRangeCreators = new List<IRangeCreator<SObject>>
            {
                sprinklerRangeCreator,
                scarecrowRangeCreator,
                new BeeHouseRangeCreator()
            };

            this.buildingRangeCreators = new List<IRangeCreator<Building>>
            {
                new JunimoHutRangeCreator()
            };

            this.modRegistryListeners = new List<IModRegistryListener>
            {
                sprinklerRangeCreator,
                scarecrowRangeCreator
            };

            this.config = helper.ReadConfig<ModConfig>();

            helper.Events.Display.RenderingHud += this.OnRenderingHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            if (this.config.ShowRangeOfHeldItem || this.config.ShowRangeOfHoveredOverItem)
            {
                helper.Events.Input.CursorMoved += this.OnCursorMoved;
                if (this.config.ShowRangeOfHoveredOverItem)
                    helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            }

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            foreach (IModRegistryListener listener in this.modRegistryListeners)
            {
                listener.ModRegistryReady(this.Helper.ModRegistry);
            }
        }

        /// <summary>Updates the range the mod is currently showing.</summary>
        /// <param name="location">The location to search for objects in.</param>
        private void RefreshRangeItems(GameLocation location)
        {
            if (!Context.IsWorldReady || location == null)
                return;

            Vector2 mouseTile = new Vector2((Game1.getMouseX() + Game1.viewport.X) / Game1.tileSize, (Game1.getMouseY() + Game1.viewport.Y) / Game1.tileSize);

            this.displayManager.Clear();

            // Objects in the world
            foreach (KeyValuePair<Vector2, SObject> item in location.Objects.Pairs)
            {
                IRangeCreator<SObject> creator = this.objectRangeCreators.FirstOrDefault(elem => elem.CanCreateRangeFor(item.Value));

                if (creator != null)
                {
                    this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.CreateRange(item.Value, item.Key, location), mouseTile.Equals(item.Key) && this.isModifierKeyDown);
                }
            }

            // Buildings in the world
            if (location is BuildableGameLocation buildableGameLocation)
            {
                foreach (Building building in buildableGameLocation.buildings)
                {
                    IRangeCreator<Building> creator = this.buildingRangeCreators.FirstOrDefault(elem => elem.CanCreateRangeFor(building));

                    if (creator != null)
                    {
                        bool buildingContainsMouse = ModEntry.AreaContainsPoint(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value, (int)mouseTile.X, (int)mouseTile.Y);
                        this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.CreateRange(building, new Vector2(building.tileX.Value, building.tileY.Value), location), buildingContainsMouse && this.isModifierKeyDown);
                    }
                }
            }

            // Held item
            if (this.activeObject != null && this.config.ShowRangeOfHeldItem)
            {
                IRangeCreator<SObject> creator = this.objectRangeCreators.FirstOrDefault(elem => elem.CanCreateRangeFor(this.activeObject));

                if (creator != null)
                {
                    this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.CreateRange(this.activeObject, mouseTile, location), true);
                }
            }
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (this.config.HoverModifierKeys.Contains(e.Button) && this.config.ShowRangeOfHoveredOverItem)
            {
                this.isModifierKeyDown = false;
                this.RefreshRangeItems(Game1.currentLocation);
            }
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (this.activeObject != null && Game1.activeClickableMenu == null && this.config.ShowRangeOfHeldItem)
                this.RefreshRangeItems(Game1.currentLocation);
            else if (this.config.ShowRangeOfHoveredOverItem && this.isModifierKeyDown)
                this.RefreshRangeItems(Game1.currentLocation);
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null || !e.IsMultipleOf(8))
                return;

            this.RefreshRangeItems(Game1.currentLocation);

            this.activeObject = Game1.player.ActiveObject;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == this.config.CycleActiveDisplayKey)
            {
                if (this.displayIndex == this.allRangeItems.Length - 1)
                {
                    this.displayIndex++;
                    this.displayManager.DisplayAll(true);
                    this.hudMessageManager.AddHudMessage("All");
                    return;
                }
                else if (this.displayIndex == this.allRangeItems.Length)
                {
                    this.displayIndex++;
                    this.displayManager.DisplayAll(false);
                    this.hudMessageManager.AddHudMessage("None");
                    return;
                }

                if (this.displayIndex == this.allRangeItems.Length + 1)
                {
                    this.displayIndex = -1;
                }

                this.displayIndex = (this.displayIndex + 1) % this.allRangeItems.Length;
                this.displayManager.DisplayOnly(this.allRangeItems[this.displayIndex]);
                this.hudMessageManager.AddHudMessage(this.allRangeItems[this.displayIndex]);
            }
            else if (this.config.HoverModifierKeys.Contains(e.Button) && this.config.ShowRangeOfHoveredOverItem)
            {
                this.isModifierKeyDown = true;
                this.RefreshRangeItems(Game1.currentLocation);
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            this.displayManager.Draw(e.SpriteBatch);
        }

        /// <summary>Checks whether the area contains the given point.</summary>
        /// <param name="left">The left start of the area.</param>
        /// <param name="top">The top start of the area.</param>
        /// <param name="width">How wide the area is.</param>
        /// <param name="height">How tall the area is.</param>
        /// <param name="pointX">The x position of the point.</param>
        /// <param name="pointY">The y position of the point.</param>
        /// <returns>Whether the area contains the point.</returns>
        private static bool AreaContainsPoint(int left, int top, int width, int height, int pointX, int pointY)
        {
            return pointX >= left && pointX < left + width && pointY >= top && pointY < top + height;
        }
    }
}
