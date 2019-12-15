using Harmony;
using StardewModdingAPI;
using System.Reflection;

namespace NoCrows
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
            HarmonyInstance harmony = HarmonyInstance.Create(this.Helper.DirectoryPath);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
