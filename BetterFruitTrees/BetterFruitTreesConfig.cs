namespace BetterFruitTrees
{
    internal class BetterFruitTreesConfig
    {
        public bool DisableFruitTreeJunimoHarvesting { get; set; } = false;

        public bool WaitToHarvestFruitTreesUntilTheyHaveThreeFruitsThenHarvestAllThreeAtOnce
        {
            get;
            set;
        } = false;

        public bool AllowPlacingFruitTreesOutsideFarm { get; set; } = false;
        public bool AllowDangerousPlanting { get; set; } = false;
    }
}
