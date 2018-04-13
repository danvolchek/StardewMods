using StardewValley;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    /// Sets the weather for today and tomorrow as lightning.
    /// </summary>
    internal class SetLightningCommand : BaseCommand
    {
        public SetLightningCommand() : base("set_lightning", "sl", "Sets today and tomorrow's weather to lightning.")
        {
        }

        public override string Parse(string[] args)
        {
            Game1.weatherForTomorrow = Game1.weather_lightning;
            Game1.isLightning = true;

            return "Okay, set weather for today and tomorrow as lightning.";
        }
    }
}