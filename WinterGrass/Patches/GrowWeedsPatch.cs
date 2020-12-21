using Harmony;
using StardewValley;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace WinterGrass.Patches
{
    /// <summary>Patches the grow weeds method to not grow more grass in winter, if disabled.</summary>
    [HarmonyPatch]
    internal class GrowWeedsPatch
    {
        /*********
        ** Private methods
        *********/

        /// <summary>The method to be patched.</summary>
        /// <returns><see cref="GameLocation.growWeedGrass"/>.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(GameLocation).GetMethod(nameof(GameLocation.growWeedGrass));
        }

        /// <summary>The code to run before the original method.</summary>
        /// <returns>Whether to run the original method or not.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix(GameLocation __instance)
        {
            // Grow grass if the season in the current location is not winter, or it is winter and winter grass growth is enabled
            return Game1.GetSeasonForLocation(__instance) != "winter" || !ModEntry.Instance.Config.DisableWinterGrassGrowth;
        }
    }
}
