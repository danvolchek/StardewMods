using System;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BetterHay
{
    class BetterHayGrass
    {
        //Tries to add an item to the player's inventory
        public static bool TryAddItemToInventory(int which)
        {
            string name = Game1.objectInformation[which].Split('/')[0];
            bool addedToInventory = Game1.player.addItemToInventory(new SObject(which, 1)) == null;
            if (addedToInventory)
                Game1.addHUDMessage(new HUDMessage(name, 1, true, Color.LightGoldenrodYellow, (Item)new StardewValley.Object(which, 1, false, -1, 0)));

            return addedToInventory;
        }

        //Drops an item on the given tileLocation
        public static void DropOnGround(Vector2 tileLocation, int which)
        {
            Random random;
            if (Game1.IsMultiplayer)
            {
                random = Game1.recentMultiplayerRandom;
            }
            else
            {
                double uniqueId = Game1.uniqueIDForThisGame;
                double tilePos = tileLocation.X * 1000.0 + tileLocation.Y * 11.0;
                double mineLevel = Game1.mine?.mineLevel ?? 0;
                double timesReachedBottom = Game1.player.timesReachedMineBottom;
                random = new Random((int)(uniqueId + tilePos + mineLevel + timesReachedBottom));
            }

            Game1.createObjectDebris(which, (int)tileLocation.X, (int)tileLocation.Y);
        }
    }


}
