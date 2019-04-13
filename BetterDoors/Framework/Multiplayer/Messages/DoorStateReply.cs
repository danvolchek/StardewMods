using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BetterDoors.Framework.Multiplayer.Messages
{
    internal class DoorStateReply
    {
        internal string LocationName { get; }
        internal IList<Tuple<Point, string>> DoorStates { get; }

        public DoorStateReply(string locationName, IList<Tuple<Point, string>> doorStates)
        {
            this.LocationName = locationName;
            this.DoorStates = doorStates;
        }
    }
}
