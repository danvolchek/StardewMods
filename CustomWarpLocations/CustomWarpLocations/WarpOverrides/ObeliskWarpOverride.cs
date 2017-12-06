using System;

namespace CustomWarpLocations.WarpOverrides
{
    class ObeliskWarpOverride : WarpOverride
    {
        string obeliskType;
        internal ObeliskWarpOverride(Object target)
        {
            this.obeliskType = ((StardewValley.Buildings.Building)target).buildingType;
        }

        internal override WarpLocation GetWarpLocation()
        {
            WarpLocation newLocation = null;
            if (this.obeliskType == "Earth Obelisk")
                newLocation = warpLocations.MountainWarpLocation_Obelisk;
            else
                newLocation = warpLocations.BeachWarpLocation_Obelisk;

            return newLocation;
        }
    }
}
