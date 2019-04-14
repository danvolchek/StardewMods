using System;
using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using BetterDoors.Framework.Utility;
using StardewValley;

namespace BetterDoors.Framework
{
    /// <summary>
    /// Creates and then manages doors.
    /// </summary>
    internal class DoorManager : IResetable
    {
        private readonly Action<Door> onToggledDoor;
        private readonly IDictionary<string, IDictionary<Point, Door>> doors = new Dictionary<string, IDictionary<Point, Door>>();
        private ISet<Door> doorsNearPlayers = new HashSet<Door>();

        public DoorManager(Action<Door> onToggledDoor)
        {
            this.onToggledDoor = onToggledDoor;
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

        public Dictionary<string, Dictionary<Point, State>> GetDoorStates()
        {
            return this.doors.ToDictionary(item => item.Key, item => item.Value.ToDictionary(item2 => item2.Key, item2 => item2.Value.State));
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

        public void UserClicked(string locationName, Point position)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;


            for (int y = 0; y < 2; y++)
            {
                if (doorsInLocation.TryGetValue(new Point(position.X, position.Y + y), out Door door))
                {
                    foreach (Door toggleDoor in this.TryToggleDoor(door, doorsInLocation, false))
                        this.onToggledDoor(toggleDoor);
                }
            }
        }

        public void ToggleAutomaticDoors(GameLocation location)
        {
            if (!this.doors.ContainsKey(Utils.GetLocationName(location)))
                return;

            ISet<Door> nearDoors = new HashSet<Door>(this.GetDoorsNearPlayers(location));

            ISet<Door> newInRangeDoors = new HashSet<Door>(nearDoors);
            ISet<Door> newOutOfRangeDoors = new HashSet<Door>(this.doorsNearPlayers);

            newInRangeDoors.ExceptWith(this.doorsNearPlayers); // Now only has doors that just entered in range
            newOutOfRangeDoors.ExceptWith(nearDoors); // Now only has doors that just exited the range

            // Only toggle door that should be open that aren't open, doors that should be closed and aren't closed, and doors that aren't both pending for opening and closing.
            HashSet<Door> doorsToToggle = new HashSet<Door>(newInRangeDoors.Where(door => door.State != State.Open));
            doorsToToggle.SymmetricExceptWith(newOutOfRangeDoors.Where(door => door.State != State.Closed));

            this.doorsNearPlayers = nearDoors;

            foreach (Door toggleDoor in doorsToToggle.Where(door => door.Toggle(true)))
                this.onToggledDoor(toggleDoor);
        }

        private IEnumerable<Door> GetDoorsNearPlayers(GameLocation location)
        {
            if (!this.doors.TryGetValue(Utils.GetLocationName(location), out IDictionary<Point, Door> doorsInLocation))
                yield break;

            foreach (Farmer farmer in location.farmers)
            {
                for (int i = -2; i < 3; i ++)
                {
                    if (doorsInLocation.TryGetValue(new Point(farmer.getTileX() + i, farmer.getTileY()), out Door door) && door.Extras.IsAutomaticDoor && door.Orientation == Orientation.Horizontal)
                    {
                        yield return door;

                        if (DoorManager.GetDoubleDoor(door, doorsInLocation, out Door doubleDoor))
                            yield return doubleDoor;
                    }

                    if (doorsInLocation.TryGetValue(new Point(farmer.getTileX(), farmer.getTileY() + i), out door) && door.Extras.IsAutomaticDoor && door.Orientation == Orientation.Vertical)
                    {
                        yield return door;

                        if (DoorManager.GetDoubleDoor(door, doorsInLocation, out Door doubleDoor))
                            yield return doubleDoor;
                    }
                }
            }
        }

        public void ResetDoorsNearPlayers()
        {
            this.doorsNearPlayers.Clear();
        }

        private IEnumerable<Door> TryToggleDoor(Door door, IDictionary<Point, Door> doorsInLocation, bool force)
        {
            if (door.Toggle(force))
            {
                yield return door;
                if (DoorManager.GetDoubleDoor(door, doorsInLocation, out Door doubleDoor) && doubleDoor.Toggle(force))
                    yield return doubleDoor;
            }
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

        private static bool GetDoubleDoor(Door door, IDictionary<Point, Door> doorsInLocation, out Door doubleDoor)
        {
            doubleDoor = null;

            if (!door.Extras.IsDoubleDoor)
                return false;

            for (int i = -1; i < 2; i += 2)
            {
                Point adjacentPoint = new Point(door.Position.X + (door.Orientation == Orientation.Vertical ? i : 0), door.Position.Y + (door.Orientation == Orientation.Horizontal ? i : 0));
                if (doorsInLocation.TryGetValue(adjacentPoint, out doubleDoor) && doubleDoor.Extras.IsDoubleDoor)
                    return true;
            }

            return false;
        }
    }
}