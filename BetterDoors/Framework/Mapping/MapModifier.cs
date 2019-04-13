using BetterDoors.Framework.DoorGeneration;
using StardewValley;
using System.Collections.Generic;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace BetterDoors.Framework.Mapping
{
    /// <summary>
    /// Modifies map tile sheets and layers so that doors can be drawn on them.
    /// </summary>
    internal class MapModifier : IResetable
    {
        private readonly IDictionary<Map, IEnumerable<string>> addedSheetsByLocation = new Dictionary<Map, IEnumerable<string>>();

        public void AddTileSheetsToLocation(Map map, IList<Door> doors)
        {
            //Don't add the same tile sheet to a map twice
            HashSet<string> addedSheets = new HashSet<string>();
            //Don't modify the properties of the same tiles twice
            HashSet<GeneratedDoorTileInfo> addedTileInfo = new HashSet<GeneratedDoorTileInfo>();

            foreach (Door door in doors)
            {
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

                if (addedTileInfo.Add(door.DoorTileInfo))
                {
                    // Make the entire tile sheet passable
                    for (int animationFrame = 0; animationFrame < 4; animationFrame++)
                    {
                        for (int doorYTile = 0; doorYTile < 3; doorYTile++)
                        {
                            tileSheet.TileIndexProperties[door.DoorTileInfo.TopLeftTileIndex + animationFrame + (door.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X) * doorYTile]["Passable"] = "T";
                        }
                    }
                }

                if(map.GetLayer("AlwaysFront") == null)
                {
                   Layer buildingsLayer = map.GetLayer("Buildings");
                   map.AddLayer(new Layer("AlwaysFront", map, buildingsLayer.LayerSize, buildingsLayer.TileSize));
                }

                this.addedSheetsByLocation[map] = addedSheets;

                map.LoadTileSheets(Game1.mapDisplayDevice);
            }
        }

        public void Reset()
        {
            foreach (KeyValuePair<Map, IEnumerable<string>> sheetsInMap in this.addedSheetsByLocation)
            {
                foreach(string layerId in sheetsInMap.Value)
                    sheetsInMap.Key.RemoveTileSheet(sheetsInMap.Key.GetTileSheet(layerId));
            }

            this.addedSheetsByLocation.Clear();
        }
    }
}
