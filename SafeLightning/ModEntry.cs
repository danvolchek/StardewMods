using Harmony;
using SafeLightning.CommandParsing;
using StardewModdingAPI;
using System.Reflection;

namespace SafeLightning
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            CommandParser commandParser = new CommandParser(this.Monitor, this.Helper.ConsoleCommands);
            commandParser.RegisterCommands();

            HarmonyInstance instance = HarmonyInstance.Create(this.Helper.ModRegistry.ModID);
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
