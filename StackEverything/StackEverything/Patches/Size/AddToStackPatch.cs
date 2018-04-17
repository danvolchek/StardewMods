using StardewValley;

namespace StackEverything.Patches.Size
{
    /// <summary>
    /// Copy over and modify <see cref="Object.addToStack(int)"/> slightly.
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

            int num1 = __instance.maximumStackSize();
            if (num1 == 1)
            {
                __result = amount;
                return false;
            }
            __instance.Stack = __instance.Stack + amount;
            if (__instance.Stack <= num1)
            {
                __result = 0;
                return false;
            }
            int num2 = __instance.Stack - num1;
            __instance.Stack = num1;
            __result = num2;
            return false;
        }
    }
}