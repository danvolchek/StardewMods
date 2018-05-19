using Netcode;
using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    ///     Resets the <see cref="StardewValley.TerrainFeatures.DiggableWall" />s health to what it was before.
    /// </summary>
    internal class DiggableWallHandler : BaseHandler
    {
        private readonly IReflectionHelper reflectionHelper;

        public DiggableWallHandler(IMonitor monitor, IReflectionHelper reflectionHelper) : base(monitor)
        {
            this.reflectionHelper = reflectionHelper;
        }

        public override bool CanHandle(BaseFeatureSaveData featureSaveData)
        {
            return featureSaveData is DiggableWallSaveData;
        }

        public override void Handle(BaseFeatureSaveData featureSaveData, GameLocation location)
        {
            DiggableWallSaveData diggableWallSaveData = featureSaveData as DiggableWallSaveData;
            this.reflectionHelper.GetField<NetInt>(location.terrainFeatures[featureSaveData.FeaturePosition], "health").GetValue().Value = diggableWallSaveData.Health;
        }
    }
}