using BetterSlingshots.Framework;
using BetterSlingshots.Framework.Config;
using BetterSlingshots.Framework.Patching;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSlingshots
{
    public class BetterSlingshotsMod : Mod
    {
        //TODO: docs, aiming, extra shots/spam prevention.
        private SlingshotManager slingshotManager;
        private BetterSlingshotsModConfig config;
        private readonly IDictionary<string, string> nameToConfigName = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"Slingshot", "Basic"},
            {"Master Slingshot", "Master"},
            {"Galaxy Slingshot", "Galaxy"}
        };
        private readonly IDictionary<string, int> configNameToFireRate = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"Basic", 60},
            {"Master", 45},
            {"Galaxy", 30}
        };
        private bool isActionButtonDown;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = new ConfigManager(this.Helper).GetConfig();
            this.slingshotManager = new SlingshotManager(this.Helper.Reflection, this.config, new PatchManager(this.ModManifest.UniqueID));

            helper.Events.Display.MenuChanged += this.Display_MenuChanged;
            helper.Events.GameLoop.UpdateTicking += this.GameLoop_UpdateTicking;
            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += this.Input_ButtonReleased;
        }

        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button.IsActionButton())
                this.isActionButtonDown = false;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton())
                this.isActionButtonDown = true;
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (Game1.player.usingSlingshot && Game1.player.CurrentTool is Slingshot slingshot &&
                this.isActionButtonDown &&
                this.nameToConfigName.TryGetValue(slingshot.BaseName, out string configName) &&
                this.config.AutomaticSlingshots.Contains(configName) &&
                this.configNameToFireRate.TryGetValue(configName, out int rate) && e.IsMultipleOf((uint)(this.config.RapidFire ? rate / 2 : rate)))
            {
                this.slingshotManager.FireSlingshot(slingshot);
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shopMenu && shopMenu.portraitPerson == null && Game1.currentLocation is Club)
            {
                Item slingshotItem = new Slingshot(34);

                Dictionary<Item, int[]> itemPriceAndStock = this.Helper.Reflection
                    .GetField<Dictionary<Item, int[]>>(shopMenu, "itemPriceAndStock").GetValue();
                itemPriceAndStock.Add(slingshotItem, new[] { this.config.GalaxySlingshotPrice, 1 });
                List<Item> forSale = this.Helper.Reflection.GetField<List<Item>>(shopMenu, "forSale").GetValue();
                forSale.Insert(0, slingshotItem);
            }
        }

    }
}