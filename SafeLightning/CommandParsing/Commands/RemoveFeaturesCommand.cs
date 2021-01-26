using StardewModdingAPI;
using StardewValley;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>Removes every terrain feature in the player's farm.</summary>
    internal class RemoveFeaturesCommand : BaseCommand
    {
        /*********
        ** Public methods
        *********/

        /// <summary></summary>
        /// <param name="monitor">The monitor used for command output.</param>
        public RemoveFeaturesCommand(IMonitor monitor) : base(monitor, "remove_features", "Removes all terrain features from your farm.")
        {
        }

        /// <summary>Invoke the command.</summary>
        /// <param name="args">The command arguments</param>
        public override void Invoke(string[] args)
        {
            Game1.getFarm().terrainFeatures.Clear();
            Monitor.Log("Okay, removed all terrain features from your farm.", LogLevel.Info);
        }
    }
}
