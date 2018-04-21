using BetterSlingshots.Slingshot;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace BetterSlingshots
{
    public class BetterSlingshotsMod : Mod
    {
        private bool wasUsingSlingshot = false;
        private BetterSlingshotsConfig config;
        private SlingshotManager manager;

        //Projectile ideas
        //Ricochet
        //Homing
        //explode into bullets
        //boomerang
        //auto reload
        //slime

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<BetterSlingshotsConfig>();
            if (config.GalaxySlingshotPrice < 0)
            {
                config.GalaxySlingshotPrice = new BetterSlingshotsConfig().GalaxySlingshotPrice;
                helper.WriteConfig(config);
            }

            manager = new SlingshotManager(config, helper.Reflection);

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
            InputEvents.ButtonReleased += InputEvents_ButtonReleased;
            GameEvents.UpdateTick += GameEvents_UpdateTick;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is ShopMenu shopMenu && shopMenu.portraitPerson == null && Game1.currentLocation is Club)
            {
                Item slingshotItem = new StardewValley.Tools.Slingshot(34);

                Dictionary<Item, int[]> itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shopMenu, "itemPriceAndStock").GetValue();
                itemPriceAndStock.Add(slingshotItem, new int[] { config.GalaxySlingshotPrice, 1 });
                List<Item> forSale = Helper.Reflection.GetField<List<Item>>(shopMenu, "forSale").GetValue();
                forSale.Insert(0, slingshotItem);
            }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            bool usingSlingshot = Game1.player != null ? Game1.player.usingSlingshot : false;
            if (usingSlingshot)
            {
                if (!wasUsingSlingshot)
                    manager.PrepareForFiring();
            }
            else if (wasUsingSlingshot)
                manager.FiringOver();

            wasUsingSlingshot = usingSlingshot;
        }

        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (e.IsActionButton)
                manager.SetActionButtonDownState(false);
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (e.IsActionButton)
                manager.SetActionButtonDownState(true);
        }
    }
}