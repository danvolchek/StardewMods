using Netcode;
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
            Tree currentTree = location.terrainFeatures[featureSaveData.FeaturePosition] as Tree;
            TreeSaveData treeSaveData = featureSaveData as TreeSaveData;

            currentTree.stump.Value = false;
            currentTree.health.Value = treeSaveData.health;
            this.reflectionHelper.GetField<NetBool>(currentTree, "falling").GetValue().Value = false;

            this.monitor.Log($"Stopped {currentTree.GetType()} at position {featureSaveData.Feature} from falling.", LogLevel.Trace);
        }
    }
}