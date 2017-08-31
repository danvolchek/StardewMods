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
        //Tries to add hay to the player's inventory
        public static bool TryAddHayToInventory(Vector2 tileLocation)
        {
            bool addedToInventory = Game1.player.addItemToInventory(new SObject(178, 1)) == null;
            if (addedToInventory)
                Game1.addHUDMessage(new HUDMessage("Hay", 1, true, Color.LightGoldenrodYellow, (Item)new StardewValley.Object(178, 1, false, -1, 0)));

            return addedToInventory;
        }

        //Drops hay on the given tileLocation
        public static void DropHayOnGround(Vector2 tileLocation)
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

            Game1.createObjectDebris(178, (int)tileLocation.X, (int)tileLocation.Y);
        }
    }


}
