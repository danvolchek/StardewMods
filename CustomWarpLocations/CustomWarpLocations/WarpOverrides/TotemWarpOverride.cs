using System;

namespace CustomWarpLocations.WarpOverrides
{
    class TotemWarpOverride : WarpOverride
    {
        int parentSheetIndex;
        internal TotemWarpOverride(Object target)
        {
            this.parentSheetIndex = ((StardewValley.Object)target).parentSheetIndex;
        }

        internal override WarpLocation GetWarpLocation()
        {
            WarpLocation newLocation = null;
            switch (this.parentSheetIndex)
            {
                case 688:
                    newLocation = warpLocations.FarmWarpLocation_Totem;
                    break;
                case 689:
                    newLocation = warpLocations.MountainWarpLocation_Totem;
                    break;
                case 690:
                    newLocation = warpLocations.BeachWarpLocation_Totem;
                    break;
            }
            return newLocation;
        }
    }
}
