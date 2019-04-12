using StardewValley;
using System.Collections.Generic;
using System.Linq;
using BetterDoors.Framework.DoorGeneration;
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

        public void AddTileSheetsToMaps(IDictionary<GameLocation, IList<Door>> doorsByLocation)
        {
            foreach (KeyValuePair<GameLocation, IList<Door>> doorsInLocation in doorsByLocation)
            {
                //Don't add the same tile sheet to a map twice
                HashSet<string> addedSheets = new HashSet<string>();
                //Don't modify the properties of the same tiles twice
                HashSet<GeneratedDoorTileInfo> addedTileInfo  = new HashSet<GeneratedDoorTileInfo>();

                foreach (Door door in doorsInLocation.Value)
                {
                    TileSheet tileSheet;
                    if (addedSheets.Add(door.DoorTileInfo.TileSheetInfo.TileSheetId))
                    {
                        tileSheet = new TileSheet(
                            id: door.DoorTileInfo.TileSheetInfo.TileSheetId,
                            map: doorsInLocation.Key.Map,
                            imageSource: door.DoorTileInfo.TileSheetInfo.AssetKey,
                            sheetSize: new xTile.Dimensions.Size(door.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X, door.DoorTileInfo.TileSheetInfo.TileSheetDimensions.Y),
                            tileSize: new xTile.Dimensions.Size(16, 16)
                        );

                        doorsInLocation.Key.Map.AddTileSheet(tileSheet);
                    }
                    else
                    {
                        tileSheet = doorsInLocation.Key.Map.GetTileSheet(door.DoorTileInfo.TileSheetInfo.TileSheetId);
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

                    if(doorsInLocation.Key.Map.GetLayer("AlwaysFront") == null)
                    {
                        Layer buildingsLayer = doorsInLocation.Key.Map.GetLayer("Buildings");
                        doorsInLocation.Key.Map.AddLayer(new Layer("AlwaysFront", doorsInLocation.Key.Map, buildingsLayer.LayerSize, buildingsLayer.TileSize));
                    }
                }

                this.addedSheetsByLocation[doorsInLocation.Key.Map] = addedSheets;

                doorsInLocation.Key.Map.LoadTileSheets(Game1.mapDisplayDevice);
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
