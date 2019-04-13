using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework
{
    /// <summary>
    /// Creates and then manages doors.
    /// </summary>
    internal class DoorManager : IResetable
    {
        public IDictionary<GameLocation, IList<Door>> Doors { get; private set; }

        public void Init(IDictionary<GameLocation, IList<Door>> doors, IDictionary<string, IDictionary<Point, State>> initialPositions)
        {
            // Load doors based on the provided content packs.
            this.Doors = doors;
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
                {
                    door.Toggle();
                }
            }
        }


        public bool IsDoorCollisionAt(GameLocation location, Rectangle position)
        {
            return this.Doors.TryGetValue(location, out IList<Door> doors) && doors.Where(door => door.State == State.Closed).Any(door => door.CollisionInfo.Intersects(position));
        }

        public void Reset()
        {
            foreach(Door door in this.Doors.SelectMany(doorsByLoc => doorsByLoc.Value))
                door.RemoveFromMap();
        }
    }
}