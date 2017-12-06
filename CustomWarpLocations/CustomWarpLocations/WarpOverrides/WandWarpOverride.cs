using StardewValley;

namespace CustomWarpLocations.WarpOverrides
{
    class WandWarpOverride : WarpOverride
    {
        internal override WarpLocation GetWarpLocation()
        {
            if (!Game1.isStartingToGetDarkOut())
                Game1.playMorningSong();
            else
                Game1.changeMusicTrack("none");

            return warpLocations.FarmWarpLocation_Scepter;
        }
    }
}
