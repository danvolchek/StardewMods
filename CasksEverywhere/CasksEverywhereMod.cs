using Harmony;
using StardewModdingAPI;
using System;
using System.Reflection;

namespace CasksEverywhere
{
    public class CasksEverywhereMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            HarmonyInstance instance = HarmonyInstance.Create("cat.caskseverywhere");

            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType($"{prefix}{type}, Stardew Valley");

            return defaultSDV ?? Type.GetType($"{prefix}{type}, StardewValley");
        }
    }
}
