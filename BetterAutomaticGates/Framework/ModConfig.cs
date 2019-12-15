namespace BetterAutomaticGates.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/

        /// <summary>How close the player needs to be to a gate before they can toggle it.</summary>
        public int GateToggleRadius { get; set; } = 2;
    }
}
