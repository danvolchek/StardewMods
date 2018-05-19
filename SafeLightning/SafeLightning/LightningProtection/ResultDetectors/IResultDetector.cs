using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    ///     Detects when a <see cref="TerrainFeature" /> was affected by a lightning strike.
    /// </summary>
    internal interface IResultDetector : IAvoidsResult
    {
        IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations);
    }
}