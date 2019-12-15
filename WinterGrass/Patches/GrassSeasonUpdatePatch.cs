using Harmony;
using StardewValley.TerrainFeatures;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace WinterGrass.Patches
{
    /// <summary>Patches grass season update to not remove it in winter.</summary>
    [HarmonyPatch]
    internal class GrassSeasonUpdatePatch
    {
        /*********
        ** Private methods
        *********/

        /// <summary>The method to be patched.</summary>
        /// <returns><see cref="Grass.seasonUpdate"/>.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Grass).GetMethod(nameof(Grass.seasonUpdate));
        }

        /// <summary>The code to run before the original method.</summary>
        /// <returns>Whether to run the original method or not.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Argument keywords are defined by Harmony.")]
        private static bool Prefix(Grass __instance, ref bool __result)
        {
            // Set return value to never remove the grass on season update
            __result = false;
            // Load new season sprite
            __instance.loadSprite();
            // Don't run original method
            return false;
        }
    }
}
