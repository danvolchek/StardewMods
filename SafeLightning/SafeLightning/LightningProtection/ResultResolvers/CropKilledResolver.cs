using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    ///     Brings <see cref="Crop" />s back to life.
    /// </summary>
    internal class CropKilledResolver : BaseResolver
    {
        public CropKilledResolver(IMonitor monitor) : base(monitor)
        {
        }

        public override LightningStrikeResult Result => LightningStrikeResult.CropKilled;

        public override void Resolve(GameLocation location, BaseFeatureSaveData featureSaveData)
        {
            Crop crop = (location.terrainFeatures[featureSaveData.FeaturePosition] as HoeDirt).crop;
            crop.dead.Value = false;
            this.monitor.Log($"Restored {crop.indexOfHarvest} at position {featureSaveData.FeaturePosition} to life.",
                LogLevel.Trace);
        }
    }
}