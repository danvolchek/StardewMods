using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    ///     Removes the flooring <see cref="Item" /> that is dropped when the <see cref="Flooring" /> is hit by lightning.
    /// </summary>
    internal class FlooringHandler : BaseHandler
    {
        public FlooringHandler(IMonitor monitor) : base(monitor)
        {
        }

        public override bool CanHandle(BaseFeatureSaveData featureSaveData)
        {
            return featureSaveData.Feature is Flooring;
        }

        public override void Handle(BaseFeatureSaveData featureSaveData, GameLocation location)
        {
            Flooring f = location.terrainFeatures[featureSaveData.FeaturePosition] as Flooring;

            for (int i = 0; i < location.debris.Count; i++)
            {
                Debris debris = location.debris[i];

                if (debris.item != null && debris.item.ParentSheetIndex == GetParentSheetIndexForFlooring(f.whichFloor.Value))
                {
                    if (!this.WithinRange(debris.Chunks[0].position.Value / Game1.tileSize, featureSaveData.FeaturePosition,
                        2))
                        continue;
                    this.monitor.Log($"Removed flooring with parentSheetIndex {debris.item.ParentSheetIndex}.",
                        LogLevel.Trace);
                    location.debris.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        ///     Same code the game uses to determine the parentSheetIndex of the floor.
        /// </summary>
        /// <param name="whichFloor">Which floor is it</param>
        /// <returns>The parentSheetIndex of that floor</returns>
        private static int GetParentSheetIndexForFlooring(int whichFloor)
        {
            int parentSheetIndex = -1;
            switch (whichFloor)
            {
                case 0:
                    parentSheetIndex = 328;
                    break;

                case 1:
                    parentSheetIndex = 329;
                    break;

                case 2:
                    parentSheetIndex = 331;
                    break;

                case 3:
                    parentSheetIndex = 333;
                    break;

                case 4:
                    parentSheetIndex = 401;
                    break;

                case 5:
                    parentSheetIndex = 407;
                    break;

                case 6:
                    parentSheetIndex = 405;
                    break;

                case 7:
                    parentSheetIndex = 409;
                    break;

                case 8:
                    parentSheetIndex = 411;
                    break;

                case 9:
                    parentSheetIndex = 415;
                    break;
            }

            return parentSheetIndex;
        }
    }
}