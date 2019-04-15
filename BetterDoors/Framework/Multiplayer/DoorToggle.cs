using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;

namespace BetterDoors.Framework.Multiplayer
{
    /// <summary>Represents a serializable door toggle message.</summary>
    internal class DoorToggle
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The position to look for a door at.</summary>
        public Point Position { get; }

        /// <summary>The state before the door was toggled.</summary>
        public State StateBeforeToggle { get; }

        /// <summary>The location to look for a door in.</summary>
        public string LocationName { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Constructs an instance.</summary>
        /// <param name="position">The position to look for a door at.</param>
        /// <param name="stateBeforeToggle">The state before the door was toggled.</param>
        /// <param name="locationName">The location to look for a door in.</param>
        public DoorToggle(Point position, State stateBeforeToggle, string locationName)
        {
            this.Position = position;
            this.StateBeforeToggle = stateBeforeToggle;
            this.LocationName = locationName;
        }
    }
}
