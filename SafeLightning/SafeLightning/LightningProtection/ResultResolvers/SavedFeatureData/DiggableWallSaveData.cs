using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData
{
    /// <summary>
    ///     Save the health of the original <see cref="DiggableWall" /> before it is modified.
    /// </summary>
    internal class DiggableWallSaveData : BaseFeatureSaveData
    {
        public int Health;

        public DiggableWallSaveData(Vector2 featurePosition, DiggableWall diggableWall, IReflectionHelper helper) :
            base(featurePosition, diggableWall)
        {
            this.Health = helper.GetField<NetInt>(diggableWall, "health").GetValue().Value;
        }
    }
}