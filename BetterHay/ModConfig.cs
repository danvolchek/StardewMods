namespace BetterHay
{
    public class ModConfig
    {
        public bool EnableTakingHayFromHoppersAnytime { get; set; } = true;
        public bool EnableGettingHayFromGrassAnytime { get; set; } = true;
        public bool DropHayOnGroundIfNoRoomInInventory { get; set; } = true;
        public double ChanceToDropGrassStarterInsteadOfHay { get; set; } = 0.0;
    }
}
