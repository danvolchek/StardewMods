using StardewValley.Tools;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;

namespace BetterSlingshots.Framework.Patching.Patches
{
    [HarmonyPatch]
    internal class SlingshotBeginUsingPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Slingshot).GetMethod(nameof(Slingshot.beginUsing));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Postfix(Slingshot __instance)
        {
            PatchManager.Instance.AfterUsingHook(__instance);
        }
    }
}
