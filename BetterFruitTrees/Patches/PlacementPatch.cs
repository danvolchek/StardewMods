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
            //Not a sapling
            if (!__instance.isSapling() || __instance.ParentSheetIndex == 251) // 251: tea sapling
                return true;

            int tileX = x / 64;
            int tileY = y / 64;
            Vector2 index1 = new Vector2(tileX, tileY);

            //The original code has a check for this, but execution never actually reaches here because saplings aren't allowed to be placed on dirt
            //Terrain feature at the position
            if (location.terrainFeatures.TryGetValue(index1, out TerrainFeature feature))
            {
                //Not dirt or the dirt has a crop
                if (!(feature is HoeDirt dirt) || dirt.crop != null)
                    return true;
            }

            bool nearbyTree = false;

            for (int index2 = tileX - 2; index2 <= tileX + 2; ++index2)
            {
                for (int index3 = tileY - 2; index3 <= tileY + 2; ++index3)
                {
                    Vector2 key = new Vector2(index2, index3);
                    if (location.terrainFeatures.ContainsKey(key) &&
                        (location.terrainFeatures[key] is Tree || location.terrainFeatures[key] is FruitTree))
                    {
                        nearbyTree = true;
                        break;
                    }
                }

                if (nearbyTree)
                    break;
            }

            bool correctTileProperties = IsFarm(location) ? ((location.doesTileHaveProperty((int)index1.X, (int)index1.Y, "Diggable",
                                              "Back") != null ||
                                          location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "Type",
                                              "Back").Equals("Grass")) &&
                                         !location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y,
                                             "NoSpawn", "Back").Equals("Tree")) : (location.IsGreenhouse && (location.doesTileHaveProperty((int)index1.X, (int)index1.Y, "Diggable",
                                                                                                                "Back") != null ||
                                                                                                            location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "Type",
                                                                                                                "Back").Equals("Stone")));

            bool gameValidLocation = IsFarm(location) || location.IsGreenhouse;

            //If the game would return true, let it run
            if (gameValidLocation && correctTileProperties && !nearbyTree)
                return true;

            //If not at farm or greenhouse and not allowed to plant outside farm, show an error
            bool failedBecauseOutsideFarm = !gameValidLocation &&
                                            !BetterFruitTreesMod.Instance.Config.Allow_Placing_Fruit_Trees_Outside_Farm;

            //If at farm or greenhouse and tile properties are wrong and no dangerous planting allowed, show an error
            bool failedBecauseDangerousPlant = gameValidLocation && !correctTileProperties &&
                                               !BetterFruitTreesMod.Instance.Config.Allow_Dangerous_Planting;

            if (failedBecauseOutsideFarm || failedBecauseDangerousPlant)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
                __result = false;
                return false;
            }

            //Place sapling
            location.playSound("dirtyHit");
            DelayedAction.playSoundAfterDelay("coin", 100);

            //If the game was going to place a tree, it removed anything at the tree index, so we do the same
            bool actAsGreenhouse = location.IsGreenhouse || ((__instance.ParentSheetIndex == 69 || __instance.ParentSheetIndex == 835) && location is IslandWest); // 69: banana sapling, 835: mango sapling
            location.terrainFeatures.Remove(index1);
            location.terrainFeatures.Add(index1, new FruitTree(__instance.ParentSheetIndex)
            {
                GreenHouseTree = actAsGreenhouse,
                GreenHouseTileTree = location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "Type", "Back")
                    .Equals("Stone")
            });

            __result = true;
            return false;
        }

        private static bool IsFarm(GameLocation location)
        {
            return location is Farm || location is IslandWest;
        }
    }
}
