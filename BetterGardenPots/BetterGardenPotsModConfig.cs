namespace BetterGardenPots
{
    internal class BetterGardenPotsModConfig
    {
        public bool MakeBeeHousesNoticeFlowersInGardenPots { get; set; } = true;
        public bool MakeSprinklersWaterGardenPots { get; set; } = true;
        public bool HarvestMatureCropsWhenGardenPotBreaks { get; set; } = true;

        public bool AllowPlantingAncientSeedsInGardenPots { get; set; } = false;
        public bool AllowCropsToGrowInAnySeasonOutsideWhenInGardenPot { get; set; } = false;
    }
}
