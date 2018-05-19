using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    ///     Saves all information about a <see cref="TerrainFeature" /> needed for <see cref="IResultResolver" />s and
    ///     <see cref="SideEffectHandlers.ISideEffectHandler" />
    ///     to work properly.
    /// </summary>
    internal class BaseFeatureSaveData
    {
        public TerrainFeature Feature;
        public Vector2 FeaturePosition;

        public BaseFeatureSaveData(Vector2 featurePosition, TerrainFeature feature)
        {
            this.FeaturePosition = featurePosition;
            this.Feature = feature;
        }
    }
}