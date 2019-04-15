using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace BetterDoors.Framework.Patches
{
    /// <summary>Patches GameLocation.isCollidingPosition to allow for accurate door collision detection.</summary>
    [HarmonyPatch]
    internal class DoorCollisionPatch
    {
        /*********
        ** Private methods
        *********/
        /// <summary>Gets the method to patch.</summary>
        /// <returns>The method to patch.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(GameLocation).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(method => method.Name == nameof(GameLocation.isCollidingPosition) && DoorCollisionPatch.ArrayEquals(method.GetParameters().Select(info => info.ParameterType).ToArray(), new[]
            {
                typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool)
            }));
        }

        /// <summary>The method to call after GameLocation.isCollidingPosition.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="__result">The result of the original method.</param>
        /// <param name="position">The position being checked for collision.</param>
        /// <param name="isFarmer">Whether the farmer is doing the checking.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Postfix(GameLocation __instance, ref bool __result, Rectangle position, bool isFarmer)
        {
            if (!isFarmer || __result)
                return;

            __result = BetterDoorsMod.Instance.IsClosedDoorAt(__instance, position);
        }

        /// <summary>Compares two arrays, returning equality if each element is equal.</summary>
        /// <typeparam name="T">The type of the arrays.</typeparam>
        /// <param name="first">The first array.</param>
        /// <param name="second">The second array.</param>
        /// <returns>Whether the arrays are equal.</returns>
        private static bool ArrayEquals<T>(T[] first, T[] second)
        {
            if (first.Length != second.Length)
                return false;

            for (int i = 0; i < first.Length; i++)
                if (!first[i].Equals(second[i]))
                    return false;
            return true;
        }
    }
}
