using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewValley;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    /// Handles side effects of re-adding removed <see cref="StardewValley.TerrainFeatures.TerrainFeature"/>.
    /// </summary>
    internal interface ISideEffectHandler
    {
        void Handle(BaseFeatureSaveData featureSaveData, GameLocation location);

        bool CanHandle(BaseFeatureSaveData featureSaveData);
    }
}