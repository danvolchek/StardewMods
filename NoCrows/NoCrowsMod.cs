using System.Reflection;
using Harmony;
using StardewModdingAPI;

namespace NoCrows
{
    public class NoCrowsMod : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            HarmonyInstance harmony = HarmonyInstance.Create("cat.nocrows");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}