using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    ///     Grows all trees and fruit trees in the player's farm.
    /// </summary>
    internal class GrowTreesCommand : BaseCommand
    {
        public GrowTreesCommand() : base("grow_trees", "gt", "Grows all trees and fruit trees in your farm.")
        {
        }

        public override string Parse(string[] args)
        {
            foreach (KeyValuePair<Vector2, NetRef<TerrainFeature>> item in Game1.getFarm().terrainFeatures.FieldPairs)
                switch (item.Value.Value)
                {
                    case Tree t:
                        t.growthStage.Value++;
                        break;
                    case FruitTree ft:
                        ft.daysUntilMature.Value -= 4;
                        ft.dayUpdate(Game1.getFarm(), item.Key);
                        break;
                }

            return "Okay, grew trees and fruit trees in your farm.";
        }
    }
}