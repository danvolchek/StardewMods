using StardewValley.Buildings;

namespace CustomWarpLocations.WarpOverrides
{
    internal class ObeliskWarpOverride : WarpOverride
    {
        private readonly string obeliskType;

        internal ObeliskWarpOverride(object target)
        {
            this.obeliskType = ((Building) target).buildingType.Value;
        }

        internal override WarpLocation GetWarpLocation()
        {
            return this.obeliskType == "Earth Obelisk" ? WarpLocations.MountainWarpLocation_Obelisk : WarpLocations.BeachWarpLocation_Obelisk;
        }
    }
}