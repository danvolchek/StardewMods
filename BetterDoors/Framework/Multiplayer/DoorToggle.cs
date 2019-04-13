using Microsoft.Xna.Framework;

namespace BetterDoors.Framework.Multiplayer
{
    internal class DoorToggle
    {
        public Point Position { get; }
        public string LocationName { get; }

        public DoorToggle(Point position, string locationName)
        {
            this.Position = position;
            this.LocationName = locationName;
        }
    }
}
