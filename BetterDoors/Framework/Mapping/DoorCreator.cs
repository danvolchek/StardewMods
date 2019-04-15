using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping.Properties;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace BetterDoors.Framework.Mapping
{
    /// <summary>Creates doors out of door definitions in maps.</summary>
    internal class DoorCreator
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages information about the tiles needed to draw doors.</summary>
        private readonly GeneratedDoorTileInfoManager doorTileInfoManager;

        /// <summary>Callback timer for door animations.</summary>
        private readonly CallbackTimer timer;

        /// <summary>Queues door definitions errors.</summary>
        private readonly ErrorQueue errorQueue;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="doorTileInfoManager">Manages information about the tiles needed to draw doors.</param>
        /// <param name="timer">Callback timer for door animations.</param>
        /// <param name="errorQueue">Error manager for reading door definitions from map files.</param>
        public DoorCreator(GeneratedDoorTileInfoManager doorTileInfoManager, CallbackTimer timer, ErrorQueue errorQueue)
        {
            this.doorTileInfoManager = doorTileInfoManager;
            this.errorQueue = errorQueue;
            this.timer = timer;
        }

        /// <summary>Finds every door in a map.</summary>
        /// <param name="map">The map to look in.</param>
        /// <param name="pendingDoors">The doors that were found.</param>
        /// <returns>Whether any doors were found.</returns>
        public bool FindDoorsInLocation(Map map, out IList<PendingDoor> pendingDoors)
        {
            pendingDoors = new List<PendingDoor>();

            Layer backLayer = map?.GetLayer("Back");
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

                    // If there's no version property log an error if other properties are present.
                    if (!tile.Properties.TryGetValue(MapDoorVersion.PropertyKey, out PropertyValue doorVersionValue))
                    {
                        if (tile.Properties.ContainsKey(MapDoor.PropertyKey) || tile.Properties.ContainsKey(MapDoorExtras.PropertyKey))
                        {
                            this.errorQueue.AddError($"The door at ({x},{y}) is malformed. Info: Missing a {MapDoorVersion.PropertyKey} property.");
                        }

                        continue;
                    }

                    // Log an error if the version is invalid.
                    if(!MapDoorVersion.TryParseString(doorVersionValue.ToString(), out string error, out MapDoorVersion version))
                    {
                        this.errorQueue.AddError($"The {MapDoorVersion.PropertyKey} property at ({x},{y}) is malformed. Info: {error}.");
                        continue;
                    }

                    // Log an error if the door property is missing.
                    if (!tile.Properties.TryGetValue(MapDoor.PropertyKey, out PropertyValue doorValue))
                    {
                        this.errorQueue.AddError($"The door at ({x},{y}) is malformed. Info: No {MapDoor.PropertyKey} property was found.");
                        continue;
                    }

                    // Log an error if the door property is invalid.
                    if (!MapDoor.TryParseString(doorValue.ToString(), version.PropertyVersion, out error, out MapDoor property))
                    {
                        this.errorQueue.AddError($"The {MapDoor.PropertyKey} property at ({x},{y}) is malformed. Info: {error}.");
                        continue;
                    }

                    // Log an error if the door extras property is present but invalid.
                    MapDoorExtras extras = new MapDoorExtras();
                    if (tile.Properties.TryGetValue(MapDoorExtras.PropertyKey, out PropertyValue doorExtraValue) && !MapDoorExtras.TryParseString(doorExtraValue.ToString(), version.PropertyVersion, out error, out extras))
                    {
                        this.errorQueue.AddError($"The {MapDoorExtras.PropertyKey} property at ({x},{y}) is malformed. Info: {error}.");
                        continue;
                    }

                    // Log an error for invalid door and extras property combinations.
                    if (property.Orientation == Orientation.Horizontal && extras.IsDoubleDoor)
                    {
                        this.errorQueue.AddError($"The door at ({x},{y}) is invalid. Info: Horizontal doors can't be double doors.");
                        continue;
                    }

                    foundDoors = true;

                    // Record sprite request.
                    this.doorTileInfoManager.RegisterDoorRequest(property.ModId, property.DoorName, property.Orientation, property.OpeningDirection);

                    // Mark door as pending.
                    pendingDoors.Add(new PendingDoor(new Point(x, y), property, map, extras, this.timer));
                }
            }

            this.errorQueue.PrintErrors("Found some errors when parsing doors from maps:");

            return foundDoors;
        }

        /// <summary>Creates doors.</summary>
        /// <param name="pendingDoors">The pending doors to use.</param>
        /// <returns>The created doors.</returns>
        public IList<Door> CreateDoors(IList<PendingDoor> pendingDoors)
        {
            IList<Door> foundDoors = new List<Door>();

            foreach (PendingDoor pendingDoor in pendingDoors)
            {
                // Get the right door type to create.
                if (!this.doorTileInfoManager.GetGeneratedTileInfo(pendingDoor.Property.ModId, pendingDoor.Property.DoorName, pendingDoor.Property.Orientation, pendingDoor.Property.OpeningDirection, out string error, out GeneratedDoorTileInfo tileInfo))
                {
                    this.errorQueue.AddError($"The tile property at {pendingDoor.Position.X} {pendingDoor.Position.Y} is invalid. Info: {error}.");
                    continue;
                }

                foundDoors.Add(pendingDoor.ToDoor(tileInfo));
            }

            this.errorQueue.PrintErrors("Found some errors when creating doors from maps:");

            return foundDoors;
        }
    }
}