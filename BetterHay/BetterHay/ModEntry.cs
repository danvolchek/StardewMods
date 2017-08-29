using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
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

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            SaveEvents.BeforeSave += this.BeforeSave;
            if (config.EnableGettingHayFromGrassAnytime)
            {
                SaveEvents.AfterSave += this.ConvertAllGrassToBetterHayGrass;
                SaveEvents.AfterLoad += this.ConvertAllGrassToBetterHayGrass;
            }

            if (config.EnableTakingHayFromHoppersAnytime)
            {
                LocationEvents.CurrentLocationChanged += this.HandleHopperLocationChanged;
                LocationEvents.LocationObjectsChanged += this.HandleHopperMaybePlacedDown;
            }
        }

        //Revert all hay anytime grass to grass and hay anytime hoppers to hoppers
        private void BeforeSave(object sender, EventArgs e)
        {
            if (config.EnableGettingHayFromGrassAnytime)
                ConvertGrass<BetterHayGrass, Grass>();

            if (config.EnableTakingHayFromHoppersAnytime)
                ConvertHopper<BetterHayHopper, SObject>(this.currentLocation);
        }

        //Converts all grass from normal grass to hay anytime grass
        private void ConvertAllGrassToBetterHayGrass(object sender, EventArgs e)
        {
            ConvertGrass<Grass, BetterHayGrass>();
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

        //Converts all grass from FromType to ToType
        private void ConvertGrass<FromType, ToType>() where FromType : Grass where ToType : Grass
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> kvp in location.terrainFeatures.Where(item => item.Value is FromType).ToList())
                {
                    FromType g = (FromType)kvp.Value;
                    location.terrainFeatures.Remove(kvp.Key);
                    location.terrainFeatures.Add(kvp.Key, (ToType)Activator.CreateInstance(typeof(ToType), new object[] { g.grassType, g.numberOfWeeds }));
                }
            }
        }



    }
}
