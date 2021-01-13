using StardewValley;

namespace StackEverything.Patches.Size
{
    /// <summary>Rewrite the <see cref="Item.addToStack(Item)"/> method.</summary>
    public class AddToStackPatch
    {
        /*********
        ** Public methods
        *********/

        public static bool Prefix(Item instance, ref Item stack, ref int result)
        {
            //Handle negative stack amounts from the original game
            if (instance.Stack == -1 || instance.Stack == 0)
                instance.Stack = 1;
            if (stack.Stack == -1 || stack.Stack == 0)
                stack.Stack = 1;

            var maxStack = instance.maximumStackSize();
            var proposedStack = instance.Stack + stack.Stack;
            if (proposedStack > maxStack)
            {
                instance.Stack = maxStack;
                result = proposedStack - maxStack;
            }
            else
            {
                instance.Stack = proposedStack;
                result = 0;
            }

            return false;
        }
    }
}
