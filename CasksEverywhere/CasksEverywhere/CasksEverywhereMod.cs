using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using StardewModdingAPI;

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
