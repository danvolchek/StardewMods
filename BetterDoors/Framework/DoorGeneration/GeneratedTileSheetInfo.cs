using Microsoft.Xna.Framework;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Information about a generated tile sheet needed to be drawn by a map.
    /// </summary>
    internal class GeneratedTileSheetInfo
    {
        public Point TileSheetDimensions { get; }
        public string AssetKey { get; }
        public string TileSheetId { get; }

        public GeneratedTileSheetInfo(Point tileSheetDimensions, string assetKey, string tileSheetId)
        {
            this.TileSheetDimensions = tileSheetDimensions;
            this.AssetKey = assetKey;
            this.TileSheetId = tileSheetId;
        }
    }
}