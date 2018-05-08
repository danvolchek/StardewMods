using BetterArtisanGoodIcons.Patches.SObjectPatches;
using Harmony;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterArtisanGoodIcons
{
    /// <summary>Draws different icons for different Artisan Good types.</summary>
    /// <remarks>Honey does not save the original item in <see cref="StardewValley.Object.preservedParentSheetIndex"/> so we have to use its name to determine its type, resulting in
    /// honey and non-honey versions of things.</remarks>
    public class BetterArtisanGoodIconsMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            ArtisanGoodsManager.Init(this.Helper, this.Monitor);

            HarmonyInstance harmony = HarmonyInstance.Create("cat.betterartisangoodicons");

            //Don't need to override draw for Object because artisan goods can't be placed down.
            Type objectType = GetSDVType("Object");
            IList<Tuple<string, Type, Type>> replacements = new List<Tuple<string, Type, Type>>()
            {
                {"drawWhenHeld", objectType, typeof(DrawWhenHeldPatch)},
                {"drawInMenu", objectType, typeof(DrawInMenuPatch)},
                {"draw", objectType, typeof(DrawPatch)},
                {"draw", GetSDVType("Objects.Furniture"), typeof(Patches.FurniturePatches.DrawPatch)}
            };

            foreach (Tuple<string, Type, Type> replacement in replacements)
            {
                MethodInfo original = replacement.Item2.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToList().Find(m => m.Name == replacement.Item1);

                MethodInfo prefix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Prefix");
                MethodInfo postfix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Postfix");

                harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
            }
        }

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        /// <summary>Gets the correct type of the object, handling different assembly names for mac/linux users.</summary>
        private static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            return defaultSDV != null ? defaultSDV : Type.GetType(prefix + type + ", StardewValley");
        }
    }
}