using System;
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

        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<BetterSlingshotsConfig>();
            if (this.config.GalaxySlingshotPrice < 0)
            {
                this.config.GalaxySlingshotPrice = new BetterSlingshotsConfig().GalaxySlingshotPrice;
                helper.WriteConfig(this.config);
            }

            this.manager = new SlingshotManager(this.config, helper.Reflection);

            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            InputEvents.ButtonReleased += this.InputEvents_ButtonReleased;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is ShopMenu shopMenu && shopMenu.portraitPerson == null && Game1.currentLocation is Club)
            {
                Item slingshotItem = new StardewValley.Tools.Slingshot(34);

                Dictionary<Item, int[]> itemPriceAndStock = this.Helper.Reflection
                    .GetField<Dictionary<Item, int[]>>(shopMenu, "itemPriceAndStock").GetValue();
                itemPriceAndStock.Add(slingshotItem, new[] {this.config.GalaxySlingshotPrice, 1});
                List<Item> forSale = this.Helper.Reflection.GetField<List<Item>>(shopMenu, "forSale").GetValue();
                forSale.Insert(0, slingshotItem);
            }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
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

        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (e.IsActionButton) this.manager.SetActionButtonDownState(false);
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (e.IsActionButton) this.manager.SetActionButtonDownState(true);
        }
    }
}