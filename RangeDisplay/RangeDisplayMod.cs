using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RangeDisplay.RangeHandling;
using RangeDisplay.RangeHandling.RangeCreators.Buildings;
using RangeDisplay.RangeHandling.RangeCreators.Objects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace RangeDisplay
{
    public class RangeDisplayMod : Mod
    {
        private DisplayManager displayManager;
        private readonly HudMessageManager hudMessageManager = new HudMessageManager();
        private IList<IObjectRangeCreator> objectRangeCreators;
        private IList<IBuildingRangeCreator> buildingRangeCreators;

        private readonly RangeItem[] allRangeItems = (RangeItem[])Enum.GetValues(typeof(RangeItem));
        private int displayIndex = -1;

        private SObject activeObject = null;
        private bool isModifierKeyDown = false;

        private RangeDisplayConfig config;

        private SprinklerRangeCreator sprinklerRangeCreator;
        private ScarecrowRangeCreator scarecrowRangeCreator;

        //Need to fix rendering and item change (walking)
        public override void Entry(IModHelper helper)
        {
            this.displayManager = new DisplayManager(helper.Content.Load<Texture2D>("assets/border.png"), helper.Content.Load<Texture2D>("assets/filled_in.png"),
            new Dictionary<RangeItem, Color>()
            {
                { RangeItem.Sprinkler, Color.LightSkyBlue },
                { RangeItem.Scarecrow, Color.SaddleBrown },
                { RangeItem.Bee_House, Color.Yellow },
                { RangeItem.Junimo_Hut, Color.LimeGreen }
            });

            this.sprinklerRangeCreator = new SprinklerRangeCreator();
            this.scarecrowRangeCreator = new ScarecrowRangeCreator();
            this.objectRangeCreators = new List<IObjectRangeCreator>()
            {
                this.sprinklerRangeCreator,
                this.scarecrowRangeCreator,
                new BeeHouseRangeCreator()
            };

            this.buildingRangeCreators = new List<IBuildingRangeCreator>()
            {
                new JunimoHutRangeCreator()
            };

            this.config = helper.ReadConfig<RangeDisplayConfig>();

            //handle compatability for the versions where we assigned the modifier key badly
            if (this.config.HoverModifierKey == "control")
            {
                this.config.HoverModifierKey = "leftcontrol,rightcontrol";
                helper.WriteConfig(this.config);
            }

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

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.sprinklerRangeCreator.ModRegistryReady(this.Helper.ModRegistry);
            this.scarecrowRangeCreator.ModRegistryReady(this.Helper.ModRegistry);
        }

        private void RefreshRangeItems(GameLocation location)
        {
            if (!Context.IsWorldReady || location == null)
                return;

            Vector2 mouseTile = new Vector2((Game1.getMouseX() + Game1.viewport.X) / Game1.tileSize, (Game1.getMouseY() + Game1.viewport.Y) / Game1.tileSize);

            this.displayManager.Clear();

            //Objects in the world and objects being hovered over
            foreach (KeyValuePair<Vector2, SObject> item in location.Objects.Pairs)
            {
                foreach (IObjectRangeCreator creator in this.objectRangeCreators)
                {
                    if (creator.CanHandle(item.Value))
                    {
                        this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetRange(item.Value, item.Key, location), mouseTile.Equals(item.Key) && this.isModifierKeyDown);
                    }
                }
            }

            //Buildings in the world and objects being hovered over
            foreach (IBuildingRangeCreator creator in this.buildingRangeCreators)
            {
                Vector2 mouseTileOrZero = this.isModifierKeyDown ? mouseTile : Vector2.Zero;
                this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetRange(mouseTileOrZero, location));
                this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetForceRange(mouseTileOrZero, location), true);
            }

            //Held item
            if (this.activeObject != null && this.config.ShowRangeOfHeldItem)
            {
                foreach (IObjectRangeCreator creator in this.objectRangeCreators)
                {
                    if (creator.CanHandle(this.activeObject))
                    {
                        this.displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetRange(this.activeObject, mouseTile, Game1.currentLocation), true);
                    }
                }
            }
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (this.DoesMatchConfigKey(e.Button, this.config.HoverModifierKey) && this.config.ShowRangeOfHoveredOverItem)
            {
                this.isModifierKeyDown = false;
                this.RefreshRangeItems(Game1.currentLocation);
            }
        }

        private bool DoesMatchConfigKey(SButton b, string configValue)
        {
            string buttonAsString = b.ToString().ToLower();
            return configValue.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(Item => buttonAsString.Equals(Item.Trim()));
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
            if (this.DoesMatchConfigKey(e.Button, this.config.CycleActiveDisplayKey))
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
            else if (this.DoesMatchConfigKey(e.Button, this.config.HoverModifierKey) && this.config.ShowRangeOfHoveredOverItem)
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
    }
}