using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace BetterDoors.Framework
{
    /// <summary>
    /// Creates and then manages doors.
    /// </summary>
    internal class DoorManager
    {
        private readonly DoorCreator doorCreator;
        private readonly MapModifier mapModifier;

        public IDictionary<GameLocation, IList<Door>> Doors { get; private set; }

        public DoorManager(DoorCreator doorCreator, MapModifier mapModifier)
        {
            this.doorCreator = doorCreator;
            this.mapModifier = mapModifier;
        }

        public void Init(IDictionary<string, IDictionary<Point, State>> initialPositions)
        {
            // Load doors based on the provided content packs.
            this.Doors = this.doorCreator.FindAndCreateDoors();
            // Modify the maps that have doors with tile sheets so the doors can be drawn.
            this.mapModifier.AddTileSheetsToMaps(this.Doors);
            // Set the positions of the doors to what they previously were.
            this.InitializeDoorStates(initialPositions);
        }

        private void InitializeDoorStates(IDictionary<string, IDictionary<Point, State>> initialStatesByLocation)
        {
            foreach (KeyValuePair<GameLocation, IList<Door>> doorLocation in this.Doors)
            {
                if (initialStatesByLocation.TryGetValue(doorLocation.Key.Name, out IDictionary<Point, State> doorStatesByPoint))
                {
                    foreach (Door door in doorLocation.Value)
                        door.State = doorStatesByPoint.TryGetValue(door.Position, out State doorPosition) ? doorPosition : State.Closed;
                }
                else
                {
                    foreach (Door door in doorLocation.Value)
                        door.State = State.Closed;
                }
            }
        }

        public void TryToggleDoor(GameLocation location, Point farmerPos)
        {
            if (!this.Doors.TryGetValue(location, out IList<Door> doors))
                return;

            foreach (Door door in doors)
            {
                double distance = Math.Sqrt(Math.Pow(door.Position.X - farmerPos.X, 2) + Math.Pow(door.Position.Y - farmerPos.Y, 2));
                if ((distance > 0 || door.State != State.Open) && distance < 2)
                    door.Toggle();
            }
        }
    }
}