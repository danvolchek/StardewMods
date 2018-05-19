using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    ///     Save the health of the <see cref="Tree" /> before it is modified.
    /// </summary>
    internal class TreeSaveData : BaseFeatureSaveData
    {
        public float health;

        public TreeSaveData(Vector2 featurePosition, Tree tree) : base(featurePosition, tree)
        {
            this.health = tree.health.Value;
        }
    }
}