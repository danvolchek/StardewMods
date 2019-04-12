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

        public GeneratedDoorTileInfo(GeneratedTileSheetInfo info, Rectangle collisionInfo, int topLeftTileIndex)
        {
            this.TileSheetInfo = info;
            this.CollisionInfo = collisionInfo;
            this.TopLeftTileIndex = topLeftTileIndex;
        }
    }
}