using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BetterHay
{
    public class ModEntry : Mod
    {
        //Config
        public static ModConfig Config;

        //Current player location
        public GameLocation CurrentLocation;

        private Random dropGrassStarterRandom;

        //Last list of terrain features
        private Dictionary<Vector2, TerrainFeature> lastTerrainFeatures;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            this.dropGrassStarterRandom = new Random();
            SaveEvents.BeforeSave += this.BeforeSave;

            if (Config.EnableGettingHayFromGrassAnytime)
            {
                GameEvents.EighthUpdateTick += this.EighthUpdateTick;
                PlayerEvents.Warped += this.PlayerEvents_Warped;
            }

            if (Config.EnableTakingHayFromHoppersAnytime)
            {
                PlayerEvents.Warped += this.HandleHopperLocationChanged;
                LocationEvents.ObjectsChanged += this.HandleHopperMaybePlacedDown;
            }
        }

        //Update tick - check for removed grass and spawn hay if appropriate
        private void EighthUpdateTick(object sender, EventArgs e)
        {
            if (Game1.player?.currentLocation?.terrainFeatures == null || this.lastTerrainFeatures == null ||
                Game1.player.currentLocation != this.CurrentLocation)
                return;

            foreach (KeyValuePair<Vector2, TerrainFeature> item in this.lastTerrainFeatures)
                if (!Game1.player.currentLocation.terrainFeatures.FieldDict.ContainsKey(item.Key) && item.Value is Grass grass && grass.numberOfWeeds.Value <= 0 && grass.grassType.Value == 1)
                    if ((Game1.IsMultiplayer
                            ? Game1.recentMultiplayerRandom
                            : new Random((int) (Game1.uniqueIDForThisGame + item.Key.X * 1000.0 + item.Key.Y * 11.0)))
                        .NextDouble() < 0.5)
                        if (Game1.player.CurrentTool is MeleeWeapon &&
                            (Game1.player.CurrentTool.Name.Contains("Scythe") ||
                             Game1.player.CurrentTool.ParentSheetIndex == 47))
                            if (this.IsWithinRange(Game1.player.getTileLocation(), item.Key, 3))
                            {
                                if (this.dropGrassStarterRandom.NextDouble() <
                                    Config.ChanceToDropGrassStarterInsteadOfHay)
                                    this.AttemptToGiveGrassStarter(item.Key,
                                        Game1.getFarm().piecesOfHay.Value == Utility.numSilos() * 240);
                                else if (Game1.getFarm().tryToAddHay(1) != 0)
                                    if (!BetterHayGrass.TryAddItemToInventory(178) &&
                                        Config.DropHayOnGroundIfNoRoomInInventory)
                                        BetterHayGrass.DropOnGround(item.Key, 178);
                            }

            this.lastTerrainFeatures = Game1.player.currentLocation?.terrainFeatures?.FieldDict.ToDictionary(entry => entry.Key,
                entry => entry.Value.Value);
        }

        //Correctly give grass starters instead of hay when silos are full and not full
        private void AttemptToGiveGrassStarter(Vector2 location, bool silosAreFull)
        {
            if (silosAreFull)
            {
                if (!BetterHayGrass.TryAddItemToInventory(297) && Config.DropHayOnGroundIfNoRoomInInventory)
                    BetterHayGrass.DropOnGround(location, 297);
            }
            else
            {
                bool added = BetterHayGrass.TryAddItemToInventory(297);
                if (!added && Config.DropHayOnGroundIfNoRoomInInventory)
                {
                    BetterHayGrass.DropOnGround(location, 297);
                    added = true;
                }

                if (added)
                    Game1.getFarm().piecesOfHay.Value -= 1;
            }
        }

        //Returns whether the first vector is with range of the second, in euclidian distance
        private bool IsWithinRange(Vector2 first, Vector2 second, double range)
        {
            return Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)) < range;
        }

        //Update the last list of terrain features
        private void PlayerEvents_Warped(object sender, EventArgsPlayerWarped e)
        {
            this.lastTerrainFeatures = e.NewLocation?.terrainFeatures?.FieldDict.ToDictionary(item => item.Key, item => item.Value.Value);
            this.CurrentLocation = e.NewLocation;
        }


        //Revert all hay anytime hoppers to hoppers
        private void BeforeSave(object sender, EventArgs e)
        {
            if (Config.EnableTakingHayFromHoppersAnytime)
                foreach(GameLocation loc in Game1.locations)
                    this.ConvertHopper<BetterHayHopper, SObject>(loc);
        }


        //Revert the hoppers in the location player left to normal hoppers, convert hoppers in new location to hay anytime hoppers
        private void HandleHopperLocationChanged(object sender, EventArgsPlayerWarped e)
        {
            //this.Monitor.Log($"Player warped from {e.PriorLocation.Name} to {e.NewLocation.Name}");
            if(!this.AreAnyPlayersInLocation(e.PriorLocation))
                this.ConvertHopper<BetterHayHopper, SObject>(e.PriorLocation);
            if(!this.AreAnyPlayersBesidesMeInLocation(e.NewLocation))
                this.ConvertHopper<SObject, BetterHayHopper>(e.NewLocation);
            this.CurrentLocation = e.NewLocation;
        }

        //Try and convert a placed down hopper to a hay anytime hopper
        private void HandleHopperMaybePlacedDown(object sender, EventArgsLocationObjectsChanged e)
        {
            if(e.Location == Game1.player.currentLocation)
                this.ConvertHopper<SObject, BetterHayHopper>(e.Location);
        }

        //Converts all hoppers in location that are FromType to ToType
        private void ConvertHopper<TFromType, TToType>(GameLocation location)
            where TToType : SObject where TFromType : SObject
        {
            //this.Monitor.Log($"Converting from {typeof(TFromType).Name} to {typeof(TToType).Name} in {location.Name}.");
            this.ConvertHopperImpl<TFromType, TToType>(location?.Objects);
        }

        //Converts all hoppers in Objects to ToType that are FromType
        private void ConvertHopperImpl<TFromType, TToType>(OverlaidDictionary<Vector2, SObject> Objects)
            where TToType : SObject where TFromType : SObject
        {
            if (Objects == null)
                return;

            IList<Vector2> hopperLocations = new List<Vector2>();

            foreach (KeyValuePair<Vector2, SObject> kvp in Objects.Pairs)
                if (kvp.Value.GetType() == typeof(TFromType) && (typeof(TToType) == typeof(SObject) ? kvp.Value is TFromType : kvp.Value.name.Contains("Hopper")))
                {
                    hopperLocations.Add(kvp.Key);
                    break;
                }

            foreach (Vector2 hopperLocation in hopperLocations)
                Objects[hopperLocation] = (TToType) Activator.CreateInstance(typeof(TToType), hopperLocation, 99, false);
        }

        private bool AreAnyPlayersInLocation(GameLocation location)
        {
            return Game1.getOnlineFarmers().Any(farmer => farmer.currentLocation == location);
        }

        private bool AreAnyPlayersBesidesMeInLocation(GameLocation location)
        {
            return Game1.getOnlineFarmers().Any(farmer => farmer != Game1.player && farmer.currentLocation == location);
        }
    }
}