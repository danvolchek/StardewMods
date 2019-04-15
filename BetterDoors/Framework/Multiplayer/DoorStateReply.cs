using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BetterDoors.Framework.Multiplayer
{
    /// <summary>Represents a serializable door state reply.</summary>
    internal class DoorStateReply
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location the states are in.</summary>
        public string LocationName { get; }

        /// <summary>The state of each door in the location.</summary>
        public IDictionary<Point, State> DoorStates { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Constructs an instance.</summary>
        /// <param name="locationName">The location the states are in.</param>
        /// <param name="doorStates">The state of each door in the location.</param>
        public DoorStateReply(string locationName, IDictionary<Point, State> doorStates)
        {
            this.LocationName = locationName;
            this.DoorStates = doorStates;
        }
    }
}
