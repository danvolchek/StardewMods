using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RangeDisplay.RangeHandling;
using RangeDisplay.RangeHandling.RangeCreators;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using RangeDisplay.RangeHandling.RangeCreators.Buildings;
using RangeDisplay.RangeHandling.RangeCreators.Objects;
using SObject = StardewValley.Object;

namespace RangeDisplay
{
    public class RangeDisplayMod : Mod
    {
        private DisplayManager displayManager;
        private HudMessageManager hudMessageManager = new HudMessageManager();
        private IList<IObjectRangeCreator> objectRangeCreators;
        private IList<IBuildingRangeCreator> buildingRangeCreators;

        private RangeItem[] allRangeItems = (RangeItem[])Enum.GetValues(typeof(RangeItem));
        private int displayIndex = -1;

        private SObject activeObject = null;
        private bool isModifierKeyDown = false;

        private RangeDisplayConfig config;

        private SprinklerRangeCreator sprinklerRangeCreator;

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
            this.objectRangeCreators = new List<IObjectRangeCreator>()
            {
                this.sprinklerRangeCreator,
                new ScarecrowRangeCreator(),
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

            GraphicsEvents.OnPreRenderHudEvent += this.OnPreRenderHudEvent;
            InputEvents.ButtonPressed += this.ButtonPressed;
            GameEvents.EighthUpdateTick += this.EighthUpdateTick;

            if (this.config.ShowRangeOfHeldItem || this.config.ShowRangeOfHoveredOverItem)
            {
                ControlEvents.MouseChanged += this.MouseChanged;
                if (this.config.ShowRangeOfHoveredOverItem)
                    InputEvents.ButtonReleased += this.ButtonReleased;
            }

            GameEvents.FirstUpdateTick += this.FirstUpdateTick;
        }

        private void FirstUpdateTick(object sender, EventArgs e)
        {
            this.sprinklerRangeCreator.ModRegistryReady(this.Helper.ModRegistry);
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
                        break;
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
                        break;
                    }
                }
            }
        }

        private void ButtonReleased(object sender, EventArgsInput e)
        {
            if (this.DoesMatchConfigKey(e.Button, this.config.HoverModifierKey) && this.config.ShowRangeOfHoveredOverItem)
            {
                this.isModifierKeyDown = false;
                RefreshRangeItems(Game1.currentLocation);
            }
        }

        private bool DoesMatchConfigKey(SButton b, string configValue)
        {
            string buttonAsString = b.ToString().ToLower();
            return configValue.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(Item => buttonAsString.Equals(Item.Trim()));
        }       

        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (this.activeObject != null && Game1.activeClickableMenu == null && this.config.ShowRangeOfHeldItem)
                RefreshRangeItems(Game1.currentLocation);
            else if (this.config.ShowRangeOfHoveredOverItem && this.isModifierKeyDown)
                RefreshRangeItems(Game1.currentLocation);
        }

        private void EighthUpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null)
                return;

            RefreshRangeItems(Game1.currentLocation);

            this.activeObject = Game1.player.ActiveObject;
        }

        private void ButtonPressed(object sender, EventArgsInput e)
        {
            if (DoesMatchConfigKey(e.Button, this.config.CycleActiveDisplayKey))
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
            else if (DoesMatchConfigKey(e.Button, this.config.HoverModifierKey) && this.config.ShowRangeOfHoveredOverItem)
            {
                this.isModifierKeyDown = true;
                RefreshRangeItems(Game1.currentLocation);
            }
        }

        private void OnPreRenderHudEvent(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            this.displayManager.Draw(Game1.spriteBatch);
        }
    }
}