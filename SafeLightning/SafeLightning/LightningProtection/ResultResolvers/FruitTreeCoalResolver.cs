using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    ///     Keeps <see cref="FruitTree" />s from turning to coal trees and handles side effects of doing so.
    /// </summary>
    internal class FruitTreeCoalResolver : BaseResolver
    {
        public FruitTreeCoalResolver(IMonitor monitor) : base(monitor)
        {
            this.sideEffectHandlers.Add(new FruitTreeHandler(monitor));
        }

        public override LightningStrikeResult Result => LightningStrikeResult.FruitTreeTurnedToCoal;

        public override void Resolve(GameLocation location, BaseFeatureSaveData featureSaveData)
        {
            FruitTreeSaveData fruitTreeSaveData = featureSaveData as FruitTreeSaveData;

            FruitTree fruitTree = location.terrainFeatures[featureSaveData.FeaturePosition] as FruitTree;

            fruitTree.struckByLightningCountdown.Value = 0;
            fruitTree.fruitsOnTree.Value = fruitTreeSaveData.FruitsOnTree;
            this.monitor.Log(
                $"Turned {fruitTree.GetType()} at position {featureSaveData.FeaturePosition} back to fruit.",
                LogLevel.Trace);

            //Handle the dropped coal
            if (this.sideEffectHandlers[0].CanHandle(fruitTreeSaveData))
                this.sideEffectHandlers[0].Handle(fruitTreeSaveData, location);
        }
    }
}