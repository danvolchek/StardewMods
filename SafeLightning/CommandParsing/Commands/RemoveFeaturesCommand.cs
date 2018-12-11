using StardewValley;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    ///     Removes every terrain feature in the player's farm.
    /// </summary>
    internal class RemoveFeaturesCommand : BaseCommand
    {
        public RemoveFeaturesCommand() : base("remove_features", "Removes all terrain features from your farm.")
        {
        }

        public override string Parse(string[] args)
        {
            Game1.getFarm().terrainFeatures.Clear();
            return "Okay, removed all terrain features from your farm.";
        }
    }
}