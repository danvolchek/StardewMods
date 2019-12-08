using Harmony;
using StardewModdingAPI;
using System.Reflection;

namespace BetterWorkbenches
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // Do reflection in the entry method to crash when the mod is loaded rather than do nothing when workbenches are clicked (i.e. fail fast to know the mod needs updates)
            WorkbenchPatch.GetTypes();

            HarmonyInstance harmony = HarmonyInstance.Create(helper.ModRegistry.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
