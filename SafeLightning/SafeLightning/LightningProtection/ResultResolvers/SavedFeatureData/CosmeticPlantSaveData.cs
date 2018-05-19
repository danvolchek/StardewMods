using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    ///     Create a copy of the <see cref="CosmeticPlant" /> before it is modified.
    /// </summary>
    internal class CosmeticPlantSaveData : BaseFeatureSaveData
    {
        public CosmeticPlantSaveData(Vector2 featurePosition, CosmeticPlant cosmeticPlant)
            : base(featurePosition, new CosmeticPlant(cosmeticPlant.grassType.Value))
        {
        }
    }
}