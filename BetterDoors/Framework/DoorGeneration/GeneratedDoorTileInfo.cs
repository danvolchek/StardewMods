namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Information about generated tiles needed to draw a door by a map. 
    /// </summary>
    internal class GeneratedDoorTileInfo
    {
        public GeneratedTileSheetInfo TileSheetInfo { get; }
        public int TopLeftTileIndex { get; }

        public GeneratedDoorTileInfo(GeneratedTileSheetInfo info, int topLeftTileIndex)
        {
            this.TileSheetInfo = info;
            this.TopLeftTileIndex = topLeftTileIndex;
        }
    }
}