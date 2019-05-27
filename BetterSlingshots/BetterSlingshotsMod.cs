using BetterSlingshots.Framework.Config;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterSlingshots.Framework.Patches;

namespace BetterSlingshots
{
    public class BetterSlingshotsMod : Mod
    {
        internal bool IsAutoFire { get; private set; }
        internal BetterSlingshotsModConfig Config { get; private set; }
        internal static BetterSlingshotsMod Instance;

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
            this.Config = new ConfigManager(this.Helper).GetConfig();
            BetterSlingshotsMod.Instance = this;

            HarmonyInstance harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            helper.Events.Display.MenuChanged += this.Display_MenuChanged;
            helper.Events.GameLoop.UpdateTicking += this.GameLoop_UpdateTicking;
            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += this.Input_ButtonReleased;
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button.IsActionButton())
                this.isActionButtonDown = false;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton())
                this.isActionButtonDown = true;
        }

        /// <summary>Raised before the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (Game1.player.usingSlingshot && Game1.player.CurrentTool is Slingshot slingshot &&
                this.isActionButtonDown &&
                this.nameToConfigName.TryGetValue(slingshot.BaseName, out string configName) &&
                this.Config.AutomaticSlingshots.Contains(configName) &&
                this.configNameToFireRate.TryGetValue(configName, out int rate) && e.IsMultipleOf((uint)(this.Config.RapidFire ? rate / 2 : rate)) &&
                slingshot.attachments[0] != null)
            {
                this.IsAutoFire = true;
                SlingshotFinishPatch.ShouldRun(slingshot, false);
                slingshot.DoFunction(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), 1, slingshot.getLastFarmerToUse());
                SlingshotFinishPatch.ShouldRun(slingshot, true);
                this.IsAutoFire = false;
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
                itemPriceAndStock.Add(slingshotItem, new[] { this.Config.GalaxySlingshotPrice, 1 });
                List<Item> forSale = this.Helper.Reflection.GetField<List<Item>>(shopMenu, "forSale").GetValue();
                forSale.Insert(0, slingshotItem);
            }
        }
    }
}