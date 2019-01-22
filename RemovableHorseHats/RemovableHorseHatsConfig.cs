using StardewModdingAPI;

namespace RemovableHorseHats
{
    internal class RemovableHorseHatsConfig
    {
        public string KeysToRemoveHat { get; set; } = $"{SButton.LeftShift} {SButton.RightShift}";
    }
}