using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_5;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterHay
{
    public class BetterHayMod : Mod
    {
        //Config
        public static ModConfig Config;

        //Current player location
        public GameLocation CurrentLocation;

        private Random dropGrassStarterRandom;

        //Last list of terrain features
        private Dictionary<Vector2, TerrainFeature> lastTerrainFeatures;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            this.dropGrassStarterRandom = new Random();

            if (Config.EnableGettingHayFromGrassAnytime)
            {
                helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
                helper.Events.Player.Warped += this.OnWarped;
            }

            if (Config.EnableTakingHayFromHoppersAnytime)
            {
                var harmony = HarmonyInstanceFacade.Create(helper.ModRegistry.ModID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // check for removed grass and spawn hay if appropriate
            if (Config.EnableGettingHayFromGrassAnytime && e.IsMultipleOf(8))
            {
                if (Game1.player?.currentLocation?.terrainFeatures == null || this.lastTerrainFeatures == null || Game1.player.currentLocation != this.CurrentLocation)
                    return;

                foreach (KeyValuePair<Vector2, TerrainFeature> item in this.lastTerrainFeatures)
                {
                    if (!Game1.player.currentLocation.terrainFeatures.FieldDict.ContainsKey(item.Key) && item.Value is Grass grass && grass.numberOfWeeds.Value <= 0 && grass.grassType.Value == 1)
                    {
                        //if ((Game1.IsMultiplayer
                        //        ? Game1.recentMultiplayerRandom
                        //        : new Random((int)(Game1.uniqueIDForThisGame + item.Key.X * 1000.0 + item.Key.Y * 11.0)))
                        //    .NextDouble() < 0.5)
                        if ((new Random((int)(Game1.uniqueIDForThisGame + item.Key.X * 1000.0 + item.Key.Y * 11.0)))
                        .NextDouble() < 0.5)
                        {
                            if (Game1.player.CurrentTool is MeleeWeapon && (Game1.player.CurrentTool.Name.Contains("Scythe") || Game1.player.CurrentTool.ParentSheetIndex == 47))
                            {
                                //if (this.IsWithinRange(Game1.player.getTileLocation(), item.Key, 3))
                                if (this.IsWithinRange(Game1.player.Tile, item.Key, 3))
                                {
                                    if (this.dropGrassStarterRandom.NextDouble() < Config.ChanceToDropGrassStarterInsteadOfHay)
                                        this.AttemptToGiveGrassStarter(item.Key, Game1.getFarm().piecesOfHay.Value == Game1.getFarm().GetHayCapacity());
                                    else if (Game1.getFarm().tryToAddHay(1) != 0)
                                    {
                                        if (!BetterHayGrass.TryAddItemToInventory(178) && Config.DropHayOnGroundIfNoRoomInInventory)
                                            BetterHayGrass.DropOnGround(item.Key, 178);
                                    }
                                }
                            }
                        }
                    }
                }

                this.lastTerrainFeatures = Game1.player.currentLocation?.terrainFeatures?.FieldDict.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value.Value
                );
            }
        }

        //Correctly give grass starters instead of hay when silos are full and not full
        private void AttemptToGiveGrassStarter(Vector2 location, bool silosAreFull)
        {
            bool added = BetterHayGrass.TryAddItemToInventory(297);
            if (!added && Config.DropHayOnGroundIfNoRoomInInventory)
            {
                BetterHayGrass.DropOnGround(location, 297);
                added = true;
            }

            if (!silosAreFull && added)
                Game1.getFarm().piecesOfHay.Value -= 1;
        }

        //Returns whether the first vector is with range of the second, in euclidian distance
        private bool IsWithinRange(Vector2 first, Vector2 second, double range)
        {
            return Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)) < range;
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                // update the last list of terrain features
                if (Config.EnableGettingHayFromGrassAnytime)
                {
                    this.lastTerrainFeatures = e.NewLocation?.terrainFeatures?.FieldDict.ToDictionary(item => item.Key, item => item.Value.Value);
                    this.CurrentLocation = e.NewLocation;
                }
            }
        }
    }
}
