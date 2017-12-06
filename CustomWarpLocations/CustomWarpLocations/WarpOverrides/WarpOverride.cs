using StardewValley;

namespace CustomWarpLocations.WarpOverrides
{
    internal abstract class WarpOverride
    {
        internal enum WarpLocationCategory { Farm, Mountains, Beach };
        internal enum WarpTypeCategory { Totem, Obelisk };

        internal static NewWarpLocations warpLocations;

        abstract internal WarpLocation GetWarpLocation();

        internal void DoWarp()
        {
            WarpLocation location = GetWarpLocation();
            Game1.warpFarmer(location.locationName, location.xCoord, location.yCoord, false);
            AfterWarp();
        }

        void AfterWarp()
        {
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }
    }
}
