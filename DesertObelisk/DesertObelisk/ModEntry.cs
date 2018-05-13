using System;
using System.Collections;
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

        //Is listenening for events
        private bool isSubscribed;

        public override void Entry(IModHelper helper)
        {

            if (helper.ModRegistry.IsLoaded("Entoarox.ExtendedMinecart")) DesertWarpX -= 2;
            var savesFolder = $"{helper.DirectoryPath}{Path.DirectorySeparatorChar}saves";
            if (!Directory.Exists(savesFolder))
                Directory.CreateDirectory(savesFolder);
            this.modifier = new AssetModifier(helper, this.Monitor, DesertWarpX);
            this.savedTempObelisks = new List<DesertObelisk>();
            this.obeliskBlueprint = new BluePrint("Desert Obelisk");

            SaveEvents.AfterLoad += this.AfterLoad;
            SaveEvents.AfterReturnToTitle += (sender, args) => this.Unsubscribe();
            MenuEvents.MenuChanged += this.MenuChanged;
            MenuEvents.MenuClosed += this.MenuClosed;
        }

        private void Subscribe()
        {
            if (this.isSubscribed)
                return;

            SaveEvents.AfterSave += this.AfterSave;
            SaveEvents.BeforeSave += this.BeforeSave;
            this.modifier.ModifyMap();

            this.Monitor.Log("Subscribed to events.", LogLevel.Trace);

            this.isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!this.isSubscribed)
                return;

            SaveEvents.AfterSave -= this.AfterSave;
            SaveEvents.BeforeSave -= this.BeforeSave;

            this.Monitor.Log("Unsubscribed from events.", LogLevel.Trace);

            this.isSubscribed = false;
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                this.Unsubscribe();
                return;
            }

            this.Subscribe();

            this.LoadObelisksFromFile(
                $"saves{Path.DirectorySeparatorChar}{Constants.SaveFolderName}_{savedObeliskPath}");
        }

        private void AfterSave(object sender, EventArgs e)
        {
            this.LoadAndClearTempObelisks(this.savedTempObelisks);
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            this.SaveObelisksToFile(
                $"saves{Path.DirectorySeparatorChar}{Constants.SaveFolderName}_{savedObeliskPath}");

            this.ClearAndSaveTempObelisks(this.savedTempObelisks);
        }

        private void MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            //Only if the player has access to the desert
            if (!Game1.player.mailReceived.Contains("ccVault"))
                return;

            if (e.NewMenu is CarpenterMenu cMenu &&
                this.Helper.Reflection.GetField<bool>(cMenu, "magicalConstruction").GetValue())
                this.Helper.Reflection.GetField<List<BluePrint>>(cMenu, "blueprints").GetValue()
                    .Insert(3, this.obeliskBlueprint);

            this.HandleMenuClosed(e.PriorMenu);
        }

        private void MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            this.HandleMenuClosed(e.PriorMenu);
        }

        private void HandleMenuClosed(IClickableMenu m)
        {
            if (m is CarpenterMenu cMenu &&
                this.Helper.Reflection.GetField<bool>(cMenu, "magicalConstruction").GetValue()) this.ConvertObelisks();
        }

        private void SaveObelisksToFile(string path)
        {
            this.Helper.WriteJsonFile<IList<Vector2>>(path,
                Game1.getFarm().buildings.OfType<DesertObelisk>().Select(item => new Vector2(item.tileX, item.tileY))
                    .ToList());
        }

        private void LoadObelisksFromFile(string path)
        {
            var saveData = this.Helper.ReadJsonFile<IList<Vector2>>(path);

            if (saveData != null)
                foreach (Vector2 item in saveData)
                    Game1.getFarm().buildings.Add(new DesertObelisk(this.obeliskBlueprint, item, DesertWarpX));
        }

        private void ConvertObelisks()
        {
            NetCollection<Building> buildings = Game1.getFarm().buildings;
            for (var i = 0; i < buildings.Count; i++)
                if (buildings[i].buildingType == "Desert Obelisk" && !(buildings[i] is DesertObelisk))
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