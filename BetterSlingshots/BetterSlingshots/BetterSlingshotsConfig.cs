namespace BetterSlingshots
{
    internal class BetterSlingshotsConfig
    {
        public bool DisableReverseAiming { get; set; } = true;
        public bool AutoReload { get; set; } = true;
        public string AutomaticSlingshots { get; set; } = "Galaxy, Master";
        public bool ShowActualMousePositionWhenAiming { get; set; } = true;

        public bool CanMoveWhileFiring { get; set; } = false;
        public bool InfiniteAmmo { get; set; } = false;
        public bool RapidFire { get; set; } = false;
        public int GalaxySlingshotPrice { get; set; } = 50000;
    }
}