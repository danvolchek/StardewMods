using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using xTile;

namespace BetterDoors.Framework.Mapping
{
    internal class PendingDoor
    {
        public Point Position { get; }
        public MapDoorProperty Property { get; }
        private readonly Map map;
        private readonly MapDoorExtraProperty extras;
        private readonly CallbackTimer callbackTimer;

        public PendingDoor(Map map, Point position, MapDoorProperty property, MapDoorExtraProperty extras, CallbackTimer timer)
        {
            this.map = map;
            this.Position = position;
            this.Property = property;
            this.extras = extras;
            this.callbackTimer = timer;
        }

        public Door ToDoor(GeneratedDoorTileInfo tileInfo)
        {
            return new Door(this.Position, this.Property.Orientation, this.extras, this.map, tileInfo, this.callbackTimer);
        }
    }
}
