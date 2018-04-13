using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewValley;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    /// Undoes a lightning strike result.
    /// </summary>
    internal interface IResultResolver : IAvoidsResult
    {
        void Resolve(GameLocation location, BaseFeatureSaveData savedFeatureData);
    }
}