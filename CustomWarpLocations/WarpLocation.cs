namespace CustomWarpLocations
{
    internal class WarpLocation
    {
        public string locationName;
        public int xCoord;
        public int yCoord;

        public WarpLocation(string locationName, int x, int y)
        {
            this.locationName = locationName;
            this.xCoord = x;
            this.yCoord = y;
        }
    }
}
