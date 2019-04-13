using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
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
        private readonly IDictionary<string, IList<Door>> doors = new Dictionary<string, IList<Door>>();

        private readonly IMonitor monitor;
        public DoorManager(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void AddDoorsToLocation(string locationName, IList<Door> doorsInLocation)
        {
            // Load doors based on the provided content packs.
            this.doors[locationName] = doorsInLocation;
        }

        public bool WasProcessed(string locationName)
        {
            return this.doors.ContainsKey(locationName);
        }

        public void InitializeDoorStates(string locationName, IDictionary<Point, State> initialPositions)
        {
            if (!this.doors.TryGetValue(locationName, out IList<Door> doorsInLocation))
                return;

            foreach (Door door in doorsInLocation)
            {
                door.State = initialPositions != null && initialPositions.TryGetValue(door.Position, out State doorPosition) ? doorPosition : State.Closed;
            }
        }

        public IDictionary<string, IList<Door>> GetDoors()
        {
            return this.doors;
        }

        public IDictionary<Point, State> GetStates(string locationName)
        {
            return !this.doors.TryGetValue(locationName, out IList<Door> doorsInLocation) ? null : doorsInLocation.ToDictionary(door => door.Position, door => door.State);
        }

        public void ToggleDoor(string locationName, Point position)
        {
            if (!this.doors.TryGetValue(locationName, out IList<Door> doorsInLocation))
                return;

            doorsInLocation.FirstOrDefault(door => door.Position == position)?.Toggle(true);
        }

        public bool TryToggleDoor(string locationName, Point farmerPos, out Door toggledDoor)
        {
            toggledDoor = null;

            if (!this.doors.TryGetValue(locationName, out IList<Door> doorsInLocation))
                return false;

            foreach (Door door in doorsInLocation)
            {
                double distance = Math.Sqrt(Math.Pow(door.Position.X - farmerPos.X, 2) + Math.Pow(door.Position.Y - farmerPos.Y, 2));
                if (distance < 2 && door.Toggle(false))
                {
                    toggledDoor = door;
                    return true;
                }
            }

            return false;
        }

        public bool IsDoorCollisionAt(string locationName, Rectangle position)
        {
            return this.doors.TryGetValue(locationName, out IList<Door> doorsInLocation) && doorsInLocation.Where(door => door.State == State.Closed).Any(door => door.CollisionInfo.Intersects(position));
        }

        public void Reset()
        {
            foreach(Door door in this.doors.SelectMany(doorsByLoc => doorsByLoc.Value))
                door.RemoveFromMap();
            this.doors.Clear();
        }
    }
}