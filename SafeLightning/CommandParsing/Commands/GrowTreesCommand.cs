using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>Grows all trees and fruit trees in the player's farm.</summary>
    internal class GrowTreesCommand : BaseCommand
    {
        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The monitor used for command output.</param>
        public GrowTreesCommand(IMonitor monitor) : base(monitor, "grow_trees", "gt", "Grows all trees and fruit trees in your farm.")
        {
        }

        /// <summary>Invoke the command.</summary>
        /// <param name="args">The command arguments</param>
        public override void Invoke(string[] args)
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> item in Game1.getFarm().terrainFeatures.Pairs)
            {
                switch (item.Value)
                {
                    case Tree t:
                        t.growthStage.Value++;
                        break;

                    case FruitTree ft:
                        ft.daysUntilMature.Value -= 4;
                        ft.dayUpdate(Game1.getFarm(), item.Key);
                        break;
                }
            }

            this.monitor.Log("Okay, grew trees and fruit trees in your farm.", LogLevel.Info);
        }
    }
}
