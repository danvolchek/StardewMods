using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace BetterFruitTrees.Patches
{
    internal class PlacementPatch
    {
        public static bool Prefix(SObject __instance, ref bool __result, GameLocation location, int x, int y,
            Farmer who)
        {
            if (!__instance.isSapling() || __instance.ParentSheetIndex == 251) return true;
            var placementTile = new Vector2(x / 64f, y / 64f);
            if (location.terrainFeatures.TryGetValue(placementTile, out var feature))
            {
                //Not dirt or the dirt has a crop
                if (!(feature is HoeDirt dirt) || dirt.crop != null) return true;
            }

            var nearbyTree = false;
            var v2 = default(Vector2);
            for (var i = x / 64 - 2; i <= x / 64 + 2; i++)
            {
                for (var k = y / 64 - 2; k <= y / 64 + 2; k++)
                {
                    v2.X = i;
                    v2.Y = k;
                    if (!location.terrainFeatures.ContainsKey(v2) || !(location.terrainFeatures[v2] is Tree) &&
                        !(location.terrainFeatures[v2] is FruitTree)) continue;
                    nearbyTree = true;
                    break;
                }

                if (nearbyTree) break;
            }

            var correctTileProperties = location is Farm ? ((location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable",
                                                                  "Back") != null ||
                                                              location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type",
                                                                  "Back").Equals("Grass")) &&
                                                             !location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y,
                                                                 "NoSpawn", "Back").Equals("Tree")) : location.IsGreenhouse && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable",
                    "Back") != null ||
                location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type",
                    "Back").Equals("Stone"));
            var gameValidLocation = location.IsFarm || location.IsGreenhouse || location is IslandWest;
            if (gameValidLocation && correctTileProperties && !nearbyTree) return true;

            //If not at farm or greenhouse and not allowed to plant outside farm, show an error
            var failedBecauseOutsideFarm = !gameValidLocation &&
                                           !ModEntry.Instance.Config.AllowPlacingFruitTreesOutsideFarm;

            //If at farm or greenhouse and tile properties are wrong and no dangerous planting allowed, show an error
            var failedBecauseDangerousPlant = gameValidLocation && !correctTileProperties &&
                                              !ModEntry.Instance.Config.AllowDangerousPlanting;
            if (failedBecauseOutsideFarm || failedBecauseDangerousPlant)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
                __result = false;
                return false;
            }

            {
                location.playSound("dirtyHit");
                DelayedAction.playSoundAfterDelay("coin", 100);
                if (__instance.ParentSheetIndex == 251)
                {
                    location.terrainFeatures.Add(placementTile, new Bush(placementTile, 3, location));
                    return true;
                }

                var actAsGreenhouse = location.IsGreenhouse ||
                                      (__instance.ParentSheetIndex == 69 || __instance.ParentSheetIndex == 835) &&
                                      location is IslandWest;
                location.terrainFeatures.Add(placementTile,
                    new FruitTree(__instance.ParentSheetIndex)
                    {
                        GreenHouseTree = actAsGreenhouse,
                        GreenHouseTileTree = location.doesTileHavePropertyNoNull((int) placementTile.X,
                            (int) placementTile.Y, "Type", "Back").Equals("Stone")
                    });
                return true;
            }
        }
    }
}