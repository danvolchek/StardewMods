using Harmony;
using StardewValley;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NoCrows
{
    /// <summary>Patches the farm crows method to do nothing.</summary>
    [HarmonyPatch]
    internal class AddCrowsPatch
    {
        /// <summary>Gets the method to patch.</summary>
        /// <returns>The method to patch.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Farm).GetMethod(nameof(Farm.addCrows));
        }

        /// <summary>The method to run before the original method.</summary>
        /// <returns>Whether to run the original method or not.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix()
        {
            return false;
        }
    }
}
