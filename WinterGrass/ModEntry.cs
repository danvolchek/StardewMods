﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace WinterGrass
{
    public class ModEntry : Mod
    {
        //Mod configuration
        private static ModConfig config;

        //The location of the current save file
        private string saveFile;

        //Last list of grass
        private Dictionary<GameLocation, Dictionary<Vector2, Grass>> yesterdayGrassByLocation;

        //Is listenening for events
        private bool isSubscribed;

        //Entry method -> set up event listeners
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            this.yesterdayGrassByLocation = new Dictionary<GameLocation, Dictionary<Vector2, Grass>>();

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += (sender, args) => this.Unsubscribe();
        }

        private void Subscribe()
        {
            if (this.isSubscribed)
                return;

            this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.Helper.Events.GameLoop.Saving += this.OnSaving;
            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;

            this.Monitor.Log("Subscribed to events.", LogLevel.Trace);

            this.isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!this.isSubscribed)
                return;

            this.Helper.Events.GameLoop.DayStarted -= this.OnDayStarted;
            this.Helper.Events.GameLoop.Saving -= this.OnSaving;
            this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
            this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;

            this.Monitor.Log("Unsubscribed from events.", LogLevel.Trace);

            this.isSubscribed = false;
        }

        //Event listener -> After the day starts, re add grass that was removed on the first day of winter, handle newly grown grass, and fix grass color
        private void OnDayStarted(object sender, EventArgs e)
        {
            if (Game1.currentSeason.Equals("winter"))
            {
                if (Game1.dayOfMonth == 1)
                    this.ReAddRemovedGrass();
                else if (config.DisableWinterGrassGrowth) this.RemoveNewlyGrownGrass();
                this.FixGrassColor();
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            //These could be removed if there was a way to save before Game1.NewDayAfterFade() - they attempt to imitate that
            //(BeforeSave happens after that, which is too late - NewDayAfterFade removes grass to be deleted for winter)

            //If the user glitches to stay up forever, we will be repeatedly saving the grass
            //Event listener -> After the user is supposed to pass out, the day is over, so save existing grass
            if (e.IsOneSecond && (Game1.timeOfDay >= 2600 || Game1.newDay))
                this.SaveGrass();
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // If the user chose yes to go to sleep, the day is over, so save existing grass

            if (e.OldMenu is DialogueBox && Game1.player.currentLocation is FarmHouse && Game1.newDay)
                this.SaveGrass();
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Before a save occurs, write existing grass to a file
            if (Game1.currentSeason.Equals("winter") || Game1.currentSeason.Equals("fall") && Game1.dayOfMonth == 28)
            {
                Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems =
                    new Dictionary<string, Dictionary<Vector2, GrassSave>>();
                foreach (KeyValuePair<GameLocation, Dictionary<Vector2, Grass>> item in this
                    .yesterdayGrassByLocation)
                {
                    Dictionary<Vector2, GrassSave> grass = new Dictionary<Vector2, GrassSave>();
                    foreach (KeyValuePair<Vector2, Grass> item2 in item.Value)
                        grass[item2.Key] = new GrassSave(item2.Value.grassType.Value,
                            item2.Value.numberOfWeeds.Value);
                    savedItems[item.Key.Name] = grass;
                }

                Directory.CreateDirectory(new FileInfo(this.saveFile).Directory.FullName);

                this.WriteToFile(this.saveFile, savedItems);
            }
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // After a save loads, if its winter, add back in saved grass

            if (!Context.IsMainPlayer)
            {
                this.Unsubscribe();
                return;
            }

            this.Subscribe();

            this.saveFile = Path.Combine(this.Helper.DirectoryPath, "Saved Grass", $"{Constants.SaveFolderName}.txt");

            if (Game1.currentSeason.Equals("winter"))
            {
                Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems = this.ReadFromFile(this.saveFile);
                if (savedItems != null)
                    foreach (KeyValuePair<string, Dictionary<Vector2, GrassSave>> item in savedItems)
                    {
                        GameLocation l = Game1.getLocationFromName(item.Key);
                        if (l != null)
                            foreach (KeyValuePair<Vector2, GrassSave> item2 in item.Value)
                            {
                                l.terrainFeatures[item2.Key] = new Grass(item2.Value.GrassType, item2.Value.NumWeeds);
                                ((Grass)l.terrainFeatures[item2.Key]).grassSourceOffset.Value = 80;
                            }
                    }
            }
        }

        /// <summary>Raised after items are added or removed to a player's inventory.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // After the user places down a grass starter, fix the color of the newly placed grass
            if (e.IsLocalPlayer && Game1.IsWinter && e.Removed.Any(item => item.ParentSheetIndex == 297))
            {
                this.FixGrassColor();
            }
        }

        //Writes saved grass data to a file
        private void WriteToFile(string path, Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(path))
                {
                    foreach (KeyValuePair<string, Dictionary<Vector2, GrassSave>> item in savedItems)
                    {
                        string res = this.SerializeAll(item.Value);
                        if (res != "")
                            file.WriteLine(item.Key + "-" + res);
                    }
                }
            }
            catch (Exception e)
            {
                this.Monitor.Log("There was an error writing saved grass to a file", LogLevel.Error);
                this.Monitor.Log(e.Message, LogLevel.Error);
            }
        }

        //Reads saved grass data from a file
        private Dictionary<string, Dictionary<Vector2, GrassSave>> ReadFromFile(string path)
        {
            try
            {
                Dictionary<string, Dictionary<Vector2, GrassSave>> savedItems =
                    new Dictionary<string, Dictionary<Vector2, GrassSave>>();

                using (StreamReader file =
                    new StreamReader(path))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string name = line.Substring(0, line.IndexOf('-'));
                        savedItems[name] = new Dictionary<Vector2, GrassSave>();

                        line = line.Substring(line.IndexOf('-') + 1);

                        while (line.Length != 0)
                        {
                            line = this.DeSerializeOne(line, out int x);
                            line = this.DeSerializeOne(line, out int y);
                            line = this.DeSerializeOne(line, out int type);
                            line = this.DeSerializeOne(line, out int weeds);

                            savedItems[name][new Vector2(x, y)] = new GrassSave(type, weeds);
                        }
                    }
                }

                return savedItems;
            }
            catch (Exception e)
            {
                this.Monitor.Log("No saved grass was found.");
                this.Monitor.Log(e.Message);
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
                str += $"{item.Key.X},{item.Key.Y},{item.Value.GrassType},{item.Value.NumWeeds},";
            return str;
        }

        //Removes newly grown grass by looking at the differences between what was saved and what exists now
        private void RemoveNewlyGrownGrass()
        {
            foreach (KeyValuePair<GameLocation, Dictionary<Vector2, Grass>> locPair in this
                .yesterdayGrassByLocation)
            {
                Dictionary<Vector2, Grass> currentGrass = locPair.Key?.terrainFeatures?.Pairs
                    .Where(item => item.Value is Grass).ToDictionary(entry => entry.Key, entry => entry.Value as Grass);
                Dictionary<Vector2, Grass> yesterdayGrass = locPair.Value;

                if (currentGrass != null && this.yesterdayGrassByLocation != null)
                {
                    foreach (KeyValuePair<Vector2, Grass> item in currentGrass)
                    {
                        if (!yesterdayGrass.ContainsKey(item.Key))
                            locPair.Key.terrainFeatures.Remove(item.Key);
                    }
                }
            }
        }

        //Adds removed grass back in by looking at the difference between what was saved and what exists now
        private void ReAddRemovedGrass()
        {
            foreach (KeyValuePair<GameLocation, Dictionary<Vector2, Grass>> locPair in this
                .yesterdayGrassByLocation)
            {
                Dictionary<Vector2, TerrainFeature> currentTerrainFeatures =
                    locPair.Key?.terrainFeatures?.Pairs.ToDictionary(entry => entry.Key, entry => entry.Value);

                if (currentTerrainFeatures != null && this.yesterdayGrassByLocation != null)
                {
                    foreach (KeyValuePair<Vector2, Grass> item in locPair.Value)
                    {
                        if (!currentTerrainFeatures.ContainsKey(item.Key))
                        {
                            item.Value.grassSourceOffset.Value = 80;
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
                IEnumerable<Grass> currentTerrainFeatures =
                    l?.terrainFeatures?.Pairs.Select(item => item.Value).OfType<Grass>();
                if (currentTerrainFeatures != null)
                {
                    foreach (Grass item in currentTerrainFeatures)
                        item.grassSourceOffset.Value = 80;
                }
            }
        }

        //Saves every piece of grass that exists into a dictionary
        private void SaveGrass()
        {
            if (Game1.currentSeason.Equals("winter") || Game1.currentSeason.Equals("fall") && Game1.dayOfMonth == 28)
            {
                this.yesterdayGrassByLocation.Clear();
                foreach (GameLocation l in Game1.locations)
                    this.yesterdayGrassByLocation[l] = l.terrainFeatures?.Pairs.Where(item => item.Value is Grass)
                        .ToDictionary(entry => entry.Key,
                            entry => entry.Value as Grass);
                if (this.yesterdayGrassByLocation != null)
                    this.Monitor.Log($"Saved {this.yesterdayGrassByLocation.Select(item => item.Value.Count).Sum()} clumps.");
                else
                    this.Monitor.Log("There was an error saving grass.", LogLevel.Error);
            }
        }
    }
}
