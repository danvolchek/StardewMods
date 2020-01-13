using StardewValley;
using StardewValley.Objects;

namespace StackEverything.Patches
{
    // Dressers can't stack with each other if they're holding items
    internal class CanStackWithPatch
    {
        public static bool Prefix(Item __instance, ref bool __result, ISalable other)
        {
            if ((__instance is StorageFurniture dresser1 && dresser1.heldItems.Count != 0) || (other is StorageFurniture dresser2 && dresser2.heldItems.Count != 0))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
