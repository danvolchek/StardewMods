using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    /// Detects <see cref="FruitTree"/> turned into coal trees.
    /// </summary>
    internal class FruitTreeCoalDetector : IResultDetector
    {
        public LightningStrikeResult Result => LightningStrikeResult.FruitTreeTurnedToCoal;

        public IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations)
        {
            foreach (Vector2 item in strikeLocations)
            {
                if (location.terrainFeatures.ContainsKey(item) && location.terrainFeatures[item] is FruitTree fruitTree && fruitTree.struckByLightningCountdown == 4)
                {
                    yield return item;
                }
            }
        }
    }
}