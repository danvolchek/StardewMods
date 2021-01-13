namespace StackEverything.Patches.Size
{
    /// <summary>Sets maximum stack size to 999.</summary>
    public class MaximumStackSizePatch
    {
        public static bool Prefix(ref int result)
        {
            result = 999;

            return false;
        }
    }
}
