using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    /// Detects removed <see cref="TerrainFeature"/>s.
    /// </summary>
    internal class RemovedFeatureDetector : IResultDetector
    {
        public LightningStrikeResult Result => LightningStrikeResult.Removed;

        public IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations)
        {
            foreach (Vector2 item in strikeLocations)
            {
                if (!location.terrainFeatures.ContainsKey(item) || location.terrainFeatures[item] == null)
                {
                    yield return item;
                }
            }
        }
    }
}