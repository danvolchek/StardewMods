using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DesertObelisk
{
    public class ModEntry : Mod
    {

        private BluePrint obeliskBlueprint;
        private List<DesertObelisk> savedTempObelisks;
        private readonly string savedObeliskPath = "savedObelisks.json";
        private AssetModifier modifier;
        private int desertWarpX = 34;

        public override void Entry(IModHelper helper)
        {
            if (helper.ModRegistry.IsLoaded("Entoarox.ExtendedMinecart"))
            {
                desertWarpX -= 2;
            }
            string savesFolder = $"{helper.DirectoryPath}{Path.DirectorySeparatorChar}saves";
            if (!Directory.Exists(savesFolder))
                Directory.CreateDirectory(savesFolder);
            modifier = new AssetModifier(helper, this.Monitor, desertWarpX);
            savedTempObelisks = new List<DesertObelisk>();
            obeliskBlueprint = new BluePrint("Desert Obelisk");

            SaveEvents.AfterLoad += this.AfterLoad;
            SaveEvents.AfterSave += this.AfterSave;
            SaveEvents.BeforeSave += this.BeforeSave;
            MenuEvents.MenuChanged += this.MenuChanged;
            MenuEvents.MenuClosed += this.MenuClosed;
        }

        private void AfterLoad(object sender, EventArgs e)
        {        
            LoadObelisksFromFile($"saves{Path.DirectorySeparatorChar}{Constants.SaveFolderName}_{savedObeliskPath}");
        }

        private void AfterSave(object sender, EventArgs e)
        {
            LoadAndClearTempObelisks(savedTempObelisks);
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            SaveObelisksToFile($"saves{Path.DirectorySeparatorChar}{Constants.SaveFolderName}_{savedObeliskPath}");

            ClearAndSaveTempObelisks(savedTempObelisks);
        }

        private void MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            //Only if the player has access to the desert
            if (!Game1.player.mailReceived.Contains("ccVault"))
                return;

            if (e.NewMenu is CarpenterMenu cMenu && Helper.Reflection.GetField<bool>(cMenu, "magicalConstruction").GetValue())
            {
                Helper.Reflection.GetField<List<BluePrint>>(cMenu, "blueprints").GetValue().Insert(3, obeliskBlueprint);
            }

            HandleMenuClosed(e.PriorMenu);
        }

        private void MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            HandleMenuClosed(e.PriorMenu);
        }

        private void HandleMenuClosed(IClickableMenu m)
        {
            if (m is CarpenterMenu cMenu && Helper.Reflection.GetField<bool>(cMenu, "magicalConstruction").GetValue())
            {
                ConvertObelisks();
            }
        }

        private void SaveObelisksToFile(string path)
        {
            Helper.WriteJsonFile<IList<Vector2>>(path, Game1.getFarm().buildings.OfType<DesertObelisk>().Select(item => new Vector2(item.tileX, item.tileY)).ToList());
        }

        private void LoadObelisksFromFile(string path)
        {
            IList<Vector2> saveData = Helper.ReadJsonFile<IList<Vector2>>(path);

            if (saveData != null)
                Game1.getFarm().buildings.AddRange(saveData.Select(item => new DesertObelisk(obeliskBlueprint, item, desertWarpX)));
        }

        private void ConvertObelisks()
        {
            List<Building> buildings = Game1.getFarm().buildings;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].buildingType == "Desert Obelisk" && !(buildings[i] is DesertObelisk))
                {
                    Monitor.Log($"Converting {buildings[i].GetType().Name}", LogLevel.Trace);
                    buildings[i] = new DesertObelisk(obeliskBlueprint, new Vector2(buildings[i].tileX, buildings[i].tileY), desertWarpX);
                }
            }
        }

        private void ClearAndSaveTempObelisks(List<DesertObelisk> saveData)
        {
            saveData.Clear();
            List<Building> buildings = Game1.getFarm().buildings;
            saveData.AddRange(buildings.OfType<DesertObelisk>());
            buildings.RemoveAll(item => item is DesertObelisk);
        }

        private void LoadAndClearTempObelisks(IList<DesertObelisk> saveData)
        {
            Game1.getFarm().buildings.AddRange(saveData);
            saveData.Clear();
        }
    }
}
