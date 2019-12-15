using StardewValley;
using SObject = StardewValley.Object;

namespace StackEverything.Patches
{
    internal class TryToPlaceItemPatch
    {
        /// <summary>The same as <see cref="Utility.tryToPlaceHere"/> except the furniture code is commented out.</summary>
        /// <remarks> Hopefully this doesn't cause any problems. I have no idea why it was added in 1.3.</remarks>
        public static bool Prefix(GameLocation location, Item item, int x, int y)
        {
            if (item is Tool)
                return false;

            if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player))
            {
                //if (item is Furniture)
                //    Game1.player.ActiveObject = (SObject)null;
                if (((SObject)item).placementAction(location, x, y, Game1.player))
                {
                    Game1.player.reduceActiveItemByOne();
                }
                /*else
                {
                    if (!(item is Furniture))
                        return;
                    Game1.player.ActiveObject = (SObject)(item as Furniture);
                }*/
            }
            else
                Utility.withinRadiusOfPlayer(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 3, Game1.player);

            return false;
        }
    }
}
