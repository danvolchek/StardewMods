using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>Information about generated tiles needed to draw a door by a map.</summary>
    internal class GeneratedDoorTileInfo
    {
        /*********
        ** Accessors
        *********/

        /// <summary>Info about the generated tile sheet.</summary>
        public GeneratedTileSheetInfo TileSheetInfo { get; }

        /// <summary>Collision info about the door when closed.</summary>
        public Rectangle CollisionInfo { get; }

        /// <summary>The tile index of the start of the animation.</summary>
        public int TopLeftTileIndex { get; }

        /*********
        ** Fields
        *********/

        /// <summary>Whether the first frame is closed or open.</summary>
        private readonly bool isFirstFrameIsClosed;

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="info">Info about the generated tile sheet.</param>
        /// <param name="collisionInfo">Collision info about the door when closed.</param>
        /// <param name="topLeftTileIndex">The tile index of the start of the animation.</param>
        /// <param name="isFirstFrameIsClosed">Whether the first frame is closed or open.</param>
        public GeneratedDoorTileInfo(GeneratedTileSheetInfo info, Rectangle collisionInfo, int topLeftTileIndex, bool isFirstFrameIsClosed)
        {
            this.TileSheetInfo = info;
            this.CollisionInfo = collisionInfo;
            this.TopLeftTileIndex = topLeftTileIndex;
            this.isFirstFrameIsClosed = isFirstFrameIsClosed;
        }

        /// <summary>Gets the tile index of the given state.</summary>
        /// <param name="state">The door state.</param>
        /// <returns>The tile index.</returns>
        public int GetTileIndex(State state)
        {
            return Utils.StateToXIndexOffset(state, this.isFirstFrameIsClosed);
        }
    }
}
