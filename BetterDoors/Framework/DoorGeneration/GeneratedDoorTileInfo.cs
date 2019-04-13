using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Information about generated tiles needed to draw a door by a map. 
    /// </summary>
    internal class GeneratedDoorTileInfo
    {
        public GeneratedTileSheetInfo TileSheetInfo { get; }
        public Rectangle CollisionInfo { get; }
        public int TopLeftTileIndex { get; }

        private readonly bool isFirstFrameIsClosed;

        public GeneratedDoorTileInfo(GeneratedTileSheetInfo info, Rectangle collisionInfo, int topLeftTileIndex, bool isFirstFrameIsClosed)
        {
            this.TileSheetInfo = info;
            this.CollisionInfo = collisionInfo;
            this.TopLeftTileIndex = topLeftTileIndex;
            this.isFirstFrameIsClosed = isFirstFrameIsClosed;
        }

        public int GetTileIndex(State state)
        {
            return Utils.StateToXIndexOffset(state, this.isFirstFrameIsClosed);
        }
    }
}