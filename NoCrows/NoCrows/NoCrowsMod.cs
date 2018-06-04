using System;
using System.Reflection;
using Harmony;
using StardewModdingAPI;

namespace NoCrows
{
    public class NoCrowsMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            HarmonyInstance harmony = HarmonyInstance.Create("cat.nocrows");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        /// <summary>Gets the correct type of the object, handling different assembly names for mac/linux users.</summary>
        internal static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            return defaultSDV ?? Type.GetType(prefix + type + ", StardewValley");
        }
    }
}