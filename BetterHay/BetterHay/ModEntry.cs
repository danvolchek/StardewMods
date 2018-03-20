using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace BetterHay
{
    public class ModEntry : Mod
    {

        //Config
        public static ModConfig config;

        //Current player location
        public GameLocation currentLocation = null;

        //Last list of terrain features
        private Dictionary<Vector2, TerrainFeature> lastTerrainFeatures = null;

        private Random dropGrassStarterRandom;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            dropGrassStarterRandom = new Random();
            SaveEvents.BeforeSave += this.BeforeSave;

            if (config.EnableGettingHayFromGrassAnytime)
            {
                GameEvents.EighthUpdateTick += this.EighthUpdateTick;
                LocationEvents.CurrentLocationChanged += this.CurrentLocationChanged;
            }

            if (config.EnableTakingHayFromHoppersAnytime)
            {
                LocationEvents.CurrentLocationChanged += this.HandleHopperLocationChanged;
                LocationEvents.LocationObjectsChanged += this.HandleHopperMaybePlacedDown;
            }
        }

        //Update tick - check for removed grass and spawn hay if appropriate
        private void EighthUpdateTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation?.terrainFeatures == null || lastTerrainFeatures == null || Game1.currentLocation != currentLocation)
                return;

            foreach (KeyValuePair<Vector2, TerrainFeature> item in lastTerrainFeatures)
                if (!Game1.currentLocation.terrainFeatures.Contains(item) && item.Value is Grass)
                    if (((Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)((double)Game1.uniqueIDForThisGame + (double)item.Key.X * 1000.0 + (double)item.Key.Y * 11.0))).NextDouble() < 0.5))
                        if (Game1.player.CurrentTool is MeleeWeapon && (Game1.player.CurrentTool.Name.Contains("Scythe") || Game1.player.CurrentTool.parentSheetIndex == 47))
                        {
                            if (IsWithinRange(Game1.player.getTileLocation(), item.Key, 3))
                            {
                                if (dropGrassStarterRandom.NextDouble() < config.ChanceToDropGrassStarterInsteadOfHay)
                                {
                                    AttemptToGiveGrassStarter(item.Key, Game1.getFarm().piecesOfHay == Utility.numSilos() * 240);
                                }
                                else if (Game1.getFarm().tryToAddHay(1) != 0)
                                    if (!BetterHayGrass.TryAddItemToInventory(178) && config.DropHayOnGroundIfNoRoomInInventory)
                                        BetterHayGrass.DropOnGround(item.Key, 178);
                            }
                        }

            lastTerrainFeatures = Game1.currentLocation?.terrainFeatures?.ToDictionary(entry => entry.Key,
                                  entry => entry.Value);
        }

        //Correctly give grass starters instead of hay when silos are full and not full
        private void AttemptToGiveGrassStarter(Vector2 location, bool silosAreFull)
        {
            if (silosAreFull)
            {
                if (!BetterHayGrass.TryAddItemToInventory(297) && config.DropHayOnGroundIfNoRoomInInventory)
                    BetterHayGrass.DropOnGround(location, 297);
            }
            else
            {
                bool added = BetterHayGrass.TryAddItemToInventory(297);
                if (!added && config.DropHayOnGroundIfNoRoomInInventory)
                {
                    BetterHayGrass.DropOnGround(location, 297);
                    added = true;
                }
                if (added)
                    Game1.getFarm().piecesOfHay--;
            }
        }

        //Returns whether the first vector is with range of the second, in euclidian distance
        private bool IsWithinRange(Vector2 first, Vector2 second, double range)
        {
            if (second == null || first == null)
                return false;
            return Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)) < range;
        }

        //Update the last list of terrain features
        private void CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            lastTerrainFeatures = Game1.currentLocation?.terrainFeatures?.ToDictionary(entry => entry.Key,
                                  entry => entry.Value);
        }


        //Revert all hay anytime hoppers to hoppers
        private void BeforeSave(object sender, EventArgs e)
        {
            if (config.EnableTakingHayFromHoppersAnytime)
                ConvertHopper<BetterHayHopper, SObject>(this.currentLocation);
        }


        //Revert the hoppers in the location player left to normal hoppers, convert hoppers in new location to hay anytime hoppers
        private void HandleHopperLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {

            ConvertHopper<BetterHayHopper, SObject>(e.PriorLocation);
            ConvertHopper<SObject, BetterHayHopper>(e.NewLocation);
            currentLocation = e.NewLocation;
        }

        //Try and convert a placed down hopper to a hay anytime hopper
        private void HandleHopperMaybePlacedDown(object sender, EventArgsLocationObjectsChanged e)
        {
            ConvertHopper<SObject, BetterHayHopper>(e.NewObjects);
        }

        //Converts all hoppers in location that are FromType to ToType
        private void ConvertHopper<FromType, ToType>(GameLocation location) where ToType : SObject where FromType : SObject
        {
            ConvertHopper<FromType, ToType>(location?.Objects);
        }

        //Converts all hoppers in Objects to ToType that are FromType
        private void ConvertHopper<FromType, ToType>(SerializableDictionary<Vector2, SObject> Objects) where ToType : SObject where FromType : SObject
        {
            if (Objects == null)
                return;

            IList<Vector2> hopperLocations = new List<Vector2>();

            foreach (KeyValuePair<Vector2, SObject> kvp in Objects)
            {
                if (typeof(ToType) == typeof(SObject) ? kvp.Value is FromType : kvp.Value.name.Contains("Hopper"))
                {
                    hopperLocations.Add(kvp.Key);
                    break;
                }
            }

            foreach (Vector2 hopperLocation in hopperLocations)
            {
                Objects.Remove(hopperLocation);
                Objects.Add(hopperLocation, (ToType)Activator.CreateInstance(typeof(ToType), new object[] { hopperLocation, 99, false }));
            }

        }

    }
}
