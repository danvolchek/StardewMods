using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    /// Detects falling <see cref="Tree"/>s.
    /// </summary>
    internal class TreeFallingDetector : IResultDetector
    {
        private IReflectionHelper reflectionHelper;

        public TreeFallingDetector(IReflectionHelper reflection)
        {
            this.reflectionHelper = reflection;
        }

        public LightningStrikeResult Result => LightningStrikeResult.TreeFalling;

        public IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations)
        {
            foreach (Vector2 item in strikeLocations)
            {
                if (location.terrainFeatures.ContainsKey(item) && location.terrainFeatures[item] is Tree tree)
                {
                    if (reflectionHelper.GetField<bool>(tree, "falling").GetValue() && tree.stump)
                        yield return item;
                }
            }
        }
    }
}