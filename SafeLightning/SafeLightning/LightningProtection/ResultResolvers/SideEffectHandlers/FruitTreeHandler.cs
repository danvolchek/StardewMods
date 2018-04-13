using Microsoft.Xna.Framework;
using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    /// Removes dropped coal from <see cref="StardewValley.TerrainFeatures.FruitTree"/>s.
    /// </summary>
    internal class FruitTreeHandler : BaseHandler
    {
        public FruitTreeHandler(IMonitor monitor) : base(monitor)
        {
        }

        public override bool CanHandle(BaseFeatureSaveData featureSaveData)
        {
            return featureSaveData is FruitTreeSaveData fruitTreeSaveData && fruitTreeSaveData.fruitsOnTree != 0;
        }

        public override void Handle(BaseFeatureSaveData featureSaveData, GameLocation location)
        {
            FruitTreeSaveData fruitTreeSaveData = featureSaveData as FruitTreeSaveData;

            int numRemoved = 0;

            for (int i = 0; i < location.debris.Count; i++)
            {
                Debris debris = location.debris[i];

                if (debris.chunkType == 382 && debris.Chunks.Count == 1 && debris.Chunks[0].debrisType == 382)
                {
                    if (!WithinRange(debris.Chunks[0].position, GetCoalPosition(numRemoved, featureSaveData.featurePosition), Game1.tileSize * 2))
                        continue;

                    monitor.Log($"Removed dropped coal from fruit tree {numRemoved + 1}/{fruitTreeSaveData.fruitsOnTree}.", LogLevel.Trace);
                    location.debris.RemoveAt(i);
                    i--;

                    numRemoved++;
                    if (numRemoved == fruitTreeSaveData.fruitsOnTree)
                        return;
                }
            }
        }

        /// <summary>
        /// The code the game uses to determine initial coal position.
        /// </summary>
        /// <param name="which">Which coal piece to determine</param>
        /// <param name="start">Fruit tree position</param>
        /// <returns>The coal position</returns>
        private static Vector2 GetCoalPosition(int which, Vector2 start)
        {
            Vector2 vector2 = new Vector2(0.0f, 0.0f);
            switch (which)
            {
                case 0:
                    vector2.X = (float)-Game1.tileSize;
                    break;

                case 1:
                    vector2.X = (float)Game1.tileSize;
                    vector2.Y = (float)(-Game1.tileSize / 2);
                    break;

                case 2:
                    vector2.Y = (float)(Game1.tileSize / 2);
                    break;
            }

            return new Vector2(start.X * (float)Game1.tileSize + (float)(Game1.tileSize / 2), (start.Y - 3f) * (float)Game1.tileSize + (float)(Game1.tileSize / 2)) + vector2;
        }
    }
}