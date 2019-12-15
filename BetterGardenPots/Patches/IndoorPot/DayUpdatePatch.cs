using StardewValley;

namespace BetterGardenPots.Patches.IndoorPot
{
    internal class DayUpdatePatch
    {
        private static bool wasOutdoors;

        public static void Prefix(GameLocation location)
        {
            wasOutdoors = location.IsOutdoors;
            location.IsOutdoors = false;
        }

        public static void Postfix(StardewValley.Objects.IndoorPot __instance, GameLocation location)
        {
            location.IsOutdoors = wasOutdoors;

            if (Game1.isRaining && location.IsOutdoors)
                __instance.hoeDirt.Value.state.Value = 1;
            __instance.showNextIndex.Value = __instance.hoeDirt.Value.state.Value == 1;
        }
    }
}
