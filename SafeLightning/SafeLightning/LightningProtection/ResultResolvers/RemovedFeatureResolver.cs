using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers;
using StardewModdingAPI;
using StardewValley;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    ///     Re-adds removed <see cref="StardewValley.TerrainFeatures.TerrainFeature" /> and handles any unintended side effects
    ///     of doing so.
    /// </summary>
    internal class RemovedFeatureResolver : BaseResolver
    {
        public RemovedFeatureResolver(IMonitor monitor) : base(monitor)
        {
            this.sideEffectHandlers.Add(new FlooringHandler(this.monitor));
            this.sideEffectHandlers.Add(new TreeHandler(this.monitor));
            this.sideEffectHandlers.Add(new CosmeticPlantHandler(this.monitor));
            this.sideEffectHandlers.Add(new GrassHandler(this.monitor));
        }

        public override LightningStrikeResult Result => LightningStrikeResult.Removed;

        public override void Resolve(GameLocation location, BaseFeatureSaveData featureSaveData)
        {
            location.terrainFeatures[featureSaveData.FeaturePosition] = featureSaveData.Feature;
            this.monitor.Log(
                $"Re-added {featureSaveData.Feature.GetType()} at position {featureSaveData.FeaturePosition}.",
                LogLevel.Trace);

            //Handle duplicate items or other problems caused by re-adding the terrain feature
            foreach (ISideEffectHandler handler in this.sideEffectHandlers)
                if (handler.CanHandle(featureSaveData))
                {
                    handler.Handle(featureSaveData, location);
                    break;
                }
        }
    }
}