using Harmony;
using StackEverything.Patches;
using StackEverything.Patches.Size;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StackEverything
{
    public class StackEverythingMod : Mod
    {
        public static Type[] patchedTypes = new Type[] { GetSDVType("Objects.Furniture"), GetSDVType("Objects.Wallpaper") };

        public override void Entry(IModHelper helper)
        {
            var harmony = HarmonyInstance.Create("cat.stackeverything");

            //This only works if the class' Item.Stack property is not overriden to get {1}, set {}
            //Which means boots, hats, rings, and special items can't be stacked.

            IDictionary<string, Type> replacements = new Dictionary<string, Type>()
            {
                {"maximumStackSize", typeof(MaximumStackSizePatch) },
                {"getStack", typeof(GetStackPatch) },
                {"addToStack", typeof(AddToStackPatch) }
            };

            MethodInfo drawPostfix = typeof(DrawInMenuPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            
            foreach (Type t in patchedTypes.Union(new Type[] { GetSDVType("Object") }))
            {
                MethodInfo drawInMenu = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToList().Find(m => m.Name == "drawInMenu");

                harmony.Patch(drawInMenu, null, new HarmonyMethod(drawPostfix));

                foreach (KeyValuePair<string, Type> replacement in replacements)
                {
                    MethodInfo original = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToList().Find(m => m.Name == replacement.Key);

                    MethodInfo prefix = replacement.Value.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(item => item.Name == "Prefix").FirstOrDefault();
                    MethodInfo postfix = replacement.Value.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(item => item.Name == "Postfix").FirstOrDefault();

                    harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), prefix == null ? null : new HarmonyMethod(postfix));
                }
            }
        }

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        private static Type GetSDVType(string type)
        {
            string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaultSDV != null)
                return defaultSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");
        }
    }
}