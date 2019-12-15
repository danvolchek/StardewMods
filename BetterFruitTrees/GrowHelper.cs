using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterFruitTrees
{
    /// <summary>Helps the player put down saplings where they want.</summary>
    internal class GrowHelper
    {
        /// <summary>Construct an instance.</summary>
        /// <param name="events">The available mod events.</param>
        public GrowHelper(IModEvents events)
        {
            events.GameLoop.Saving += this.OnSaving;
        }

        /// <summary>Before a save, check every fruit tree to see if it can grow. If not, make it grow.</summary>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            foreach (GameLocation l in Game1.locations)
                foreach (KeyValuePair<Vector2, TerrainFeature> fruitTree in l.terrainFeatures.Pairs.Where(
                    item => item.Value is FruitTree))
                    if (!this.CanFruitTreeGrow(l, fruitTree.Key))
                        this.SimulateFruitTreeDayUpdate(l, fruitTree.Value as FruitTree);
        }

        /// <summary>Simulates a day of growth on a fruit tree.</summary>
        private void SimulateFruitTreeDayUpdate(GameLocation l, FruitTree tree)
        {
            if (tree.daysUntilMature.Value > 28)
                tree.daysUntilMature.Value = 28;
            tree.daysUntilMature.Value--;
            int oldGrowthStage = tree.growthStage.Value;
            tree.growthStage.Value = tree.daysUntilMature.Value > 0
                ? (tree.daysUntilMature.Value > 7
                    ? (tree.daysUntilMature.Value > 14 ? (tree.daysUntilMature.Value > 21 ? 0 : 1) : 2)
                    : 3)
                : 4;

            //We only want to add a fruit to the tree if our simulated growth caused the tree to fully mature. If it is already mature, the game would have already added a fruit.
            if (oldGrowthStage != 4 && !tree.stump.Value && tree.growthStage.Value == 4 &&
                (tree.struckByLightningCountdown.Value > 0 && !Game1.IsWinter ||
                 Game1.currentSeason.Equals(tree.fruitSeason.Value) || l.Name.ToLower().Contains("greenhouse")))
            {
                tree.fruitsOnTree.Value = Math.Min(3, tree.fruitsOnTree.Value + 1);
                if (l.Name.ToLower().Contains("greenhouse"))
                    tree.GreenHouseTree = true;
            }
        }

        /// <summary>Whether a fruit tree at the given tile and game location could grow.</summary>
        private bool CanFruitTreeGrow(GameLocation l, Vector2 tileLocation)
        {
            bool cannotGrow = false;
            foreach (Vector2 surroundingTileLocations in Utility.getSurroundingTileLocationsArray(tileLocation))
            {
                bool flag2 = l.terrainFeatures.ContainsKey(surroundingTileLocations) &&
                             l.terrainFeatures[surroundingTileLocations] is HoeDirt &&
                             (l.terrainFeatures[surroundingTileLocations] as HoeDirt).crop == null;
                if (l.isTileOccupied(surroundingTileLocations, "") && !flag2)
                {
                    cannotGrow = true;
                    break;
                }
            }

            return !cannotGrow;
        }
    }
}
