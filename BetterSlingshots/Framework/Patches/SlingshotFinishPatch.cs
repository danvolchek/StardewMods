using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using StardewValley.Tools;

namespace BetterSlingshots.Framework.Patches
{
    /// <summary>Allows for automatic firing by disabling the finish fire event.</summary>
    [HarmonyPatch]
    internal class SlingshotFinishPatch
    {
        private static bool shouldRun = true;
        private static Slingshot instanceToControl;

        public static void ShouldRun(Slingshot instance, bool shouldRun)
        {
            SlingshotFinishPatch.shouldRun = shouldRun;
            SlingshotFinishPatch.instanceToControl = instance;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Slingshot).GetMethod("doFinish", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix(Slingshot __instance)
        {
            if (__instance != SlingshotFinishPatch.instanceToControl || SlingshotFinishPatch.instanceToControl == null)
                return true;

            return SlingshotFinishPatch.shouldRun;
        }
    }
}
