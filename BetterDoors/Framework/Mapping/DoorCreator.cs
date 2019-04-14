using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping.Properties;
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

        public bool FindDoorsInLocation(GameLocation location)
        {
            Layer backLayer = location?.Map.GetLayer("Back");
            if (backLayer == null)
            {
                return false;
            }

            // Search for doors in the provided location.
            bool foundDoors = false;

            for (int x = 0; x < backLayer.LayerWidth; x++)
            {
                for (int y = 0; y < backLayer.LayerHeight; y++)
                {
                    Tile tile = backLayer.Tiles[x, y];

                    if (tile == null)
                    {
                        continue;
                    }

                    if (!tile.Properties.TryGetValue(MapDoorVersion.PropertyKey, out PropertyValue doorVersionValue))
                    {
                        if (tile.Properties.ContainsKey(MapDoorProperty.PropertyKey) || tile.Properties.ContainsKey(MapDoorExtraProperty.PropertyKey))
                        {
                            Utils.LogContentPackError(this.monitor, $"The door at ({x},{y}) is malformed. Info: Missing a {MapDoorVersion.PropertyKey} property.");
                        }

                        continue;
                    }

                    if(!MapDoorVersion.TryParseString(doorVersionValue.ToString(), out string error, out MapDoorVersion version))
                    {
                        Utils.LogContentPackError(this.monitor, $"The {MapDoorVersion.PropertyKey} property at ({x},{y}) is malformed. Info: {error}.");
                        continue;
                    }

                    // Parse and validate the door property.
                    if (!tile.Properties.TryGetValue(MapDoorProperty.PropertyKey, out PropertyValue doorValue) || !MapDoorProperty.TryParseString(doorValue.ToString(), version.PropertyVersion, out error, out MapDoorProperty property))
                    {
                        Utils.LogContentPackError(this.monitor, $"The {MapDoorProperty.PropertyKey} property at ({x},{y}) is malformed. Info: {error}.");
                        continue;
                    }

                    MapDoorExtraProperty extras = new MapDoorExtraProperty();

                    if (tile.Properties.TryGetValue(MapDoorExtraProperty.PropertyKey, out PropertyValue doorExtraValue) && !MapDoorExtraProperty.TryParseString(doorExtraValue.ToString(), version.PropertyVersion, out error, out extras))
                    {
                        Utils.LogContentPackError(this.monitor, $"The {MapDoorExtraProperty.PropertyKey} property at ({x},{y}) is malformed. Info: {error}.");
                        continue;
                    }

                    if (property.Orientation == Orientation.Horizontal && extras.IsDoubleDoor)
                    {
                        Utils.LogContentPackError(this.monitor, $"The door at ({x},{y}) is invalid. Info: Horizontal doors can't be double doors.");
                        continue;
                    }

                    foundDoors = true;

                    // Record sprite request.
                    this.spriteManager.RegisterDoorSpriteRequest(property.ModId, property.DoorName, property.Orientation, property.OpeningDirection);

                    // Mark door as pending.
                    this.pendingDoors.Add(new PendingDoor(location.Map, new Point(x, y), property, extras, this.timer));
                }
            }

            return foundDoors;
        }

        public IList<Door> CreateDoors()
        {
            IList<Door> foundDoors = new List<Door>();

            foreach (PendingDoor pendingDoor in this.pendingDoors)
            {
                // Get the right door type to create.
                if (!this.spriteManager.GetDoorSprite(pendingDoor.Property.ModId, pendingDoor.Property.DoorName, pendingDoor.Property.Orientation, pendingDoor.Property.OpeningDirection, out string error, out GeneratedDoorTileInfo tileInfo))
                {
                    Utils.LogContentPackError(this.monitor, $"The tile property at {pendingDoor.Position.X} {pendingDoor.Position.Y} is invalid. Info: {error}.");
                    continue;
                }

                foundDoors.Add(pendingDoor.ToDoor(tileInfo));
            }

            return foundDoors;
        }

        public void Reset()
        {
            this.pendingDoors.Clear();
        }
    }
}