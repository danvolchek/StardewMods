using StardewValley;
using StardewValley.Characters;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;

namespace RemovableHorseHats
{
    /// <summary>Patches <see cref="Horse.checkAction"/> to remove the horse's hat when the remove hat key is down.</summary>
    [HarmonyPatch]
    internal class HorseCheckActionPatch
    {
        /*********
        ** Private methods
        *********/

        /// <summary>Gets the method to patch.</summary>
        /// <returns>The method to patch.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Horse).GetMethod(nameof(Horse.checkAction));
        }

        /// <summary>The code to run before the original method.</summary>
        /// <returns>Whether to run the original method or not.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        public static bool Prefix(Horse __instance, ref bool __result)
        {
            if (__instance.rider != null)
            {
                return true;
            }
            if (__instance.Name.Length <= 0 && Game1.player.horseName.Value == null)
            {
                return true;
            }
            if (!ModEntry.Instance.IsRemoveHatKeyDown)
            {
                return true;
            }

            if (Game1.player.addItemToInventory(__instance.hat.Value) != null)
            {
                Game1.createItemDebris(__instance.hat.Value, __instance.position, __instance.facingDirection);
            }

            __instance.hat.Value = null;
            Game1.playSound("dirtyHit");
            __result = true;
            return false;
        }
    }
}
