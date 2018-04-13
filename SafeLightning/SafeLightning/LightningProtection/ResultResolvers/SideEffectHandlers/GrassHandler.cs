using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    /// Removes rare items that can drop from <see cref="Grass"/>.
    /// </summary>
    internal class GrassHandler : BaseHandler
    {
        public GrassHandler(IMonitor monitor) : base(monitor)
        {
        }

        public override bool CanHandle(BaseFeatureSaveData featureSaveData)
        {
            return featureSaveData is GrassSaveData grassSaveData && (grassSaveData.feature as Grass).grassType != 1;
        }

        public override void Handle(BaseFeatureSaveData featureSaveData, GameLocation location)
        {
            int chunkType = -1;
            Debris.DebrisType debrisType = Debris.DebrisType.RESOURCE;
            int numChunks = -1;
            string removalMessage = "";

            Random random = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)((double)Game1.uniqueIDForThisGame + (double)featureSaveData.featurePosition.X * 1000.0 + (double)featureSaveData.featurePosition.Y * 11.0 + (double)Game1.mine.mineLevel + (double)Game1.player.timesReachedMineBottom));
            if (random.NextDouble() < 0.005)
            {
                chunkType = 114;
                debrisType = Debris.DebrisType.OBJECT;
                numChunks = 1;
                removalMessage = "ancient seed";
            }
            else if (random.NextDouble() < 0.01)
            {
                chunkType = 382;
                debrisType = Debris.DebrisType.RESOURCE;
                numChunks = random.Next(1, 2);
                removalMessage = "coal";
            }
            else if (random.NextDouble() < 0.02)
            {
                chunkType = 92;
                debrisType = Debris.DebrisType.OBJECT;
                numChunks = random.Next(2, 4);
                removalMessage = "sap";
            }

            if (chunkType != -1)
            {
                //A bug in the game code adds items to Game1.currentLocation instead of the location of the CosmeticPlant.
                location = Game1.currentLocation;
                for (int i = 0; i < location.debris.Count; i++)
                {
                    Debris debris = location.debris[i];

                    if (debris.chunkType == chunkType && debris.debrisType == debrisType && debris.Chunks.Count == numChunks)
                    {
                        if (!WithinRange(featureSaveData.featurePosition, debris.Chunks[0].position / Game1.tileSize, 2))
                            continue;

                        location.debris.RemoveAt(i);
                        i--;

                        monitor.Log($"Removed {removalMessage} for grass.", LogLevel.Trace);

                        break;
                    }
                }
            }
        }
    }
}