using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using CustomWarpLocations.WarpOverrides;
using static CustomWarpLocations.WarpOverrides.WarpOverride;
using System.IO;
using Microsoft.Xna.Framework;

namespace CustomWarpLocations
{
    public class ModEntry : Mod
    {
        ModConfig config;

        private static string LocationSaveFileName;
        private static List<String> AllowedWarpLocations = new List<string>() { "SkullCave", "Mine", "Farm", "Desert", "BusStop", "Forest", "Town", "Mountain", "Backwoods", "Railroad", "Beach", "Woods", "Sewer", "BugLand", "WitchSwamp" };

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            Directory.CreateDirectory($"{Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}");

            SaveEvents.AfterLoad += this.AfterLoad;
            GameEvents.EighthUpdateTick += this.InterceptWarps;
            ControlEvents.KeyPressed += this.KeyPressed;
        }

        /**
         * Reads/Creates a warp location save data file after the game loads.
         **/
        private void AfterLoad(object sender, EventArgs e)
        {
            LocationSaveFileName = $"{Helper.DirectoryPath}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}{new string(Game1.player.name.Where(char.IsLetterOrDigit).ToArray())}_{Game1.uniqueIDForThisGame}.json";

            if (File.Exists(LocationSaveFileName))
            {
                WarpOverride.warpLocations = Helper.ReadJsonFile<NewWarpLocations>(LocationSaveFileName);
                ValidateWarpLocations(WarpOverride.warpLocations);
            }
            else
                WarpOverride.warpLocations = new NewWarpLocations();
            Helper.WriteJsonFile<NewWarpLocations>(LocationSaveFileName, WarpOverride.warpLocations);
        }

        /**
         * Handles pressed keys in order to save new warp locations.
         **/
        private void KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.KeyPressed.ToString().ToLower() != config.LocationSaveKey.ToLower())
                return;

            if (!AllowedWarpLocations.Contains(Game1.currentLocation.Name))
            {
                Game1.showGlobalMessage("You can't warp here!");
                return;
            }

            if (Game1.player.ActiveObject != null)
            {
                WarpLocation location = GetWarpLocation();
                switch (Game1.player.ActiveObject.parentSheetIndex)
                {
                    case 688: //Farm Totem
                        SetWarpLocation(WarpLocationCategory.Farm, true, location);
                        break;
                    case 689: //Mountain Totem
                        SetWarpLocation(WarpLocationCategory.Mountains, true, location);
                        break;
                    case 690: //Beach Totem
                        SetWarpLocation(WarpLocationCategory.Beach, true, location);
                        break;
                    case 86: //Earth Crystal
                        SetWarpLocation(WarpLocationCategory.Mountains, false, location);
                        break;
                    case 372: //Clam
                        SetWarpLocation(WarpLocationCategory.Beach, false, location);
                        break;
                }
                Helper.WriteJsonFile<NewWarpLocations>(LocationSaveFileName, WarpOverride.warpLocations);
            }
            else if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is StardewValley.Tools.Wand)
            {
                //Return Scepter
                SetWarpLocation(WarpLocationCategory.Farm, false, GetWarpLocation());
                Helper.WriteJsonFile<NewWarpLocations>(LocationSaveFileName, WarpOverride.warpLocations);
            }
        }

        /**
         * Updates where items warp you to.
         **/
        private void SetWarpLocation(WarpLocationCategory locationType, bool fromTotem, WarpLocation newLocation)
        {
            switch (locationType)
            {
                case WarpLocationCategory.Farm:
                    /*if (!config.UseSeperateLocationForEachItem)
                        {
                            WarpOverride.warpLocations.FarmWarpLocation_Scepter = newLocation;
                            Game1.showGlobalMessage("New Farm Warp Location Saved!");
                        }
                        else*/
                    if (fromTotem)
                    {
                        WarpOverride.warpLocations.FarmWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Farm Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpOverride.warpLocations.FarmWarpLocation_Scepter = newLocation;
                        Game1.showGlobalMessage("New Farm Warp Scepter Location Saved!");
                    }

                    break;
                case WarpLocationCategory.Mountains:
                    if (!config.AdvancedMode)
                    {
                        WarpOverride.warpLocations.MountainWarpLocation_Totem = newLocation;
                        WarpOverride.warpLocations.MountainWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Mountain Warp Location Saved!");
                    }
                    else if (fromTotem)
                    {
                        WarpOverride.warpLocations.MountainWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Mountain Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpOverride.warpLocations.MountainWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Mountain Warp Obelisk Location Saved!");
                    }
                    break;
                case WarpLocationCategory.Beach:
                    if (!config.AdvancedMode)
                    {
                        WarpOverride.warpLocations.BeachWarpLocation_Totem = newLocation;
                        WarpOverride.warpLocations.BeachWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Beach Warp Location Saved!");
                    }
                    else if (fromTotem)
                    {
                        WarpOverride.warpLocations.BeachWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Beach Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpOverride.warpLocations.BeachWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Beach Warp Obelisk Location Saved!");
                    }
                    break;
            }

            for (int index = 0; index < 12; ++index)
                Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)Game1.player.position.X - Game1.tileSize * 4, (int)Game1.player.position.X + Game1.tileSize * 3), (float)Game1.random.Next((int)Game1.player.position.Y - Game1.tileSize * 4, (int)Game1.player.position.Y + Game1.tileSize * 3)), false, Game1.random.NextDouble() < 0.5));

        }

        /**
         * Replaces the game's warp code with my own!
         **/
        private void InterceptWarps(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            foreach (DelayedAction action in Game1.delayedActions)
            {
                Game1.afterFadeFunction afterFadeFunction = action.afterFadeBehavior;
                if (afterFadeFunction != null)
                {
                    WarpOverride newWarp = null;
                    switch (afterFadeFunction.Method.Name)
                    {
                        case "totemWarpForReal":
                            newWarp = new TotemWarpOverride(afterFadeFunction.Target);
                            break;
                        case "wandWarpForReal":
                            newWarp = new WandWarpOverride();
                            break;
                        case "obeliskWarpForReal":
                            newWarp = new ObeliskWarpOverride(afterFadeFunction.Target);
                            break;
                    }

                    if (newWarp != null)
                        action.afterFadeBehavior = newWarp.DoWarp;
                }
            }
        }

        /**
         * Makes sure that there are no disallowed locations in the given NewWarpLocations.
         **/
        private void ValidateWarpLocations(NewWarpLocations locations)
        {
            NewWarpLocations defaults = new NewWarpLocations();

            if (!AllowedWarpLocations.Contains(locations.FarmWarpLocation_Scepter.locationName))
                locations.FarmWarpLocation_Scepter = defaults.FarmWarpLocation_Scepter;
            if (!AllowedWarpLocations.Contains(locations.FarmWarpLocation_Totem.locationName))
                locations.FarmWarpLocation_Scepter = defaults.FarmWarpLocation_Totem;

            if (!AllowedWarpLocations.Contains(locations.MountainWarpLocation_Obelisk.locationName))
                locations.FarmWarpLocation_Scepter = defaults.MountainWarpLocation_Obelisk;
            if (!AllowedWarpLocations.Contains(locations.MountainWarpLocation_Totem.locationName))
                locations.FarmWarpLocation_Scepter = defaults.MountainWarpLocation_Totem;

            if (!AllowedWarpLocations.Contains(locations.BeachWarpLocation_Obelisk.locationName))
                locations.FarmWarpLocation_Scepter = defaults.BeachWarpLocation_Obelisk;
            if (!AllowedWarpLocations.Contains(locations.BeachWarpLocation_Totem.locationName))
                locations.FarmWarpLocation_Scepter = defaults.BeachWarpLocation_Totem;
        }

        /**
         * Turns the player's current position into a WarpLocation.
         **/
        private WarpLocation GetWarpLocation()
        {
            return new WarpLocation(Game1.currentLocation.Name, (int)Game1.player.getTileLocation().X, (int)Game1.player.getTileLocation().Y);
        }

    }

}
