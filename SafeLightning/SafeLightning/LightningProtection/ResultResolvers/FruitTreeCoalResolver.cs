using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    /// Keeps <see cref="FruitTree"/>s from turning to coal trees and handles side effects of doing so.
    /// </summary>
    internal class FruitTreeCoalResolver : BaseResolver
    {
        public FruitTreeCoalResolver(IMonitor monitor) : base(monitor)
        {
            sideEffectHandlers.Add(new FruitTreeHandler(monitor));
        }

        public override LightningStrikeResult Result => LightningStrikeResult.FruitTreeTurnedToCoal;

        public override void Resolve(GameLocation location, BaseFeatureSaveData featureSaveData)
        {
            FruitTreeSaveData fruitTreeSaveData = featureSaveData as FruitTreeSaveData;

            FruitTree fruitTree = location.terrainFeatures[featureSaveData.featurePosition] as FruitTree;

            fruitTree.struckByLightningCountdown = 0;
            fruitTree.fruitsOnTree = fruitTreeSaveData.fruitsOnTree;
            monitor.Log($"Turned {fruitTree.GetType()} at position {featureSaveData.featurePosition} back to fruit.", LogLevel.Trace);

            //Handle the dropped coal
            if (sideEffectHandlers[0].CanHandle(fruitTreeSaveData))
                sideEffectHandlers[0].Handle(fruitTreeSaveData, location);
        }
    }
}