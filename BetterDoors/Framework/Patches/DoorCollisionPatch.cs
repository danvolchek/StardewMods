using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Linq;
using System.Reflection;

namespace BetterDoors.Framework.Patches
{
    [HarmonyPatch]
    internal class DoorCollisionPatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(GameLocation).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(method => method.Name == nameof(GameLocation.isCollidingPosition) && ArrayEquals(method.GetParameters().Select(info => info.ParameterType).ToArray(), new[]
            {
                typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool)
            }));
        }

        private static bool ArrayEquals<T>(T[] first, T[] second)
        {
            if (first.Length != second.Length)
                return false;

            for(int i =0; i <first.Length; i++)
                if (!first[i].Equals(second[i]))
                    return false;
            return true;
        }

        private static void Postfix(GameLocation __instance, ref bool __result, Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile, bool ignoreCharacterRequirement)
        {
            if (!isFarmer || __result)
                return;

            __result = BetterDoorsMod.Instance.IsDoorCollisionAt(__instance, position);
        }
    }
}
