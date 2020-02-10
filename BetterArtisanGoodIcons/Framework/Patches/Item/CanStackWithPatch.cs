using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using StardewValley;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons.Framework.Patches.Item
{
    [HarmonyPatch]
    internal class CanStackWithPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(SObject).GetMethod(nameof(StardewValley.Item.canStackWith));
        }

        private static bool Prefix(StardewValley.Item __instance, ref bool __result, ISalable other)
        {
            if (__instance is SObject obj1 && other is SObject obj2)
            {
                __result = obj1.preservedParentSheetIndex.Value == obj2.preservedParentSheetIndex.Value;
                return false;
            }

            return true;
        }
    }
}
