using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System.Collections.Generic;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace BetterDoors.Framework.Mapping
{
    /// <summary>
    /// Uses content packs and map data to actually create doors.
    /// </summary>
    internal class DoorCreator
    {
        private readonly IMonitor monitor;
        private readonly CallbackTimer timer;
        private readonly DoorSpriteGenerator generator;

        public DoorCreator(IModHelper helper, IMonitor monitor, CallbackTimer timer)
        {
            this.monitor = monitor;
            this.timer = timer;
            this.generator = new DoorSpriteGenerator(helper.Content, this.monitor, Game1.graphics.GraphicsDevice);
        }

        public IDictionary<GameLocation, IList<Door>> FindAndCreateDoors(IList<LoadedContentPackDoorEntry> loadedDoorPacks)
        {
            // Generate different door types from the loaded content packs.
            GeneratedSpriteManager manager = this.generator.GenerateDoorSprites(loadedDoorPacks);

            // Search for doors in all maps.
            IDictionary<GameLocation, IList<Door>> foundDoors = new Dictionary<GameLocation, IList<Door>>();
            foreach (GameLocation location in DoorCreator.GetAllLocations())
            {
                Layer backLayer = location?.Map.GetLayer("Back");
                if (backLayer == null)
                {
                    continue;
                }

                IList<Door> doorsToAdd = new List<Door>();

                for (int x = 0; x < backLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < backLayer.LayerHeight; y++)
                    {
                        Tile tile = backLayer.Tiles[x, y];

                        if (tile == null || !tile.Properties.TryGetValue("Door", out PropertyValue value))
                        {
                            continue;
                        }

                        // Parse and validate the door property.
                        if (!MapDoorProperty.TryParseString(value, out string error, out MapDoorProperty property))
                        {
                            Utils.LogContentPackError(this.monitor, $"The tile property at {x} {y} is malformed. Info: {error}.");
                            continue;
                        }

                        if (!manager.GetDoorSprite(property.ModId, property.DoorName, property.Orientation, property.OpeningDirection, out error, out GeneratedDoorTileInfo tileInfo))
                        {
                            Utils.LogContentPackError(this.monitor, $"The tile property at {x} {y} is invalid. Info: {error}.");
                            continue;
                        }

                        // Finally, create a door at the given tile.
                        doorsToAdd.Add(new Door(new Point(x, y), property.Orientation, location.Map, tileInfo, this.timer));
                    }
                }

                if (doorsToAdd.Count != 0)
                    foundDoors[location] = doorsToAdd;
            }

            return foundDoors;
        }

        private static IEnumerable<GameLocation> GetAllLocations()
        {
            foreach (GameLocation location in Game1.locations)
            {
                yield return location;

                if (!(location is BuildableGameLocation bLoc))
                {
                    continue;
                }

                foreach (Building building in bLoc.buildings)
                    yield return building.indoors.Value;
            }
        }
    }
}