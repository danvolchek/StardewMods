using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using StardewValley.TerrainFeatures;

namespace WinterGrass.Patches
{
    [HarmonyPatch]
    internal class GrassSeasonUpdatePatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Grass).GetMethod(nameof(Grass.seasonUpdate));
        }

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
