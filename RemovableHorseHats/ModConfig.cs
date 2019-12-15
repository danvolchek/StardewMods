using StardewModdingAPI;

namespace RemovableHorseHats
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The keys that allow you to remove the hat from your horse when they're held down.</summary>
        public string KeysToRemoveHat { get; set; } = $"{SButton.LeftShift} {SButton.RightShift}";
    }
}
