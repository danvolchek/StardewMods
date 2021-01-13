using System;
using StardewValley;
using StardewValley.Locations;

namespace StackEverything.Patches
{
    /// <summary>Pick up furniture correctly instead of overwriting items in the player's inventory.</summary>
    internal class RemoveQueuedFurniturePatch
    {
        public static bool Prefix(DecoratableLocation instance, Guid guid)
        {
            RemoveQueuedFurniture(instance, guid);
            return false;
        }

        private static void RemoveQueuedFurniture(DecoratableLocation instance, Guid guid)
        {
            var player = Game1.player;
            if (!instance.furniture.ContainsGuid(guid))
                return;
            var furniture = instance.furniture[guid];
            if (!player.couldInventoryAcceptThisItem(furniture))
                return;
            furniture.performRemoveAction(furniture.TileLocation, instance);
            instance.furniture.Remove(guid);

            var result = player.addItemToInventory(furniture);

            if (result != null)
            {
                // Something went very wrong - between the time we checked if the player could accept the item and when we did the placement, the inventory changed.
                // Drop the furniture on the ground so it isn't lost.
                Game1.createItemDebris(result, player.position, player.facingDirection);
            }
            else
            {
                player.CurrentToolIndex = player.getIndexOfInventoryItem(furniture);
            }
            instance.localSound("coin");
        }
    }
}
