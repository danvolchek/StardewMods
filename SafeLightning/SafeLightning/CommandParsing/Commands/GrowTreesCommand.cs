using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    /// Grows all trees and fruit trees in the player's farm.
    /// </summary>
    internal class GrowTreesCommand : BaseCommand
    {
        public GrowTreesCommand() : base("grow_trees", "gt", "Grows all trees and fruit trees in your farm.")
        {
        }

        public override string Parse(string[] args)
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> item in Game1.getFarm().terrainFeatures)
            {
                if (item.Value is Tree t)
                {
                    t.growthStage++;
                }
                else if (item.Value is FruitTree ft)
                {
                    ft.daysUntilMature -= 4;
                    ft.dayUpdate(Game1.getFarm(), item.Key);
                }
            }
            return "Okay, grew trees and fruit trees in your farm.";
        }
    }
}