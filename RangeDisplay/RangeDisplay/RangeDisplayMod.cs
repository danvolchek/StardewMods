using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RangeDisplay.RangeHandling;
using RangeDisplay.RangeHandling.RangeCreators;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
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

        //Need to fix rendering and item change (walking)
        public override void Entry(IModHelper helper)
        {
            displayManager = new DisplayManager(helper.Content.Load<Texture2D>("assets/border.png"), helper.Content.Load<Texture2D>("assets/filled_in.png"),
            new Dictionary<RangeItem, Color>()
            {
                { RangeItem.Sprinkler, Color.LightSkyBlue },
                { RangeItem.Scarecrow, Color.SaddleBrown },
                { RangeItem.Bee_House, Color.Yellow },
                { RangeItem.Junimo_Hut, Color.LimeGreen }
            });

            objectRangeCreators = new List<IObjectRangeCreator>()
            {
                new SprinklerRangeCreator(),
                new ScarecrowRangeCreator(),
                new BeeHouseRangeCreator()
            };

            buildingRangeCreators = new List<IBuildingRangeCreator>()
            {
                new JunimoHutRangeCreator()
            };

            config = helper.ReadConfig<RangeDisplayConfig>();

            GraphicsEvents.OnPreRenderHudEvent += OnPreRenderHudEvent;
            InputEvents.ButtonPressed += ButtonPressed;
            GameEvents.EighthUpdateTick += EighthUpdateTick;

            if (config.ShowRangeOfHeldItem || config.ShowRangeOfHoveredOverItem)
            {
                ControlEvents.MouseChanged += MouseChanged;
                if (config.ShowRangeOfHoveredOverItem)
                    InputEvents.ButtonReleased += ButtonReleased;
            }
        }

        private void RefreshRangeItems(GameLocation location)
        {
            if (!Context.IsWorldReady || location == null)
                return;

            Vector2 mouseTile = new Vector2((Game1.getMouseX() + Game1.viewport.X) / Game1.tileSize, (Game1.getMouseY() + Game1.viewport.Y) / Game1.tileSize);

            displayManager.Clear();

            //Objects in the world and objects being hovered over
            foreach (KeyValuePair<Vector2, SObject> item in location.Objects)
            {
                foreach (IObjectRangeCreator creator in objectRangeCreators)
                {
                    if (creator.CanHandle(item.Value))
                    {
                        displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetRange(item.Value, item.Key, location), mouseTile.Equals(item.Key) && isModifierKeyDown);
                        break;
                    }
                }
            }

            //Buildings in the world and objects being hovered over
            foreach (IBuildingRangeCreator creator in buildingRangeCreators)
            {
                Vector2 mouseTileOrZero = isModifierKeyDown ? mouseTile : Vector2.Zero;
                displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetRange(mouseTileOrZero, location));
                displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetForceRange(mouseTileOrZero, location), true);
            }

            //Held item
            if (activeObject != null && config.ShowRangeOfHeldItem)
            {
                foreach (IObjectRangeCreator creator in objectRangeCreators)
                {
                    if (creator.CanHandle(activeObject))
                    {
                        displayManager.AddTilesToDisplay(creator.HandledRangeItem, creator.GetRange(activeObject, mouseTile, Game1.currentLocation), true);
                        break;
                    }
                }
            }
        }

        private void ButtonReleased(object sender, EventArgsInput e)
        {
            if (e.Button.ToString().ToLower().Contains(config.HoverModifierKey.ToLower()) && config.ShowRangeOfHoveredOverItem)
            {
                isModifierKeyDown = false;
                RefreshRangeItems(Game1.currentLocation);
            }
        }

        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (activeObject != null && Game1.activeClickableMenu == null && config.ShowRangeOfHeldItem)
                RefreshRangeItems(Game1.currentLocation);
            else if (config.ShowRangeOfHoveredOverItem && isModifierKeyDown)
                RefreshRangeItems(Game1.currentLocation);
        }

        private void EighthUpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null)
                return;

            RefreshRangeItems(Game1.currentLocation);

            activeObject = Game1.player.ActiveObject;
        }

        private void ButtonPressed(object sender, EventArgsInput e)
        {
            if (e.Button.ToString().ToLower().Contains(config.CycleActiveDisplayKey))
            {
                if (displayIndex == allRangeItems.Length - 1)
                {
                    displayIndex++;
                    displayManager.DisplayAll(true);
                    hudMessageManager.AddHudMessage("All");
                    return;
                }
                else if (displayIndex == allRangeItems.Length)
                {
                    displayIndex++;
                    displayManager.DisplayAll(false);
                    hudMessageManager.AddHudMessage("None");
                    return;
                }

                if (displayIndex == allRangeItems.Length + 1)
                {
                    displayIndex = -1;
                }
                displayIndex = (displayIndex + 1) % allRangeItems.Length;
                displayManager.DisplayOnly(allRangeItems[displayIndex]);
                hudMessageManager.AddHudMessage(allRangeItems[displayIndex]);
            }
            else if (e.Button.ToString().ToLower().Contains(config.HoverModifierKey.ToLower()) && config.ShowRangeOfHoveredOverItem)
            {
                isModifierKeyDown = true;
                RefreshRangeItems(Game1.currentLocation);
            }
        }

        private void OnPreRenderHudEvent(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            displayManager.Draw(Game1.spriteBatch);
        }
    }
}