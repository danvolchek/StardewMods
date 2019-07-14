using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SObject = StardewValley.Object;

namespace BetterFruitTrees.Patches
{
    /// <summary>
    ///     Utility class for patching junimo harvester behavior.
    /// </summary>
    internal class Utils
    {
        internal static IReflectionHelper Reflection;
        internal static bool HarvestThreeAtOnce { get; set; }

        internal static bool CanTreeBeHarvested(FruitTree tree)
        {
            return HarvestThreeAtOnce ? tree.fruitsOnTree.Value == 3 : tree.fruitsOnTree.Value > 0;
        }

        /// <summary>
        ///     Gets an unnocupied adjacent tile for a junimo to stand on, prefering the bottom, then right, then left, then top
        ///     tile.
        /// </summary>
        internal static Point GetUnnocupiedAdjacentLocation(int tileX, int tileY, GameLocation l)
        {
            if (IsPassableAndUnoccupied(l, tileX, tileY + 1))
                return new Point(tileX, tileY + 1);
            if (IsPassableAndUnoccupied(l, tileX + 1, tileY))
                return new Point(tileX + 1, tileY);
            if (IsPassableAndUnoccupied(l, tileX - 1, tileY))
                return new Point(tileX - 1, tileY);
            if (IsPassableAndUnoccupied(l, tileX, tileY - 1))
                return new Point(tileX, tileY - 1);

            return Point.Zero;
        }

        /// <summary>
        ///     Gets whether the given tile is passable and unnocupied, i.e. could a junimo stand on it.
        /// </summary>
        internal static bool IsPassableAndUnoccupied(GameLocation location, int x, int y)
        {
            Rectangle tilePixels =
                new Rectangle(x * Game1.tileSize, y * Game1.tileSize, Game1.tileSize, Game1.tileSize);
            return IsPassable(location, x, y, tilePixels) && !IsOccupied(location, new Vector2(x, y), tilePixels);
        }

        //Thanks to Pathoschild for these functions (adapted slightly)
        //https://github.com/Pathoschild/StardewMods/blob/data-maps/1.3/DataMaps/DataMaps/AccessibilityMap.cs#L137-L197
        /// <summary>
        ///     Is the given tile passable.
        /// </summary>
        private static bool IsPassable(GameLocation location, int x, int y, Rectangle tilePixels)
        {
            // check layer properties
            if (location.isTilePassable(new Location(x, y), Game1.viewport))
                return true;

            // allow bridges
            if (location.doesTileHaveProperty(x, y, "Passable", "Buildings") != null)
            {
                Tile backTile = location.map.GetLayer("Back")
                    .PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
                if (backTile == null ||
                    !backTile.TileIndexProperties.TryGetValue("Passable", out PropertyValue value) || value != "F")
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Is the given tile occupied.
        /// </summary>
        private static bool IsOccupied(GameLocation location, Vector2 tile, Rectangle tilePixels)
        {
            // show open gate as passable
            if (location.objects.TryGetValue(tile, out SObject obj) && obj is Fence fence && fence.isGate.Value &&
                fence.gatePosition.Value == Fence.gateOpenedPosition)
                return false;

            // check for objects, characters, or terrain features
            if (location.isTileOccupiedIgnoreFloors(tile))
                return true;

            // buildings
            if (location is BuildableGameLocation buildableLocation)
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle buildingArea = new Rectangle(building.tileX.Value, building.tileY.Value,
                        building.tilesWide.Value, building.tilesHigh.Value);
                    if (buildingArea.Contains((int)tile.X, (int)tile.Y))
                        return true;
                }

            // large terrain features
            if (location.largeTerrainFeatures?.Any(p => p.getBoundingBox().Intersects(tilePixels)) ?? false)
                return true;


            return false;
        }

        /// <summary>
        ///     Gets the first adjacent FruitTree with fruits on it.
        /// </summary>
        /// <returns>If a fruit tree was found</returns>
        internal static bool GetAdjacentReadyToHarvestFruitTree(Vector2 position, GameLocation location,
            out KeyValuePair<Vector2, FruitTree> result)
        {
            Vector2 treePos = Utility
                .getAdjacentTileLocations(position).FirstOrDefault(pos =>
                    location.terrainFeatures.ContainsKey(pos) && location.terrainFeatures[pos] is FruitTree tree &&
                    CanTreeBeHarvested(tree));
            if (treePos == default(Vector2))
            {
                result = new KeyValuePair<Vector2, FruitTree>(Vector2.Zero, default(FruitTree));
                return false;
            }

            result = new KeyValuePair<Vector2, FruitTree>(treePos, location.terrainFeatures[treePos] as FruitTree);
            return true;
        }

        /// <summary>
        ///     Amount of fruits to harvest at a time.
        /// </summary>
        public static int TreeHarvestAmount()
        {
            return HarvestThreeAtOnce ? 3 : 1;
        }

        /// <summary>
        ///     Harvest fruit from a FruitTree and update the tree accordingly.
        /// </summary>
        internal static SObject GetFruitFromTree(FruitTree tree)
        {
            if (tree.fruitsOnTree.Value == 0)
                return null;

            int num1 = 0;
            if (tree.daysUntilMature.Value <= -112)
                num1 = 1;
            if (tree.daysUntilMature.Value <= -224)
                num1 = 2;
            if (tree.daysUntilMature.Value <= -336)
                num1 = 4;
            if (tree.struckByLightningCountdown.Value > 0)
                num1 = 0;

            int harvestAmount = TreeHarvestAmount();
            tree.fruitsOnTree.Value -= harvestAmount;

            SObject result = new SObject(Vector2.Zero,
                tree.struckByLightningCountdown.Value > 0 ? 382 : tree.indexOfFruit.Value,
                harvestAmount) {Quality = num1};

            return result;
        }

        /// <summary>
        ///     Actually harvest an adjacent fruit tree to the current location.
        /// </summary>
        internal static void TryToActuallyHarvestFruitTree(StardewValley.Characters.JunimoHarvester harvester)
        {
            if (harvester.currentLocation == null)
                return;

            bool found = GetAdjacentReadyToHarvestFruitTree(harvester.getTileLocation(), harvester.currentLocation,
                out KeyValuePair<Vector2, FruitTree> tree);
            if (found)
            {
                //shake the tree without it releasing any fruit
                int fruitsOnTree = tree.Value.fruitsOnTree.Value;
                tree.Value.fruitsOnTree.Value = 0;
                tree.Value.performUseAction(tree.Key, harvester.currentLocation);
                tree.Value.fruitsOnTree.Value = fruitsOnTree;
                SObject result = GetFruitFromTree(tree.Value);
                if (result != null)
                    harvester.tryToAddItemToHut(result);
            }
        }

        /// <summary>
        ///     Whether there is an adjacent FruitTree with fruits on it.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        internal static bool IsAdjacentReadyToHarvestFruitTree(Vector2 position, GameLocation location)
        {
            return GetAdjacentReadyToHarvestFruitTree(position, location, out KeyValuePair<Vector2, FruitTree> p);
        }

        /// <summary>
        ///     Gets the private harvest timer field of <see cref="StardewValley.Characters.JunimoHarvester" />
        /// </summary>
        internal static IReflectedField<int> GetJunimoHarvesterHarvestTimer(StardewValley.Characters.JunimoHarvester harvester)
        {
            return Reflection.GetField<int>(harvester, "harvestTimer");
        }
    }
}