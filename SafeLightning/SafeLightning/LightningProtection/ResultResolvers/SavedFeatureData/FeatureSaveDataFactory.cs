using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    /// Creates instances of <see cref="BaseFeatureSaveData"/> before it is modified.
    /// </summary>
    internal static class FeatureSaveDataFactory
    {
        internal static BaseFeatureSaveData CreateFeatureSaveData(Vector2 position, TerrainFeature feature, IReflectionHelper helper)
        {
            if (feature is Tree tree)
                return new TreeSaveData(position, tree);
            else if (feature is FruitTree fruitTree)
                return new FruitTreeSaveData(position, fruitTree);
            else if (feature is CosmeticPlant cosmeticPlant)
                return new CosmeticPlantSaveData(position, cosmeticPlant);
            else if (feature is Grass grass)
                return new GrassSaveData(position, grass);
            else if (feature is DiggableWall wall)
                return new DiggableWallSaveData(position, wall, helper);
            else return new BaseFeatureSaveData(position, feature);
        }
    }
}