namespace BetterDoors.Framework.Serialization
{
    /// <summary>The mod configuration options.</summary>
    internal class BetterDoorsModConfig
    {
        /// <summary>How close the player needs to be to a tile before they can toggle it.</summary>
        public int DoorToggleRadius { get; set; } = 2;

        /// <summary>Whether to make all doors automatic regardless of map settings.</summary>
        public bool MakeAllDoorsAutomatic { get; set; } = false;

        /// <summary>Whether to silence automatically opened/closed doors.</summary>
        public bool SilenceAutomaticDoors { get; set; } = false;
    }
}
