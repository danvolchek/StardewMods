using StardewValley;

namespace StackEverything.Patches.Size
{
    /// <summary>Rewrite the <see cref="Item.addToStack(Item)"/> method.</summary>
    public class AddToStackPatch
    {
        /*********
        ** Public methods
        *********/

        public static bool Prefix(Item __instance, ref Item stack, ref int __result)
        {
            //Handle negative stack amounts from the original game
            if (__instance.Stack == -1 || __instance.Stack == 0)
                __instance.Stack = 1;
            if (stack.Stack == -1 || stack.Stack == 0)
                stack.Stack = 1;

            int maxStack = __instance.maximumStackSize();
            int proposedStack = __instance.Stack + stack.Stack;
            if (proposedStack > maxStack)
            {
                __instance.Stack = maxStack;
                __result = proposedStack - maxStack;
            }
            else
            {
                __instance.Stack = proposedStack;
                __result = 0;
            }

            return false;
        }
    }
}
