using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    ///     Detects <see cref="FruitTree" /> turned into coal trees.
    /// </summary>
    internal class FruitTreeCoalDetector : IResultDetector
    {
        public LightningStrikeResult Result => LightningStrikeResult.FruitTreeTurnedToCoal;

        public IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations)
        {
            foreach (Vector2 item in strikeLocations)
                if (location.terrainFeatures.ContainsKey(item) &&
                    location.terrainFeatures[item] is FruitTree fruitTree && fruitTree.struckByLightningCountdown.Value == 4)
                    yield return item;
        }
    }
}