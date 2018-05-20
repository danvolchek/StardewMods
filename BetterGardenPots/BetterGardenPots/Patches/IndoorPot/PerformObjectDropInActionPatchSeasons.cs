using StardewValley;

namespace BetterGardenPots.Patches.IndoorPot
{
    internal class PerformObjectDropInActionPatchSeasons
    {
        private static bool wasOutdoors;

        public static void Prefix()
        {
            wasOutdoors = Game1.currentLocation.IsOutdoors;
            Game1.currentLocation.IsOutdoors = false;
        }

        public static void Postfix()
        {
            Game1.currentLocation.IsOutdoors = wasOutdoors;
        }
    }
}