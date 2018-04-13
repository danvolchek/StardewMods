using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    /// Saves all information about a <see cref="TerrainFeature"/> needed for <see cref="IResultResolver"/>s and <see cref="SideEffectHandlers.ISideEffectHandler"/>
    /// to work properly.
    /// </summary>
    internal class BaseFeatureSaveData
    {
        public TerrainFeature feature;
        public Vector2 featurePosition;

        public BaseFeatureSaveData(Vector2 featurePosition, TerrainFeature feature)
        {
            this.featurePosition = featurePosition;
            this.feature = feature;
        }
    }
}