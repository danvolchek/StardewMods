using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterDoors.Framework.Mapping
{
    internal class PendingDoor
    {
        public GameLocation Location { get; }
        public Point Position { get; }
        public MapDoorProperty Property { get; }
        private readonly CallbackTimer callbackTimer;

        public PendingDoor(GameLocation location, Point position, MapDoorProperty property, CallbackTimer timer)
        {
            this.Location = location;
            this.Position = position;
            this.Property = property;
            this.callbackTimer = timer;
        }

        public Door ToDoor(GeneratedDoorTileInfo tileInfo)
        {
            return new Door(this.Position, this.Property.Orientation, this.Location.Map, tileInfo, this.callbackTimer);
        }
    }
}
