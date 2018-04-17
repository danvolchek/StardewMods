using StardewValley;

namespace StackEverything.Patches.Size
{
    /// <summary>
    /// Rewrite the <see cref="Item.addToStack(int)"/> method.
    /// </summary>
    public class AddToStackPatch
    {
        public static bool Prefix(Item __instance, ref int amount, ref int __result)
        {
            //Handle negative stack amounts from the original game
            if (__instance.Stack == -1 || __instance.Stack == 0)
                __instance.Stack = 1;
            if (amount == -1 || amount == 0)
                amount = 1;

            int maxStack = __instance.maximumStackSize();
            int proposedStack = __instance.Stack + amount;
            if(proposedStack > maxStack)
            {
                __instance.Stack = maxStack;
                __result = proposedStack - maxStack;
            } else
            {
                __instance.Stack = proposedStack;
                __result = 0;
            }

            return false;
        }
    }
}