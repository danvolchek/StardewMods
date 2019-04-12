using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace BetterDoors.Framework.Utility
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Checks whether the tile index is inside the bounds of the sheet. Assumes that sheetWidth, sheetHeight % tileSize == 0.
        /// </summary>
        /// <param name="sheetWidthPixels"></param>
        /// <param name="sheetHeightPixels"></param>
        /// <param name="tileSize"></param>
        /// <param name="tileIndex"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool IsValidTile(int sheetWidthPixels, int sheetHeightPixels, int tileSize, int tileIndex, out string error)
        {
            error = null;
            if (tileIndex < 0)
            {
                error = "Tile index can't be negative";
                return false;
            }

            if (tileIndex >= (sheetWidthPixels / tileSize) * (sheetHeightPixels / tileSize))
            {
                error = "Tile index is larger than the max";
                return false;
            }

            return true;
        }

        public static void LogContentPackError(IMonitor monitor, string message)
        {
            monitor.Log(message, LogLevel.Error);
            monitor.Log("If you're the mod maker please re-read the instructions and try again. Otherwise, please let the mod maker know.", LogLevel.Error);
        }

        public static int ConvertPositionToTileIndex(int sheetWidthPixels, int tileSize, Point position)
        {
            return (position.Y / tileSize) * (sheetWidthPixels / tileSize) + (position.X / tileSize);
        }


        public static Point ConvertTileIndexToPosition(int sheetWidthPixels, int tileSize, int index)
        {
            int sheetWidthTiles = sheetWidthPixels / tileSize;

            return new Point((index % sheetWidthTiles) * tileSize, (index / sheetWidthTiles) * tileSize);
        }

        public static int CollapseDimension(int width, int x, int y)
        {
            return y * width + x;
        }
    }
}