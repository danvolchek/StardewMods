using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace DesertObelisk
{
    public class ModEntry : Mod
    {
        private const string savedObeliskPath = "savedObelisks.json";
        internal static int DesertWarpX = 34;
        private AssetModifier modifier;

        private BluePrint obeliskBlueprint;
        private List<DesertObelisk> savedTempObelisks;

        /// <summary>Whether the mod is listening for events.</summary>
        private bool isSubscribed;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            if (helper.ModRegistry.IsLoaded("Entoarox.ExtendedMinecart"))
                DesertWarpX -= 2;

            DirectoryInfo savesFolder = new DirectoryInfo(Path.Combine(helper.DirectoryPath, "saves"));
            if (!savesFolder.Exists)
                savesFolder.Create();

            this.modifier = new AssetModifier(helper, this.Monitor, DesertWarpX);
            this.savedTempObelisks = new List<DesertObelisk>();
            this.obeliskBlueprint = new BluePrint("Desert Obelisk");

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += (sender, args) => this.Unsubscribe();
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        private void Subscribe()
        {
            if (this.isSubscribed)
                return;

            this.Helper.Events.GameLoop.Saved += this.OnSaved;
            this.Helper.Events.GameLoop.Saving += this.OnSaving;
            this.modifier.ModifyMap();

            this.Monitor.Log("Subscribed to events.", LogLevel.Trace);

            this.isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!this.isSubscribed)
                return;

            this.Helper.Events.GameLoop.Saved -= this.OnSaved;
            this.Helper.Events.GameLoop.Saving -= this.OnSaving;

            this.Monitor.Log("Unsubscribed from events.", LogLevel.Trace);

            this.isSubscribed = false;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                this.Unsubscribe();
                return;
            }

            this.Subscribe();

            this.LoadObelisksFromFile(Path.Combine("saves", $"{Constants.SaveFolderName}_{savedObeliskPath}"));
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaved(object sender, SavedEventArgs e)
        {
            this.LoadAndClearTempObelisks(this.savedTempObelisks);
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, EventArgs e)
        {
            this.SaveObelisksToFile(Path.Combine("saves", $"{Constants.SaveFolderName}_{savedObeliskPath}"));

            this.ClearAndSaveTempObelisks(this.savedTempObelisks);
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // add blueprint if player has access to the desert
            if (e.NewMenu != null && Game1.player.mailReceived.Contains("ccVault"))
            {
                if (e.NewMenu is CarpenterMenu cMenu && this.Helper.Reflection.GetField<bool>(cMenu, "magicalConstruction").GetValue())
                    this.Helper.Reflection.GetField<List<BluePrint>>(cMenu, "blueprints").GetValue().Insert(3, this.obeliskBlueprint);
            }

            // handle closed menu
            this.HandleMenuClosed(e.OldMenu);
        }

        private void HandleMenuClosed(IClickableMenu m)
        {
            if (m is CarpenterMenu cMenu && this.Helper.Reflection.GetField<bool>(cMenu, "magicalConstruction").GetValue())
                this.ConvertObelisks();
        }

        private void SaveObelisksToFile(string path)
        {
            this.Helper.Data.WriteJsonFile<IList<Vector2>>(
                path,
                Game1.getFarm().buildings.OfType<DesertObelisk>().Select(item => new Vector2(item.tileX.Value, item.tileY.Value)).ToList()
            );
        }

        private void LoadObelisksFromFile(string path)
        {
            var saveData = this.Helper.Data.ReadJsonFile<IList<Vector2>>(path);

            if (saveData != null)
                foreach (Vector2 item in saveData)
                    Game1.getFarm().buildings.Add(new DesertObelisk(this.obeliskBlueprint, item, DesertWarpX));
        }

        private void ConvertObelisks()
        {
            NetCollection<Building> buildings = Game1.getFarm().buildings;
            for (var i = 0; i < buildings.Count; i++)
                if (buildings[i].buildingType.Value == "Desert Obelisk" && !(buildings[i] is DesertObelisk))
                {
                    this.Monitor.Log($"Converting {buildings[i].GetType().Name}", LogLevel.Trace);
                    buildings[i] = new DesertObelisk(this.obeliskBlueprint,
                        new Vector2(buildings[i].tileX.Value, buildings[i].tileY.Value), DesertWarpX);
                }
        }

        private void ClearAndSaveTempObelisks(List<DesertObelisk> saveData)
        {
            saveData.Clear();

            NetCollection<Building> buildings = Game1.getFarm().buildings;
            List<DesertObelisk> copied = new List<DesertObelisk>();

            copied.AddRange(buildings.Where(item => item is DesertObelisk).OfType<DesertObelisk>());
            saveData.AddRange(copied);

            foreach (DesertObelisk obelisk in copied)
                buildings.Remove(obelisk);
        }

        private void LoadAndClearTempObelisks(ICollection<DesertObelisk> saveData)
        {
            foreach (DesertObelisk obelisk in saveData)
                Game1.getFarm().buildings.Add(obelisk);
            saveData.Clear();
        }
    }
}