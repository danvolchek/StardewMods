using Harmony;

using SObject = StardewValley.Object;
namespace StackEverything.Patches
{
    /// <summary>
    /// BigCraftables and tackle can now stack up to 999.
    /// </summary>
    [HarmonyPatch(typeof(SObject))]
    [HarmonyPatch("maximumStackSize")]
    public class StackSizePatch
    {
        static bool Prefix(ref int __result)
        {
             __result = 999;

            return false;
        }
    }
}
