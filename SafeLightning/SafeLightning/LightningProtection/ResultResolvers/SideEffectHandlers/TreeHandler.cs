using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    /// Removes wood and sapped dropped by the <see cref="Tree"/> when it is hit by lightning.
    /// </summary>
    internal class TreeHandler : BaseHandler
    {
        public TreeHandler(IMonitor monitor) : base(monitor)
        {
        }

        public override bool CanHandle(BaseFeatureSaveData featureSaveData)
        {
            return featureSaveData is TreeSaveData;
        }

        public override void Handle(BaseFeatureSaveData featureSaveData, GameLocation location)
        {
            Tree tree = location.terrainFeatures[featureSaveData.featurePosition] as Tree;
            TreeSaveData treeSaveData = featureSaveData as TreeSaveData;

            //Growth stages 0 - 2 are instantly removed, so there are no side effects

            tree.health = treeSaveData.health;

            if (tree.growthStage >= 3)
            {
                int min = 5;
                int max = 8;
                int total = 30;

                //Trees at growth stage >5 spawn 2 debris with chunkType 92 each with 1 chunk (sap)
                //This must be a stump.
                if (tree.growthStage >= 5)
                {
                    int numToRemove = 2;
                    for (int i = 0; i < location.debris.Count; i++)
                    {
                        Debris debris = location.debris[i];

                        if (debris.chunkType == 92 && debris.debrisType == Debris.DebrisType.RESOURCE && debris.Chunks.Count == 1)
                        {
                            if (!WithinRange(featureSaveData.featurePosition, debris.Chunks[0].position / Game1.tileSize, 2))
                                continue;

                            location.debris.RemoveAt(i);
                            numToRemove--;
                            i--;

                            monitor.Log($"Removing sap for stage 5 stump. {numToRemove} left.", LogLevel.Trace);

                            if (numToRemove == 0)
                                break;
                        }
                    }

                    min = 7;
                    max = 10;
                    total = 40;
                }
                else
                {
                    //Trees at growth stage [3,4] spawn 1 debris with chunkType 388 with 4 chunks, each chunk has type 388 or 389 (extra wood)
                    for (int i = 0; i < location.debris.Count; i++)
                    {
                        Debris debris = location.debris[i];

                        if (debris.chunkType == 388 && debris.debrisType == Debris.DebrisType.RESOURCE)
                        {
                            if (debris.Chunks.Count == 4 && (debris.Chunks[0].debrisType == 388 || debris.Chunks[0].debrisType == 389))
                            {
                                if (!WithinRange(featureSaveData.featurePosition, debris.Chunks[0].position / Game1.tileSize, 2))
                                    continue;

                                monitor.Log($"Removing 4 extra wood for tree stages 3 and 4.", LogLevel.Trace);

                                location.debris.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                //Trees of stage >3 spawn 4 chunks of up to total items, each chunk of size [min,max] with chunkType 12 and the chunks have debrisType 12
                int numRemoved = 0;
                for (int i = 0; i < location.debris.Count; i++)
                {
                    Debris debris = location.debris[i];

                    if (debris.chunkType == 12 && debris.debrisType == Debris.DebrisType.CHUNKS)
                    {
                        if (debris.Chunks.Count >= min && debris.Chunks.Count <= max && debris.Chunks[0].debrisType == 12)
                        {
                            if (!WithinRange(featureSaveData.featurePosition, debris.Chunks[0].position / Game1.tileSize, 3) || numRemoved + debris.Chunks.Count > total)
                                continue;

                            monitor.Log($"Removing wood, count: {debris.Chunks.Count}, total: {numRemoved + debris.Chunks.Count}/{total}.", LogLevel.Trace);
                            numRemoved += debris.Chunks.Count;
                            location.debris.RemoveAt(i);
                            i--;
                            if (numRemoved == total)
                                break;
                        }
                    }
                }
            }
        }
    }
}