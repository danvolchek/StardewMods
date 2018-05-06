using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StackEverything.Patches
{
    /// <summary>Pick up furniture correctly instead of overwriting items in the player's inventory.</summary>
    internal class FurniturePickupPatch
    {
        public static bool Prefix(DecoratableLocation __instance, ref bool __result, int x, int y, Farmer who)
        {
            if (Game1.activeClickableMenu != null)
            {
                __result = false;
                return false;
            }

            for (int index1 = __instance.furniture.Count - 1; index1 >= 0; --index1)
            {
                Furniture current = __instance.furniture[index1];
                if (current.boundingBox.Value.Contains(x, y) && current.clicked(who))
                {
                    if (current.flaggedForPickUp && who.couldInventoryAcceptThisItem(current))
                    {
                        current.flaggedForPickUp.Value = false;
                        current.performRemoveAction(new Vector2(x / Game1.tileSize, y / Game1.tileSize), __instance);

                        Item item = who.addItemToInventory(current);

                        __instance.furniture.RemoveAt(index1);
                        Game1.playSound("coin");
                    }

                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }
}