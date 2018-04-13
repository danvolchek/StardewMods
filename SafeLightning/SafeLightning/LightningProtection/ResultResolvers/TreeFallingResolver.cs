using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    /// Stops <see cref="Tree"/>s from falling.
    /// </summary>
    internal class TreeFallingResolver : BaseResolver
    {
        private IReflectionHelper reflectionHelper;

        public TreeFallingResolver(IReflectionHelper reflection, IMonitor monitor) : base(monitor)
        {
            this.reflectionHelper = reflection;
        }

        public override LightningStrikeResult Result => LightningStrikeResult.TreeFalling;

        public override void Resolve(GameLocation location, BaseFeatureSaveData featureSaveData)
        {
            Tree currentTree = location.terrainFeatures[featureSaveData.featurePosition] as Tree;
            TreeSaveData treeSaveData = featureSaveData as TreeSaveData;

            currentTree.stump = false;
            currentTree.health = treeSaveData.health;
            reflectionHelper.GetField<bool>(currentTree, "falling").SetValue(false);

            monitor.Log($"Stopped {currentTree.GetType()} at position {featureSaveData.feature} from falling.", LogLevel.Trace);
        }
    }
}