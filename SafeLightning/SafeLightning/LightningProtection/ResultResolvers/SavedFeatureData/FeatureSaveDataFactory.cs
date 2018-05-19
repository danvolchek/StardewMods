using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    ///     Creates instances of <see cref="BaseFeatureSaveData" /> before it is modified.
    /// </summary>
    internal static class FeatureSaveDataFactory
    {
        internal static BaseFeatureSaveData CreateFeatureSaveData(Vector2 position, TerrainFeature feature,
            IReflectionHelper helper)
        {
            switch (feature)
            {
                case Tree tree:
                    return new TreeSaveData(position, tree);
                case FruitTree fruitTree:
                    return new FruitTreeSaveData(position, fruitTree);
                case CosmeticPlant cosmeticPlant:
                    return new CosmeticPlantSaveData(position, cosmeticPlant);
                case Grass grass:
                    return new GrassSaveData(position, grass);
                case DiggableWall wall:
                    return new DiggableWallSaveData(position, wall, helper);
            }

            return new BaseFeatureSaveData(position, feature);
        }
    }
}