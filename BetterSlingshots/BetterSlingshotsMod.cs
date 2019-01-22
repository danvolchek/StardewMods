using System.Collections.Generic;
using BetterSlingshots.Slingshot;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace BetterSlingshots
{
    public class BetterSlingshotsMod : Mod
    {
        private BetterSlingshotsConfig config;
        private SlingshotManager manager;
        private bool wasUsingSlingshot;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<BetterSlingshotsConfig>();
            if (this.config.GalaxySlingshotPrice < 0)
            {
                this.config.GalaxySlingshotPrice = new BetterSlingshotsConfig().GalaxySlingshotPrice;
                helper.WriteConfig(this.config);
            }

            this.manager = new SlingshotManager(this.config, helper.Reflection);

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shopMenu && shopMenu.portraitPerson == null && Game1.currentLocation is Club)
            {
                Item slingshotItem = new StardewValley.Tools.Slingshot(34);

                Dictionary<Item, int[]> itemPriceAndStock = this.Helper.Reflection
                    .GetField<Dictionary<Item, int[]>>(shopMenu, "itemPriceAndStock").GetValue();
                itemPriceAndStock.Add(slingshotItem, new[] { this.config.GalaxySlingshotPrice, 1 });
                List<Item> forSale = this.Helper.Reflection.GetField<List<Item>>(shopMenu, "forSale").GetValue();
                forSale.Insert(0, slingshotItem);
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            bool usingSlingshot = Game1.player?.usingSlingshot ?? false;
            if (usingSlingshot)
            {
                if (!this.wasUsingSlingshot) this.manager.PrepareForFiring();
            }
            else if (this.wasUsingSlingshot)
            {
                this.manager.FiringOver();
            }

            /*//this fixes the problem, which means that for some reason the finish event is not getting sent. No idea why
            //it does it when we can only detect the issue - when they scroll away
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.usingSlingshot && !(farmer.CurrentTool is StardewValley.Tools.Slingshot))
                {
                    farmer.usingSlingshot = false;
                    farmer.canReleaseTool = true;
                    farmer.UsingTool = false;
                    farmer.canMove = true;
                    farmer.Halt();
                }
            }*/

            this.wasUsingSlingshot = usingSlingshot;
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button.IsActionButton())
                this.manager.SetActionButtonDownState(false);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton())
                this.manager.SetActionButtonDownState(true);
        }
    }
}