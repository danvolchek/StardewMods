namespace StackEverything.Patches.Size
{
    /// <summary>
    /// Sets maximum stack size to 999.
    /// </summary>
    public class MaximumStackSizePatch
    {
        public static bool Prefix(ref int __result)
        {
            __result = 999;

            return false;
        }
    }
}