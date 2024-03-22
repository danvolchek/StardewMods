using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace BetterHay
{
    internal class BetterHayGrass
    {
        //Tries to add an item to the player's inventory
        public static bool TryAddItemToInventory(int which)
        {
            var whichAsString = which.ToString();
            //string name = Game1.objectInformation[which].Split('/')[0];
            string name = Game1.objectData[whichAsString].Name;
            //bool addedToInventory = Game1.player.addItemToInventory(new SObject(which, 1)) == null;
            bool addedToInventory = Game1.player.addItemToInventory(new SObject(whichAsString, 1)) == null;
            if (addedToInventory)
                //Game1.addHUDMessage(new HUDMessage(name, 1, true, Color.LightGoldenrodYellow, (Item)new StardewValley.Object(which, 1, false, -1, 0)));
                Game1.addHUDMessage(new HUDMessage(name, 1, true));

            return addedToInventory;
        }

        //Drops an item on the given tileLocation
        public static void DropOnGround(Vector2 tileLocation, int which)
        {
            //Game1.createObjectDebris(which, (int)tileLocation.X, (int)tileLocation.Y);
            Game1.createObjectDebris(which.ToString(), (int)tileLocation.X, (int)tileLocation.Y);
        }
    }
}
