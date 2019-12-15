namespace BetterDoors.Framework.Multiplayer
{
    /// <summary>Represents a serializable door state request.</summary>
    internal class DoorStateRequest
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The location to request states for.</summary>
        public string LocationName { get; }

        /*********
        ** Public methods
        *********/

        /// <summary>Constructs an instance.</summary>
        /// <param name="locationName">The location to request states for.</param>
        public DoorStateRequest(string locationName)
        {
            this.LocationName = locationName;
        }
    }
}
