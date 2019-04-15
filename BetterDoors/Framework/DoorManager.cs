using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework
{
    /// <summary>Manages doors.</summary>
    internal class DoorManager
    {
        /*********
         ** Fields
         *********/
        /// <summary>The action to take when a door is toggled.</summary>
        private readonly Action<Door> onToggledDoor;

        /// <summary>All the doors that are currently loaded.</summary>
        private readonly IDictionary<string, IDictionary<Point, Door>> doors = new Dictionary<string, IDictionary<Point, Door>>();

        /// <summary>Doors currently near any players.</summary>
        private IList<Door> doorsNearPlayers = new List<Door>();

        /*********
        ** Public methods
        *********/
        /// <summary>Constructs an instance.</summary>
        /// <param name="onToggledDoor">The action to take when a door is toggled.</param>
        public DoorManager(Action<Door> onToggledDoor)
        {
            this.onToggledDoor = onToggledDoor;
        }

        /// <summary>Gives the manager doors to manage.</summary>
        /// <param name="locationName">The location to place the doors in.</param>
        /// <param name="doorsInLocation">The doors in the location.</param>
        public void ManageDoors(string locationName, IList<Door> doorsInLocation)
        {
            this.doors[locationName] = doorsInLocation.ToDictionary(door => door.Position, door => door);
        }

        /// <summary>Sets door states, defaulting to closed if not provided.</summary>
        /// <param name="locationName">The location to set states for.</param>
        /// <param name="statesByPosition">The states of the door positions.</param>
        public void SetDoorStates(string locationName, IDictionary<Point, State> statesByPosition)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;

            foreach (Door door in doorsInLocation.Values)
            {
                door.State = statesByPosition != null && statesByPosition.TryGetValue(door.Position, out State doorPosition) ? doorPosition : State.Closed;
            }
        }

        /// <summary>Gets the current state of every door.</summary>
        /// <returns>The state of every door.</returns>
        public Dictionary<string, Dictionary<Point, State>> GetEveryDoorState()
        {
            return this.doors.ToDictionary(item => item.Key, item => item.Value.ToDictionary(item2 => item2.Key, item2 => item2.Value.State));
        }

        /// <summary>Gets the current state of every door in a location.</summary>
        /// <param name="locationName">The location to get doors for.</param>
        /// <returns>The states of the doors.</returns>
        public IDictionary<Point, State> GetDoorStatesInLocation(string locationName)
        {
            return !this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation) ? null : doorsInLocation.ToDictionary(door => door.Key, door => door.Value.State);
        }

        /// <summary>Forcefully toggles a door state if found at the position.</summary>
        /// <param name="locationName">The location to toggle the door in.</param>
        /// <param name="position">The position to look for a door at.</param>
        public void ToggleDoor(string locationName, Point position)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;

            // Don't callback forceful changes.
            if(doorsInLocation.TryGetValue(position, out Door door))
                door.Toggle(true);
        }

        /// <summary>Unforcefully toggles a door state if found at the position or near it.</summary>
        /// <param name="locationName">The location to toggle the door in.</param>
        /// <param name="position">The position to look for a door at.</param>
        public void FuzzyToggleDoor(string locationName, Point position)
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

        /// <summary>Toggles automatic doors as necessary.</summary>
        /// <param name="location">The location to toggle doors in.</param>
        public void ToggleAutomaticDoors(GameLocation location)
        {
            if (!this.doors.ContainsKey(Utils.GetLocationName(location)))
                return;

            // Get currently near doors.
            IList<Door> nearDoors = this.GetDoorsNearPlayers(location).ToList();

            // Find doors that entered and exited the range.
            ISet<Door> newInRangeDoors = new HashSet<Door>(nearDoors);
            ISet<Door> newOutOfRangeDoors = new HashSet<Door>(this.doorsNearPlayers);
            newInRangeDoors.ExceptWith(this.doorsNearPlayers);
            newOutOfRangeDoors.ExceptWith(nearDoors);

            // Find doors to toggle that:
            // - Are not both in and out of the range (can be caused by double doors).
            // - Should be open and are not open.
            // - Should be closed and are not closed.
            HashSet<Door> doorsToToggle = new HashSet<Door>(newInRangeDoors.Where(door => door.State != State.Open));
            doorsToToggle.SymmetricExceptWith(newOutOfRangeDoors.Where(door => door.State != State.Closed));

            this.doorsNearPlayers = nearDoors;

            foreach (Door toggleDoor in doorsToToggle.Where(door => door.Toggle(true)))
                this.onToggledDoor(toggleDoor);
        }

        /// <summary>Checks whether a location was already processed.</summary>
        /// <param name="locationName">The location to check.</param>
        /// <returns>Whether the location was already processed.</returns>
        public bool WasProcessed(string locationName)
        {
            return this.doors.ContainsKey(locationName);
        }

        /// <summary>Resets info needed to make doors automatic.</summary>
        public void ResetAutomaticDoorTracking()
        {
            this.doorsNearPlayers.Clear();
        }

        /// <summary>Checks for a closed door.</summary>
        /// <param name="locationName">The location to check in.</param>
        /// <param name="position">The position to check at.</param>
        /// <returns>Whether a closed door was found.</returns>
        public bool IsClosedDoorAt(string locationName, Rectangle position)
        {
            return this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation) && doorsInLocation.Values.Where(door => door.State == State.Closed).Any(door => door.CollisionInfo.Intersects(position));
        }

        /// <summary>Resets the manager, removing each door from its map.</summary>
        public void Reset()
        {
            foreach(Door door in this.doors.SelectMany(doorsByLoc => doorsByLoc.Value.Values))
                door.RemoveFromMap();
            this.doors.Clear();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Tries to toggle a door, also toggling the accompanying double door if successful.</summary>
        /// <param name="door">The door to toggle.</param>
        /// <param name="doorsInLocation">The other doors in the location</param>
        /// <param name="force">Whether to toggle forcefully or not.</param>
        /// <returns>All doors that were toggled.</returns>
        private IEnumerable<Door> TryToggleDoor(Door door, IDictionary<Point, Door> doorsInLocation, bool force)
        {
            if (door.Toggle(force))
            {
                yield return door;
                if (DoorManager.GetDoubleDoor(door, doorsInLocation, out Door doubleDoor) && doubleDoor.Toggle(force))
                    yield return doubleDoor;
            }
        }

        /// <summary>Gets all doors near any player.</summary>
        /// <param name="location">The location to look in.</param>
        /// <returns>The doors that were found.</returns>
        private IEnumerable<Door> GetDoorsNearPlayers(GameLocation location)
        {
            if (!this.doors.TryGetValue(Utils.GetLocationName(location), out IDictionary<Point, Door> doorsInLocation))
                yield break;

            foreach (Farmer farmer in location.farmers)
            {
                for (int i = -2; i < 3; i++)
                {
                    // Search along the x axis for horizontal doors and along the y axis for vertical doors (parallel to the hallway direction).

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

        /// <summary>Gets the associated double door for the given door, if both are double doors.</summary>
        /// <param name="door">The door to use.</param>
        /// <param name="doorsInLocation">The doors in the location.</param>
        /// <param name="doubleDoor">The found double door.</param>
        /// <returns>Whether a double door was found.</returns>
        private static bool GetDoubleDoor(Door door, IDictionary<Point, Door> doorsInLocation, out Door doubleDoor)
        {
            doubleDoor = null;

            if (!door.Extras.IsDoubleDoor)
                return false;

            for (int i = -1; i < 2; i += 2)
            {
                // Search along the x axis for vertical doors and along the y axis for horizontal doors (perpendicular to the hallway direction).
                Point adjacentPoint = new Point(door.Position.X + (door.Orientation == Orientation.Vertical ? i : 0), door.Position.Y + (door.Orientation == Orientation.Horizontal ? i : 0));
                if (doorsInLocation.TryGetValue(adjacentPoint, out doubleDoor) && doubleDoor.Extras.IsDoubleDoor)
                    return true;
            }

            return false;
        }
    }
}