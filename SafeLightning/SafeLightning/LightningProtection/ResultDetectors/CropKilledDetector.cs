using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    ///     Detects <see cref="Crop" />s killed by lightning.
    /// </summary>
    internal class CropKilledDetector : IResultDetector
    {
        public LightningStrikeResult Result => LightningStrikeResult.CropKilled;

        public IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations)
        {
            foreach (Vector2 item in strikeLocations)
                if (location.terrainFeatures.ContainsKey(item) && location.terrainFeatures[item] is HoeDirt dirt &&
                    dirt.crop != null && dirt.crop.dead.Value)
                    if (location.Name.Equals("Greenhouse") || dirt.crop.seasonsToGrowIn.Contains(Game1.currentSeason))
                        yield return item;
        }
    }
}