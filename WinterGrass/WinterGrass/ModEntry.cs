using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WinterGrass
{
    public class ModEntry : Mod
    {
        //Mod configuration
        private static ModConfig config;

        //Last list of grass
        private Dictionary<GameLocation, Dictionary<Vector2, TerrainFeature>> yesterdayGrassByLocation;

        //The location of the current save file
        private string save_file;

        //Entry method -> set up event listeners
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            yesterdayGrassByLocation = new Dictionary<GameLocation, Dictionary<Vector2, TerrainFeature>>();

            SaveEvents.BeforeSave += this.BeforeSave;
            SaveEvents.AfterLoad += this.AfterLoad;
            PlayerEvents.InventoryChanged += this.InventoryChanged;
            GameEvents.OneSecondTick += this.OneSecondTick;
            TimeEvents.AfterDayStarted += this.AfterDayStarted;

            MenuEvents.MenuClosed += this.MenuClosed;
        }

        //Event listener -> After the day starts, re add grass that was removed on the first day of winter, handle newly grown grass, and fix grass color
        private void AfterDayStarted(object sender, EventArgs e)
        {
            if (Game1.currentSeason.Equals("winter"))
            {
                if (Game1.dayOfMonth == 1)
                    ReAddRemovedGrass();
                else if (config.DisableWinterGrassGrowth)
                    RemoveNewlyGrownGrass();
                FixGrassColor();
            }
        }

        //These could be removed if there was a way to save before Game1.NewDayAfterFade() - they attempt to imitate that
        //(BeforeSave happens after that, which is too late - NewDayAfterFade removes grass to be deleted for winter)

        //If the user glitches to stay up forever, we will be repeatedly saving the grass
        //Event listener -> After the user is supposed to pass out, the day is over, so save existing grass
        private void OneSecondTick(object sender, EventArgs e)
        {
            if (Game1.timeOfDay >= 2600)
                SaveGrass();
        }
        
        //Event listener -> If the user chose yes to go to sleep, the day is over, so save existing grass
        private void MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is DialogueBox && Game1.player.currentLocation is FarmHouse && Game1.newDay)
                SaveGrass();
        }


        //Event listenr -> Before a save occurs, write existing grass to a file
        private void BeforeSave(object sender, EventArgs e)
        {

            if (Game1.currentSeason.Equals("winter") || (Game1.currentSeason.Equals("fall") && Game1.dayOfMonth == 28))
            {
                Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems = new Dictionary<string, Dictionary<Vector2, GrassSave>>();
                foreach (KeyValuePair<GameLocation, Dictionary<Vector2, TerrainFeature>> item in yesterdayGrassByLocation)
                {
                    Dictionary<Vector2, GrassSave> grass = new Dictionary<Vector2, GrassSave>();
                    foreach (KeyValuePair<Vector2, TerrainFeature> item2 in item.Value)
                        grass[item2.Key] = new GrassSave((item2.Value as Grass).grassType, (item2.Value as Grass).numberOfWeeds);
                    savedItems[item.Key.name] = grass;
                }

                System.IO.Directory.CreateDirectory(save_file.Substring(0, save_file.LastIndexOf(Path.DirectorySeparatorChar)));

                WriteToFile(save_file, savedItems);

            }
        }

        //Event listener -> After a save loads, if its winter, add back in saved grass
        private void AfterLoad(object sender, EventArgs e)
        {
            save_file = $"{Helper.DirectoryPath}{Path.DirectorySeparatorChar}Saved Grass{Path.DirectorySeparatorChar}{new string(Game1.player.name.Where(char.IsLetterOrDigit).ToArray())}_{Game1.uniqueIDForThisGame}.txt";

            if (Game1.currentSeason.Equals("winter"))
            {
                Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems = ReadFromFile(save_file);
                if (savedItems != null)
                    foreach (KeyValuePair<string, Dictionary<Vector2, GrassSave>> item in savedItems)
                    {
                        GameLocation l = Game1.getLocationFromName(item.Key);
                        if (l != null)
                            foreach (KeyValuePair<Vector2, GrassSave> item2 in item.Value)
                            {
                                l.terrainFeatures[item2.Key] = new Grass(item2.Value.grassType, item2.Value.numWeeds);
                                (l.terrainFeatures[item2.Key] as Grass).grassSourceOffset = 80;
                            }
                    }

            }
        }

        //Event listener -> After the user places down a grass starter, fix the color of the newly placed grass
        private void InventoryChanged(object sender, EventArgsInventoryChanged e)
        {
            if (e.QuantityChanged.Any(item => item.ChangeType == ChangeType.Removed || item.ChangeType == ChangeType.StackChange && item.StackChange < 0 && item.Item.parentSheetIndex == 297)
                || e.Removed.Any(item => item.ChangeType == ChangeType.Removed || item.ChangeType == ChangeType.StackChange && item.StackChange < 0 && item.Item.parentSheetIndex == 297))
                if (Game1.currentSeason.Equals("winter"))
                    FixGrassColor();
        }

        //Writes saved grass data to a file
        private void WriteToFile(string path, Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems)
        {
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path))
                {
                    foreach (KeyValuePair<string, Dictionary<Vector2, GrassSave>> item in savedItems)
                    {
                        string res = SerializeAll(item.Value);
                        if (res != "")
                            file.WriteLine(item.Key + "-" + res);
                    }
                }
            }
            catch (Exception e)
            {
                Monitor.Log("There was an error writing saved grass to a file", LogLevel.Error);
                Monitor.Log(e.Message, LogLevel.Error);
            }
        }

        //Reads saved grass data from a file
        private Dictionary<string, Dictionary<Vector2, GrassSave>> ReadFromFile(string path)
        {
            try
            {
                Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems = new Dictionary<string, Dictionary<Vector2, GrassSave>>();

                using (System.IO.StreamReader file =
                new System.IO.StreamReader(path))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string name = line.Substring(0, line.IndexOf('-'));
                        savedItems[name] = new Dictionary<Vector2, GrassSave>();
                        
                        line = line.Substring(line.IndexOf('-') + 1);

                        while (line.Length != 0)
                        {
                            line = DeSerializeOne(line, out int x);
                            line = DeSerializeOne(line, out int y);
                            line = DeSerializeOne(line, out int type);
                            line = DeSerializeOne(line, out int weeds);

                            savedItems[name][new Vector2(x, y)] = new GrassSave(type, weeds);
                        }
                    }

                }

                return savedItems;
            }
            catch (Exception e)
            {
                Monitor.Log("No saved grass was found.");
                Monitor.Log(e.Message);
                return null;
            }
        }

        //Reads an int according to the save file format
        private string DeSerializeOne(string line, out int x)
        {
            x = int.Parse(line.Substring(0, line.IndexOf(",")));
            return line.Substring(line.IndexOf(",") + 1);
        }

        //Serializes a grass location to a string
        private string SerializeAll(Dictionary<Vector2, GrassSave> grass)
        {
            string str = "";
            foreach (KeyValuePair<Vector2, GrassSave> item in grass)
            {
                str += item.Key.X + "," + item.Key.Y + "," + item.Value.grassType + "," + item.Value.numWeeds + ",";
            }
            return str;
        }

        //Removes newly grown grass by looking at the differences between what was saved and what exists now
        private void RemoveNewlyGrownGrass()
        {
            foreach (KeyValuePair<GameLocation, Dictionary<Vector2, TerrainFeature>> locPair in yesterdayGrassByLocation)
            {
                Dictionary<Vector2, TerrainFeature> currentGrass = locPair.Key?.terrainFeatures?.Where(item => item.Value is Grass).ToDictionary(entry => entry.Key, entry => entry.Value);
                Dictionary<Vector2, TerrainFeature> yesterdayGrass = locPair.Value;


                if (currentGrass != null && yesterdayGrassByLocation != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> item in currentGrass)
                    {
                        if (!yesterdayGrass.ContainsKey(item.Key))
                        {
                            locPair.Key.terrainFeatures.Remove(item.Key);
                        }
                    }
                }
            }
        }

        //Adds removed grass back in by looking at the difference between what was saved and what exists now
        private void ReAddRemovedGrass()
        {

            foreach (KeyValuePair<GameLocation, Dictionary<Vector2, TerrainFeature>> locPair in yesterdayGrassByLocation)
            {
                Dictionary<Vector2, TerrainFeature> currentTerrainFeatures = locPair.Key?.terrainFeatures?.ToDictionary(entry => entry.Key, entry => entry.Value);

                if (currentTerrainFeatures != null && yesterdayGrassByLocation != null)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> item in locPair.Value)
                    {
                        if (!currentTerrainFeatures.ContainsKey(item.Key))
                        {
                            (item.Value as Grass).grassSourceOffset = 80;
                            locPair.Key.terrainFeatures[item.Key] = item.Value;
                        }
                    }
                }
            }
        }

        //Changes the color of every piece of grass to be snowy
        private void FixGrassColor()
        {
            foreach (GameLocation l in Game1.locations)
            {
                IEnumerable<Grass> currentTerrainFeatures = l?.terrainFeatures?.Select(item => item.Value).OfType<Grass>();
                if (currentTerrainFeatures != null)
                    foreach (Grass item in currentTerrainFeatures)
                        item.grassSourceOffset = 80;
            }
        }

        //Saves every piece of grass that exists into a dictionary
        private void SaveGrass()
        {
            if (Game1.currentSeason.Equals("winter") || (Game1.currentSeason.Equals("fall") && Game1.dayOfMonth == 28))
            {

                yesterdayGrassByLocation.Clear();
                foreach (GameLocation l in Game1.locations)
                    yesterdayGrassByLocation[l] = l.terrainFeatures?.Where(item => item.Value is Grass).ToDictionary(entry => entry.Key,
                                          entry => entry.Value);
                if (yesterdayGrassByLocation != null)
                    Monitor.Log("Saved " + yesterdayGrassByLocation.Select(item => item.Value.Count).Sum() + " clumps.");
                else
                    Monitor.Log("There was an error saving grass.", LogLevel.Error);
            }
        }

    }
}
