using System;

namespace CustomWarpLocations
{
    public class WarpLocation
    {
        public String locationName;
        public int xCoord;
        public int yCoord;

        public WarpLocation(String locationName, int x, int y)
        {
            this.locationName = locationName;
            this.xCoord = x;
            this.yCoord = y;
        }
    }
}
