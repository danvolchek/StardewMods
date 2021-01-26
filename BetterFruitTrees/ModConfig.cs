namespace BetterFruitTrees
{
    internal class ModConfig
    {
        public bool DisableFruitTreeJunimoHarvesting { get; set; } = false;
        public bool WaitToHarvestFruitTreesUntilTheyHaveThreeFruitsThenHarvestAllThreeAtOnce { get; set; } = false;
        public bool AllowPlacingFruitTreesOutsideFarm { get; set; } = false;
        public bool AllowDangerousPlanting { get; set; } = false;
    }
}