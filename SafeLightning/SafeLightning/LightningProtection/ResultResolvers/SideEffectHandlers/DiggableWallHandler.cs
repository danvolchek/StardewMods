using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    /// Resets the <see cref="StardewValley.TerrainFeatures.DiggableWall"/>s health to what it was before.
    /// </summary>
    internal class DiggableWallHandler : BaseHandler
    {
        private IReflectionHelper reflectionHelper;

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
            reflectionHelper.GetField<int>(location.terrainFeatures[featureSaveData.featurePosition], "health").SetValue(diggableWallSaveData.health);
        }
    }
}