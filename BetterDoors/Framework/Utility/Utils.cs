using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;

namespace BetterDoors.Framework.Utility
{
    /// <summary>Holds various utility functions.</summary>
    internal static class Utils
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The number of pixels in one tile dimension.</summary>
        public const int TileSize = 16;

        /*********
        ** Public methods
        *********/

        /// <summary>Checks whether the tile index is inside the bounds of the sheet. Assumes that sheetWidth, sheetHeight % tileSize == 0.</summary>
        /// <param name="sheetWidthPixels">The tile sheet width in pixels.</param>
        /// <param name="sheetHeightPixels">The tile sheet height in pixels.</param>
        /// <param name="tileSize">The number of pixels in one tile dimension.</param>
        /// <param name="tileIndex">The index of the tile.</param>
        /// <param name="error">The reason why the tile is invalid, if any.</param>
        /// <returns>If the tile is valid.</returns>
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

        /// <summary>Logs an error along with a helpful suggestion of what to do next.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        /// <param name="message">The message to log.</param>
        public static void LogContentPackError(IMonitor monitor, string message)
        {
            monitor.Log(message, LogLevel.Error);
            monitor.Log("If you're the mod maker please re-read the instructions and try again. Otherwise, please let the mod maker know.", LogLevel.Error);
        }

        /// <summary>Converts a two dimensional tile position into a tile index.</summary>
        /// <param name="sheetWidthPixels">The tile sheet width in pixels.</param>
        /// <param name="tileSize">The number of pixels in one tile dimension.</param>
        /// <param name="position">The tile position.</param>
        /// <returns>The tile index.</returns>
        public static int ConvertPositionToTileIndex(int sheetWidthPixels, int tileSize, Point position)
        {
            return (position.Y / tileSize) * (sheetWidthPixels / tileSize) + (position.X / tileSize);
        }

        /// <summary>Converts a tile index into a two dimensional tile position.</summary>
        /// <param name="sheetWidthPixels">The tile sheet width in pixels.</param>
        /// <param name="tileSize">The number of pixels in one tile.</param>
        /// <param name="index">The tile index.</param>
        /// <returns>The tile position.</returns>
        public static Point ConvertTileIndexToPosition(int sheetWidthPixels, int tileSize, int index)
        {
            int sheetWidthTiles = sheetWidthPixels / tileSize;

            return new Point((index % sheetWidthTiles) * tileSize, (index / sheetWidthTiles) * tileSize);
        }

        /// <summary>Converts a two dimensional position into a one dimensional position.</summary>
        /// <param name="width">The number of elements in a row.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>The one dimensional position.</returns>
        public static int CollapseDimension(int width, int x, int y)
        {
            return y * width + x;
        }

        /// <summary>Converts a door state to an index in the door's animation.</summary>
        /// <param name="state">The door state.</param>
        /// <param name="isFirstFrameClosed">Whether the first frame has a closed or open door.</param>
        /// <returns>The x tile offset.</returns>
        public static int StateToXIndexOffset(State state, bool isFirstFrameClosed)
        {
            return isFirstFrameClosed ? Utils.StateToXIndexOffsetUnaware(state) : 3 - Utils.StateToXIndexOffsetUnaware(state);
        }

        /// <summary>Gets the most unique name of a location.</summary>
        /// <param name="location">The location to use.</param>
        /// <returns>The name of the location.</returns>
        public static string GetLocationName(GameLocation location)
        {
            return location.uniqueName.Value ?? location.Name;
        }

        /// <summary>Gets the taxi cab distance between two points.</summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The distance between them.</returns>
        public static int GetTaxiCabDistance(Point first, Point second)
        {
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y);
        }

        /// <summary>Gets the tile the player is standing on.</summary>
        /// <returns>The tile the player is standing on.</returns>
        public static Point GetPlayerTile()
        {
            return new Point(Game1.player.getTileX(), Game1.player.getTileY());
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Converts a door state to an index in the door's animation without knowing how the animation is laid out.</summary>
        /// <param name="state">The door state.</param>
        /// <returns>The x tile offset.</returns>
        private static int StateToXIndexOffsetUnaware(State state)
        {
            switch (state)
            {
                case State.Closed:
                    return 0;

                case State.SlightlyOpen:
                    return 1;

                case State.MostlyOpen:
                    return 2;

                default:
                case State.Open:
                    return 3;
            }
        }
    }
}
