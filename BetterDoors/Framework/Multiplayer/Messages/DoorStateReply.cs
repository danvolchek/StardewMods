using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BetterDoors.Framework.Multiplayer.Messages
{
    internal class DoorStateReply
    {
        public string LocationName { get; }
        public IDictionary<Point, State> DoorStates { get; }

        public DoorStateReply(string locationName, IDictionary<Point, State> doorStates)
        {
            this.LocationName = locationName;
            this.DoorStates = doorStates;
        }
    }
}
