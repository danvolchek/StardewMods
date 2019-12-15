using BetterDoors.Framework.DoorGeneration;
using StardewValley;
using System.Collections.Generic;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace BetterDoors.Framework.Mapping
{
    /// <summary>Manages map tile sheets and layers so that doors can be drawn on them.</summary>
    internal class MapTileSheetManager
    {
        /*********
        ** Fields
        *********/

        /// <summary>Map of map => tile sheets added to that map.</summary>
        private readonly IDictionary<Map, IEnumerable<string>> addedSheetsByMap = new Dictionary<Map, IEnumerable<string>>();

        /*********
        ** Public methods
        *********/

        /// <summary>Adds tile sheets to a map.</summary>
        /// <param name="map">The map to edit.</param>
        /// <param name="doors">The doors that need their tile sheets added.</param>
        public void AddTileSheetsToMap(Map map, IList<Door> doors)
        {
            //Don't add the same tile sheet to a map twice
            HashSet<string> addedSheets = new HashSet<string>();
            //Don't modify the properties of the same tiles twice
            HashSet<GeneratedDoorTileInfo> addedTileInfo = new HashSet<GeneratedDoorTileInfo>();

            foreach (Door door in doors)
            {
                // Create and add the tile sheet if not yet added or get it if added.
                TileSheet tileSheet;
                if (addedSheets.Add(door.DoorTileInfo.TileSheetInfo.TileSheetId))
                {
                    tileSheet = new TileSheet(
                        id: door.DoorTileInfo.TileSheetInfo.TileSheetId,
                        map: map,
                        imageSource: door.DoorTileInfo.TileSheetInfo.AssetKey,
                        sheetSize: new xTile.Dimensions.Size(door.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X, door.DoorTileInfo.TileSheetInfo.TileSheetDimensions.Y),
                        tileSize: new xTile.Dimensions.Size(16, 16)
                    );

                    map.AddTileSheet(tileSheet);
                }
                else
                {
                    tileSheet = map.GetTileSheet(door.DoorTileInfo.TileSheetInfo.TileSheetId);
                }

                // Make the entire tile sheet passable.
                if (addedTileInfo.Add(door.DoorTileInfo))
                {
                    for (int animationFrame = 0; animationFrame < 4; animationFrame++)
                    {
                        for (int doorYTile = 0; doorYTile < 3; doorYTile++)
                        {
                            tileSheet.TileIndexProperties[door.DoorTileInfo.TopLeftTileIndex + animationFrame + (door.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X) * doorYTile]["Passable"] = "T";
                        }
                    }
                }

                // Add an always front layer if it's missing.
                if (map.GetLayer("AlwaysFront") == null)
                {
                    Layer buildingsLayer = map.GetLayer("Buildings");
                    map.AddLayer(new Layer("AlwaysFront", map, buildingsLayer.LayerSize, buildingsLayer.TileSize));
                }

                this.addedSheetsByMap[map] = addedSheets;
            }

            map.LoadTileSheets(Game1.mapDisplayDevice);
        }

        /// <summary>Reset this manager, removing added tile sheets from every map.</summary>
        public void Reset()
        {
            foreach (KeyValuePair<Map, IEnumerable<string>> sheetsInMap in this.addedSheetsByMap)
            {
                foreach (string layerId in sheetsInMap.Value)
                    sheetsInMap.Key.RemoveTileSheet(sheetsInMap.Key.GetTileSheet(layerId));
            }

            this.addedSheetsByMap.Clear();
        }
    }
}
