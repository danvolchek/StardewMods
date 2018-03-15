using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FruitTreesAnywhere
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            SaveEvents.BeforeSave += this.BeforeSave;
        }

        /***
         * Before a save, check every fruit tree to see if it can grow. If not, make it grow.
         ***/
        private void BeforeSave(object sender, EventArgs e)
        {
            foreach (GameLocation l in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> fruitTree in l.terrainFeatures?.Where(item => item.Value is FruitTree))
                {
                    if (!CanFruitTreeGrow(l, fruitTree.Key))
                    {
                        SimulateFruitTreeDayUpdate(l, fruitTree.Value as FruitTree);
                    }
                }
            }
        }

        /***
         * Simulates a day of growth on a fruit tree.
         ***/
        private void SimulateFruitTreeDayUpdate(GameLocation l, FruitTree tree)
        {
            if (tree.daysUntilMature > 28)
                tree.daysUntilMature = 28;

            tree.daysUntilMature--;
            int oldGrowthStage = tree.growthStage;
            tree.growthStage = tree.daysUntilMature > 0 ? (tree.daysUntilMature > 7 ? (tree.daysUntilMature > 14 ? (tree.daysUntilMature > 21 ? 0 : 1) : 2) : 3) : 4;

            //We only want to add a fruit to the tree if our simulated growth caused the tree to fully mature. If it is already mature, the game would have already added a fruit.
            if (oldGrowthStage != 4 && !tree.stump && tree.growthStage == 4 && (tree.struckByLightningCountdown > 0 && !Game1.IsWinter || (Game1.currentSeason.Equals(tree.fruitSeason) || l.name.ToLower().Contains("greenhouse"))))
            {
                tree.fruitsOnTree = Math.Min(3, tree.fruitsOnTree + 1);
                if (l.name.ToLower().Contains("greenhouse"))
                    tree.greenHouseTree = true;
            }
        }

        /***
         * Returns a bool whether a fruit tree at a location tileLocation in a GameLocation l could grow.
         ***/
        private bool CanFruitTreeGrow(GameLocation l, Vector2 tileLocation)
        {
            bool cannotGrow = false;
            foreach (Vector2 surroundingTileLocations in Utility.getSurroundingTileLocationsArray(tileLocation))
            {
                bool flag2 = l.terrainFeatures.ContainsKey(surroundingTileLocations) && l.terrainFeatures[surroundingTileLocations] is HoeDirt && (l.terrainFeatures[surroundingTileLocations] as HoeDirt).crop == null;
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
