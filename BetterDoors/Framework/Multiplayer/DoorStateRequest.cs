namespace BetterDoors.Framework.Multiplayer
{
    internal class DoorStateRequest
    {
        public string LocationName { get; }

        public DoorStateRequest(string locationName)
        {
            this.LocationName = locationName;
        }
    }
}
