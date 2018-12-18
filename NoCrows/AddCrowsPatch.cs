using System.Reflection;
using Harmony;
using StardewValley;

namespace NoCrows
{
    [HarmonyPatch]
    internal class AddCrowsPatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(Farm).GetMethod(nameof(Farm.addCrows));
        }

        private static bool Prefix()
        {
            return false;
        }
    }
}