using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    /// Create a copy of the <see cref="Grass"/> before it is modified.
    /// </summary>
    internal class GrassSaveData : BaseFeatureSaveData
    {
        public GrassSaveData(Vector2 featurePosition, Grass grass)
            : base(featurePosition, new Grass(grass.grassType, grass.numberOfWeeds))
        {
        }
    }
}