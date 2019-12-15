using Harmony;
using StardewModdingAPI;
using System.Reflection;

namespace CasksEverywhere
{
    public class CasksEverywhereMod : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            HarmonyInstance instance = HarmonyInstance.Create("cat.caskseverywhere");

            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
