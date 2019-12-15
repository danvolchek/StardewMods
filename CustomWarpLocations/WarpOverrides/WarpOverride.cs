using StardewValley;

namespace CustomWarpLocations.WarpOverrides
{
    internal abstract class WarpOverride
    {
        internal static NewWarpLocations WarpLocations;

        internal abstract WarpLocation GetWarpLocation();

        internal void DoWarp()
        {
            WarpLocation location = this.GetWarpLocation();
            Game1.warpFarmer(location.locationName, location.xCoord, location.yCoord, false);
            this.AfterWarp();
        }

        private void AfterWarp()
        {
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }

        internal enum WarpLocationCategory
        {
            Farm,
            Mountains,
            Beach,
            Desert
        }
    }
}
