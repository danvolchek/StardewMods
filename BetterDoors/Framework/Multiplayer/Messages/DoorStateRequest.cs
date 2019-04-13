namespace BetterDoors.Framework.Multiplayer.Messages
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
