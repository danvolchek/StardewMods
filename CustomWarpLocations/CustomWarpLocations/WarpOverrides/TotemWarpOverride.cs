using StardewValley;

namespace CustomWarpLocations.WarpOverrides
{
    internal class TotemWarpOverride : WarpOverride
    {
        private readonly int parentSheetIndex;

        internal TotemWarpOverride(object target)
        {
            this.parentSheetIndex = ((Object) target).ParentSheetIndex;
        }

        internal override WarpLocation GetWarpLocation()
        {
            WarpLocation newLocation = null;
            switch (this.parentSheetIndex)
            {
                case 688:
                    newLocation = WarpLocations.FarmWarpLocation_Totem;
                    break;
                case 689:
                    newLocation = WarpLocations.MountainWarpLocation_Totem;
                    break;
                case 690:
                    newLocation = WarpLocations.BeachWarpLocation_Totem;
                    break;
            }

            return newLocation;
        }
    }
}