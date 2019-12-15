using StardewValley;

namespace CustomWarpLocations.WarpOverrides
{
    internal class WandWarpOverride : WarpOverride
    {
        internal override WarpLocation GetWarpLocation()
        {
            if (!Game1.isStartingToGetDarkOut())
                Game1.playMorningSong();
            else
                Game1.changeMusicTrack("none");

            return WarpLocations.FarmWarpLocation_Scepter;
        }
    }
}
