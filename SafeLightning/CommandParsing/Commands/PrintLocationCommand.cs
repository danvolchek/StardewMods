using StardewValley;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    ///     Prints the player's location.
    /// </summary>
    internal class PrintLocationCommand : BaseCommand
    {
        public PrintLocationCommand() : base("print_location", "pl", "Prints the player's location.")
        {
        }

        public override string Parse(string[] args)
        {
            return $"Okay, player is at {Game1.player.getTileLocation()}.";
        }
    }
}