using System;

namespace StackEverything.Patches.Size
{
    /// <summary>Sets maximum stack size to 999.</summary>
    public class MaximumStackSizePatch
    {
        public static bool Prefix(ref int result)
        {
            if (result <= 0) throw new ArgumentOutOfRangeException(nameof(result));
            result = 999;

            return false;
        }
    }
}
