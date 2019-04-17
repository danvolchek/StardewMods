using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Serialization;
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
        /// <summary>Mod configuration.</summary>
        private readonly BetterDoorsModConfig config;

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
        /// <param name="config">Mod configuration.</param>
        /// <param name="onToggledDoor">The action to take when a door is toggled.</param>
        public DoorManager(BetterDoorsModConfig config, Action<Door> onToggledDoor)
        {
            this.config = config;
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
        /// <param name="stateBeforeToggle">The door state before it was toggled.</param>
        public void ToggleDoor(string locationName, Point position, State stateBeforeToggle)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;

            if (!doorsInLocation.TryGetValue(position, out Door door))
                return;

            // If animating in the wrong direction or in the wrong end state, toggle to the right one.
            if ((door.IsAnimating && door.StateBeforeToggle != stateBeforeToggle) || (!door.IsAnimating && door.State == stateBeforeToggle))
            {
                // Don't callback forceful changes.
                door.Toggle(true, true);
            }
        }

        /// <summary>Unforcefully toggles a door state if found at the position or near it.</summary>
        /// <param name="locationName">The location to toggle the door in.</param>
        /// <param name="mouseTile">The position to look for a door at.</param>
        public void MouseToggleDoor(string locationName, Point mouseTile)
        {
            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation))
                return;

            if(DoorManager.TryGetDoorFromMouse(mouseTile, doorsInLocation, out Door door))
                foreach (Door toggleDoor in this.TryToggleDoor(door, doorsInLocation, false))
                    this.onToggledDoor(toggleDoor);
        }

        /// <summary>Toggles automatic doors as necessary.</summary>
        /// <param name="location">The location to toggle doors in.</param>
        public void ToggleAutomaticDoors(GameLocation location)
        {
            if (!this.doors.ContainsKey(Utils.GetLocationName(location)))
                return;

            // Get currently near doors.
            IList<Door> nearDoors = this.GetDoorsNearLocalPlayer(location).ToList();

            // Find doors that entered and exited the range.
            ISet<Door> newInRangeDoors = new HashSet<Door>(nearDoors);
            ISet<Door> newOutOfRangeDoors = new HashSet<Door>(this.doorsNearPlayers);
            newInRangeDoors.ExceptWith(this.doorsNearPlayers);
            newOutOfRangeDoors.ExceptWith(nearDoors);

            // Find doors to toggle that:
            // - Are not both in and out of the range (can be caused by double doors).
            // - Should be open and are not open.
            // - Should be closed and are not closed and aren't near any other players.
            HashSet<Door> doorsToToggle = new HashSet<Door>(newInRangeDoors.Where(door => door.State != State.Open));
            doorsToToggle.SymmetricExceptWith(newOutOfRangeDoors.Where(door => door.State != State.Closed && !this.IsDoorNearAnyPlayer(door, location)));

            this.doorsNearPlayers = nearDoors;

            foreach (Door toggleDoor in doorsToToggle.Where(door => door.Toggle(true, !this.config.SilenceAutomaticDoors)))
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

        /// <summary>Gets the mouse cursor to display if a door is found.</summary>
        /// <param name="locationName">The location to check in.</param>
        /// <param name="playerTile">The position the player is at.</param>
        /// <param name="mouseTile">The position of the mouse.</param>
        /// <param name="cursor">The resulting cursor index, if any.</param>
        /// <param name="transparency">The resulting transparency value, if any.</param>
        /// <returns>Whether a door was found.</returns>
        public bool TryGetMouseCursorForDoor(string locationName, Point playerTile, Point mouseTile, out int cursor, out float transparency)
        {
            cursor = 0;
            transparency = 0;

            if (!this.doors.TryGetValue(locationName, out IDictionary<Point, Door> doorsInLocation) || !DoorManager.TryGetDoorFromMouse(mouseTile, doorsInLocation, out Door _))
                return false;

            cursor = 2;
            transparency = Utils.GetTaxiCabDistance(playerTile, mouseTile) <= this.config.DoorToggleRadius ? 1f : 0.5f;
            return true;
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
            if (door.Toggle(force, true))
            {
                yield return door;
                if (DoorManager.TryGetDoubleDoor(door, doorsInLocation, out Door doubleDoor) && doubleDoor.Toggle(force, true))
                    yield return doubleDoor;
            }
        }

        /// <summary>Gets all doors near the local player.</summary>
        /// <param name="location">The location to look in.</param>
        /// <returns>The doors that were found.</returns>
        private IEnumerable<Door> GetDoorsNearLocalPlayer(GameLocation location)
        {
            return this.GetDoorsNearPosition(location, new Point(Game1.player.getTileX(), Game1.player.getTileY()));
        }

        /// <summary>Gets whether the door is near any player in the given location.</summary>
        /// <param name="door">The door to look for.</param>
        /// <param name="location">The location to look in.</param>
        /// <returns>Whether a player is near the door.</returns>
        private bool IsDoorNearAnyPlayer(Door door, GameLocation location)
        {
            return location.farmers.Select(player => new Point(player.getTileX(), player.getTileY())).Any(position => this.GetDoorsNearPosition(location, position).Contains(door));
        }

        /// <summary>Gets all doors near the given position.</summary>
        /// <param name="location">The location to look in.</param>
        /// <param name="position">The position to search at.</param>
        /// <returns>The doors that were found.</returns>
        private IEnumerable<Door> GetDoorsNearPosition(GameLocation location, Point position)
        {
            if (!this.doors.TryGetValue(Utils.GetLocationName(location), out IDictionary<Point, Door> doorsInLocation))
                yield break;

            for (int i = -1 * this.config.DoorToggleRadius; i <= this.config.DoorToggleRadius; i++)
            {
                // Search along the x axis for horizontal doors and along the y axis for vertical doors (parallel to the hallway direction).

                if (doorsInLocation.TryGetValue(new Point(position.X + i, position.Y), out Door door) && (door.Extras.IsAutomaticDoor || this.config.MakeAllDoorsAutomatic) && door.Orientation == Orientation.Horizontal)
                {
                    yield return door;

                    if (DoorManager.TryGetDoubleDoor(door, doorsInLocation, out Door doubleDoor))
                        yield return doubleDoor;
                }

                if (doorsInLocation.TryGetValue(new Point(position.X, position.Y + i), out door) && (door.Extras.IsAutomaticDoor || this.config.MakeAllDoorsAutomatic) && door.Orientation == Orientation.Vertical)
                {
                    yield return door;

                    if (DoorManager.TryGetDoubleDoor(door, doorsInLocation, out Door doubleDoor))
                        yield return doubleDoor;
                }
            }
        }

        /// <summary>Gets a door from a mouse position, allowing for some fuzzyness.</summary>
        /// <param name="mouseTile">The tile the mouse is on.</param>
        /// <param name="doorsInLocation">The doors in the location to search.</param>
        /// <param name="door">The resulting door, if any.</param>
        /// <returns>Whether a door was found.</returns>
        private static bool TryGetDoorFromMouse(Point mouseTile, IDictionary<Point, Door> doorsInLocation, out Door door)
        {
            door = null;
            for (int y = 0; y < 2; y++)
            {
                if (doorsInLocation.TryGetValue(new Point(mouseTile.X, mouseTile.Y + y), out door))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Gets the associated double door for the given door, if both are double doors.</summary>
        /// <param name="door">The door to use.</param>
        /// <param name="doorsInLocation">The doors in the location.</param>
        /// <param name="doubleDoor">The found double door.</param>
        /// <returns>Whether a double door was found.</returns>
        private static bool TryGetDoubleDoor(Door door, IDictionary<Point, Door> doorsInLocation, out Door doubleDoor)
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