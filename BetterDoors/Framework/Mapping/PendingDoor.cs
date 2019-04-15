using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Mapping.Properties;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using xTile;

namespace BetterDoors.Framework.Mapping
{
    /// <summary>Represents a door parsed from a map but not created yet. See <see cref="DoorCreator"/> for how this is used.</summary>
    internal class PendingDoor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The position the door is at.</summary>
        public Point Position { get; }

        /// <summary>Parsed door info.</summary>
        public MapDoor Property { get; }

        /*********
        ** Fields
        *********/
        /// <summary>The map the door will modify.</summary>
        private readonly Map map;

        /// <summary>Parsed door extras.</summary>
        private readonly MapDoorExtras extras;

        /// <summary>Timer for animations.</summary>
        private readonly CallbackTimer callbackTimer;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="position">The position the door is at.</param>
        /// <param name="property">Parsed door info.</param>
        /// <param name="map">The map the door will modify.</param>
        /// <param name="extras">Parsed door extras.</param>
        /// <param name="callbackTimer">Timer for animations.</param>
        public PendingDoor(Point position, MapDoor property, Map map, MapDoorExtras extras, CallbackTimer callbackTimer)
        {
            this.map = map;
            this.Position = position;
            this.Property = property;
            this.extras = extras;
            this.callbackTimer = callbackTimer;
        }

        /// <summary>Converts this pending door into a real door.</summary>
        /// <param name="tileInfo">Info about how to draw the door.</param>
        /// <returns>The real door.</returns>
        public Door ToDoor(GeneratedDoorTileInfo tileInfo)
        {
            return new Door(this.Position, this.Property.Orientation, this.extras, this.map, tileInfo, this.callbackTimer);
        }
    }
}
