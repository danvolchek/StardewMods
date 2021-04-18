using Harmony;
using StardewValley.Objects;
using System.Reflection;

namespace CasksEverywhere
{
    [HarmonyPatch]
    internal class CaskPatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(Cask).GetMethod(nameof(Cask.IsValidCaskLocation));
        }

        private static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
