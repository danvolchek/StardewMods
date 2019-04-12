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
    internal class DoorCreator : IResetable
    {
        private readonly IList<PendingDoor> pendingDoors = new List<PendingDoor>();
        private readonly GeneratedSpriteManager spriteManager;
        private readonly CallbackTimer timer;
        private readonly IMonitor monitor;

        public DoorCreator(GeneratedSpriteManager spriteManager, CallbackTimer timer, IMonitor monitor)
        {
            this.spriteManager = spriteManager;
            this.monitor = monitor;
            this.timer = timer;
        }

        public bool FindDoors()
        {
            bool foundDoors = false;
            // Search for doors in all maps.
            foreach (GameLocation location in DoorCreator.GetAllLocations())
            {
                Layer backLayer = location?.Map.GetLayer("Back");
                if (backLayer == null)
                {
                    continue;
                }

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

                        foundDoors = true;

                        // Record sprite request.
                        this.spriteManager.RegisterDoorSpriteRequest(property.ModId, property.DoorName, property.Orientation, property.OpeningDirection);

                        // Mark door as pending.
                        this.pendingDoors.Add(new PendingDoor(location, new Point(x, y), property, this.timer));
                    }
                }
            }

            return foundDoors;
        }

        public IDictionary<GameLocation, IList<Door>> CreateDoors()
        {
            IDictionary<GameLocation, IList<Door>> foundDoors = new Dictionary<GameLocation, IList<Door>>();

            foreach (PendingDoor pendingDoor in this.pendingDoors)
            {
                // Get the right door type to create.
                if (!this.spriteManager.GetDoorSprite(pendingDoor.Property.ModId, pendingDoor.Property.DoorName, pendingDoor.Property.Orientation, pendingDoor.Property.OpeningDirection, out string error, out GeneratedDoorTileInfo tileInfo))
                {
                    Utils.LogContentPackError(this.monitor, $"The tile property at {pendingDoor.Position.X} {pendingDoor.Position.Y} is invalid. Info: {error}.");
                    continue;
                }

                if(!foundDoors.ContainsKey(pendingDoor.Location))
                    foundDoors[pendingDoor.Location] = new List<Door>();

                foundDoors[pendingDoor.Location].Add(pendingDoor.ToDoor(tileInfo));
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

        public void Reset()
        {
            this.pendingDoors.Clear();
        }
    }
}