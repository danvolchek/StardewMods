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
        private readonly IDictionary<string, IDictionary<Point, Door>> doors = new Dictionary<string, IDictionary<Point, Door>>();

        private readonly IMonitor monitor;
        public DoorManager(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void AddDoorsToLocation(string locationName, IList<Door> doorsInLocation)
        {
            // Load doors based on the provided content packs.
            this.doors[locationName] = doorsInLocation.ToDictionary(door => door.Position, door => door);
        }

        public bool WasProcessed(string locationName)
        {
            return this.doors.ContainsKey(locationName);
        }

        public void InitializeDoorStates(string locationName, IDictionary<Point, State> initialPositions)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;

            foreach (Door door in doorsInLocation.Values)
            {
                door.State = initialPositions != null && initialPositions.TryGetValue(door.Position, out State doorPosition) ? doorPosition : State.Closed;
            }
        }

        public IDictionary<string, IDictionary<Point, Door>> GetDoors()
        {
            return this.doors;
        }

        public IDictionary<Point, State> GetStates(string locationName)
        {
            return !this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation) ? null : doorsInLocation.ToDictionary(door => door.Key, door => door.Value.State);
        }

        public void ToggleDoor(string locationName, Point position)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;

            if(doorsInLocation.TryGetValue(position, out Door door))
                door.Toggle(true);
        }

        public bool TryToggleDoor(string locationName, Point position, out IList<Door> toggledDoors)
        {
            toggledDoors = new List<Door>();

            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return false;


            for (int y = 0; y < 2; y++)
            {
                if (doorsInLocation.TryGetValue(new Point(position.X, position.Y + y), out Door door) && door.Toggle(false))
                {
                    toggledDoors.Add(door);

                    if (door.Extras.IsDoubleDoor)
                    {
                        foreach(Door adjacentDoor in DoorManager.GetAdjacentDoors(door, doorsInLocation))
                            if(adjacentDoor.Toggle(false))
                                toggledDoors.Add(adjacentDoor);
                    }
                }
            }


            return false;
        }

        public bool IsDoorCollisionAt(string locationName, Rectangle position)
        {
            return this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation) && doorsInLocation.Values.Where(door => door.State == State.Closed).Any(door => door.CollisionInfo.Intersects(position));
        }

        public void Reset()
        {
            foreach(Door door in this.doors.SelectMany(doorsByLoc => doorsByLoc.Value.Values))
                door.RemoveFromMap();
            this.doors.Clear();
        }

        private static IEnumerable<Door> GetAdjacentDoors(Door door, IDictionary<Point, Door> doorsInLocation)
        {
            for (int i = -1; i < 2; i += 2)
            {
                Point adjacentPoint = new Point(door.Position.X + (door.Orientation == Orientation.Vertical ? i : 0), door.Position.Y + (door.Orientation == Orientation.Horizontal ? i : 0));
                if (doorsInLocation.TryGetValue(adjacentPoint, out Door adjacentDoor))
                    yield return adjacentDoor;
            }
        }
    }
}