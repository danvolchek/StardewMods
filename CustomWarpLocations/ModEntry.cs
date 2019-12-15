using CustomWarpLocations.WarpOverrides;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using static CustomWarpLocations.WarpOverrides.WarpOverride;

namespace CustomWarpLocations
{
    /// <summary>
    /// Disclaimer: This mod is very old, and could use a rewrite (a lot of duplication, saving in the mod folder, etc). But it's not very popular, so that's not worth it.
    /// </summary>
    public class ModEntry : Mod
    {
        /// <summary>The path to the current custom data file, relative to the mod folder.</summary>
        private static string LocationSaveFileName;

        private static readonly List<string> AllowedWarpLocations = new List<string>
        {
            "SkullCave",
            "Mine",
            "Farm",
            "Desert",
            "BusStop",
            "Forest",
            "Town",
            "Mountain",
            "Backwoods",
            "Railroad",
            "Beach",
            "Woods",
            "Sewer",
            "BugLand",
            "WitchSwamp"
        };

        private ModConfig config;

        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<ModConfig>();
            Directory.CreateDirectory(Path.Combine(this.Helper.DirectoryPath, "Data"));

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, EventArgs e)
        {
            // reads/Create a warp location save data file after the game loads
            LocationSaveFileName = Path.Combine("Data", $"{Constants.SaveFolderName}.json");
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, LocationSaveFileName)))
            {
                WarpLocations = this.Helper.Data.ReadJsonFile<NewWarpLocations>(LocationSaveFileName);
                this.ValidateWarpLocations(WarpLocations);
            }
            else
            {
                WarpLocations = new NewWarpLocations();
            }
            this.Helper.Data.WriteJsonFile(LocationSaveFileName, WarpLocations);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // handle pressed keys to save new warp locations
            if (!Context.IsWorldReady || e.Button != this.config.LocationSaveKey)
                return;

            if (!AllowedWarpLocations.Contains(Game1.currentLocation.Name))
            {
                Game1.showGlobalMessage("You can't warp here!");
                return;
            }

            if (Game1.player.ActiveObject != null)
            {
                WarpLocation location = this.GetWarpLocation();
                switch (Game1.player.ActiveObject.ParentSheetIndex)
                {
                    case 688: //Farm Totem
                        this.SetWarpLocation(WarpLocationCategory.Farm, true, location);
                        break;

                    case 689: //Mountain Totem
                        this.SetWarpLocation(WarpLocationCategory.Mountains, true, location);
                        break;

                    case 690: //Beach Totem
                        this.SetWarpLocation(WarpLocationCategory.Beach, true, location);
                        break;

                    case 86: //Earth Crystal
                        this.SetWarpLocation(WarpLocationCategory.Mountains, false, location);
                        break;

                    case 372: //Clam
                        this.SetWarpLocation(WarpLocationCategory.Beach, false, location);
                        break;

                    case 261: //Desert Totem
                        this.SetWarpLocation(WarpLocationCategory.Desert, true, location);
                        break;

                    case 768: //Solar essence
                        this.SetWarpLocation(WarpLocationCategory.Desert, false, location);
                        break;
                }

                this.Helper.Data.WriteJsonFile(LocationSaveFileName, WarpLocations);
            }
            else if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is Wand)
            {
                //Return Scepter
                this.SetWarpLocation(WarpLocationCategory.Farm, false, this.GetWarpLocation());
                this.Helper.Data.WriteJsonFile(LocationSaveFileName, WarpLocations);
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
                        WarpLocations.FarmWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Farm Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpLocations.FarmWarpLocation_Scepter = newLocation;
                        Game1.showGlobalMessage("New Farm Warp Scepter Location Saved!");
                    }

                    break;

                case WarpLocationCategory.Mountains:
                    if (!this.config.AdvancedMode)
                    {
                        WarpLocations.MountainWarpLocation_Totem = newLocation;
                        WarpLocations.MountainWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Mountain Warp Location Saved!");
                    }
                    else if (fromTotem)
                    {
                        WarpLocations.MountainWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Mountain Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpLocations.MountainWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Mountain Warp Obelisk Location Saved!");
                    }

                    break;

                case WarpLocationCategory.Beach:
                    if (!this.config.AdvancedMode)
                    {
                        WarpLocations.BeachWarpLocation_Totem = newLocation;
                        WarpLocations.BeachWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Beach Warp Location Saved!");
                    }
                    else if (fromTotem)
                    {
                        WarpLocations.BeachWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Beach Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpLocations.BeachWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Beach Warp Obelisk Location Saved!");
                    }

                    break;
                case WarpLocationCategory.Desert:
                    if (!this.config.AdvancedMode)
                    {
                        WarpLocations.DesertWarpLocation_Totem = newLocation;
                        WarpLocations.DesertWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Desert Warp Location Saved!");
                    }
                    else if (fromTotem)
                    {
                        WarpLocations.DesertWarpLocation_Totem = newLocation;
                        Game1.showGlobalMessage("New Desert Warp Totem Location Saved!");
                    }
                    else
                    {
                        WarpLocations.DesertWarpLocation_Obelisk = newLocation;
                        Game1.showGlobalMessage("New Desert Warp Obelisk Location Saved!");
                    }

                    break;
            }

            for (var index = 0; index < 12; ++index)
                Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354,
                    Game1.random.Next(25, 75), 6, 1,
                    new Vector2(
                        Game1.random.Next((int)Game1.player.position.X - Game1.tileSize * 4,
                            (int)Game1.player.position.X + Game1.tileSize * 3),
                        Game1.random.Next((int)Game1.player.position.Y - Game1.tileSize * 4,
                            (int)Game1.player.position.Y + Game1.tileSize * 3)), false,
                    Game1.random.NextDouble() < 0.5));
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Replaces the game's warp code with my own!
            if (Context.IsWorldReady && e.IsMultipleOf(8))
            {
                foreach (var action in Game1.delayedActions)
                {
                    var afterFadeFunction = action.afterFadeBehavior;
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
        }

        /**
         * Makes sure that there are no disallowed locations in the given NewWarpLocations.
         **/

        private void ValidateWarpLocations(NewWarpLocations locations)
        {
            var defaults = new NewWarpLocations();

            if (!AllowedWarpLocations.Contains(locations.FarmWarpLocation_Scepter.locationName))
                locations.FarmWarpLocation_Scepter = defaults.FarmWarpLocation_Scepter;
            if (!AllowedWarpLocations.Contains(locations.FarmWarpLocation_Totem.locationName))
                locations.FarmWarpLocation_Totem = defaults.FarmWarpLocation_Totem;

            if (!AllowedWarpLocations.Contains(locations.MountainWarpLocation_Obelisk.locationName))
                locations.MountainWarpLocation_Obelisk = defaults.MountainWarpLocation_Obelisk;
            if (!AllowedWarpLocations.Contains(locations.MountainWarpLocation_Totem.locationName))
                locations.MountainWarpLocation_Totem = defaults.MountainWarpLocation_Totem;

            if (!AllowedWarpLocations.Contains(locations.BeachWarpLocation_Obelisk.locationName))
                locations.BeachWarpLocation_Obelisk = defaults.BeachWarpLocation_Obelisk;
            if (!AllowedWarpLocations.Contains(locations.BeachWarpLocation_Totem.locationName))
                locations.BeachWarpLocation_Totem = defaults.BeachWarpLocation_Totem;

            if (!AllowedWarpLocations.Contains(locations.DesertWarpLocation_Obelisk.locationName))
                locations.DesertWarpLocation_Obelisk = defaults.DesertWarpLocation_Obelisk;
            if (!AllowedWarpLocations.Contains(locations.DesertWarpLocation_Totem.locationName))
                locations.DesertWarpLocation_Totem = defaults.DesertWarpLocation_Totem;
        }

        /**
         * Turns the player's current position into a WarpLocation.
         **/

        private WarpLocation GetWarpLocation()
        {
            return new WarpLocation(Game1.currentLocation.Name, (int)Game1.player.getTileLocation().X,
                (int)Game1.player.getTileLocation().Y);
        }
    }
}
