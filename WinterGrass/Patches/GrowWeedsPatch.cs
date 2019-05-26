using Harmony;
using StardewValley;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace WinterGrass.Patches
{
    [HarmonyPatch]
    internal class GrowWeedsPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(GameLocation).GetMethod(nameof(GameLocation.growWeedGrass));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix()
        {
            // Don't run the grow grass method if disabling winter grass growth is enabled
            return !Game1.IsWinter || !WinterGrassMod.Instance.Config.DisableWinterGrassGrowth;
        }
    }
}
