using StardewValley;

namespace StackEverything.Patches.Size
{
    /// <summary>
    /// Get the correct amount.
    /// </summary>
    public class GetStackPatch
    {
        public static bool Prefix(Item __instance, ref int __result)
        {
            __result = __instance.Stack;

            return false;
        }
    }
}