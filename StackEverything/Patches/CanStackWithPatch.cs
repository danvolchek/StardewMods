using StardewValley;
using StardewValley.Objects;

namespace StackEverything.Patches
{
    // Dressers can't stack with each other if they're holding items
    internal class CanStackWithPatch
    {
        public static bool Prefix(Item instance, ref bool result, ISalable other)
        {
            if ((!(instance is StorageFurniture dresser1) || dresser1.heldItems.Count == 0) &&
                (!(other is StorageFurniture dresser2) || dresser2.heldItems.Count == 0)) return true;
            result = false;
            return false;

        }
    }
}
