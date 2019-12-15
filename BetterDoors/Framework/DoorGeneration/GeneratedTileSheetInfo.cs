using Microsoft.Xna.Framework;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>Information about a generated tile sheet needed for a door to be drawn by a map.</summary>
    internal class GeneratedTileSheetInfo
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The dimensions of the tile sheet in tiles.</summary>
        public Point TileSheetDimensions { get; }

        /// <summary>The asset key used to load the sheet.</summary>
        public string AssetKey { get; }

        /// <summary>The id of the tile sheet.</summary>
        public string TileSheetId { get; }

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="tileSheetDimensions">The dimensions of the tile sheet in tiles.</param>
        /// <param name="assetKey">The asset key used to load the sheet.</param>
        /// <param name="tileSheetId">The id of the tile sheet.</param>
        public GeneratedTileSheetInfo(Point tileSheetDimensions, string assetKey, string tileSheetId)
        {
            this.TileSheetDimensions = tileSheetDimensions;
            this.AssetKey = assetKey;
            this.TileSheetId = tileSheetId;
        }
    }
}
