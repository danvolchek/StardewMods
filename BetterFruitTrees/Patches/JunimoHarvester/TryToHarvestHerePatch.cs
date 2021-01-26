namespace BetterFruitTrees.Patches.JunimoHarvester
{
    /// <summary>If a harvestable fruit tree is nearby, start the harvest timer.</summary>
    internal class TryToHarvestHerePatch
    {
        public static void Postfix(StardewValley.Characters.JunimoHarvester instance)
        {
            if (Utils.IsAdjacentReadyToHarvestFruitTree(instance.getTileLocation(), instance.currentLocation))
                Utils.GetJunimoHarvesterHarvestTimer(instance).SetValue(2000);
        }
    }
}
