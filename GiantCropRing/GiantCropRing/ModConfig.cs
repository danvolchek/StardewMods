namespace GiantCropRing
{
    internal class ModConfig
    {
        public double cropChancePercentWithRing { get; set; } = 0.05;
        public bool shouldWearingBothRingsDoublePercentage { get; set; } = true;
        public double percentOfDayNeededToWearRingToTriggerEffect { get; set; } = 0.5;

        public int cropRingPrice { get; set; } = 5000;
    }
}
