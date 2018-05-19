using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning.LightningProtection.ResultDetectors
{
    /// <summary>
    ///     Detects falling <see cref="Tree" />s.
    /// </summary>
    internal class TreeFallingDetector : IResultDetector
    {
        private readonly IReflectionHelper reflectionHelper;

        public TreeFallingDetector(IReflectionHelper reflection)
        {
            this.reflectionHelper = reflection;
        }

        public LightningStrikeResult Result => LightningStrikeResult.TreeFalling;

        public IEnumerable<Vector2> Detect(GameLocation location, IEnumerable<Vector2> strikeLocations)
        {
            foreach (Vector2 item in strikeLocations)
                if (location.terrainFeatures.ContainsKey(item) && location.terrainFeatures[item] is Tree tree)
                    if (this.reflectionHelper.GetField<NetBool>(tree, "falling").GetValue().Value && tree.stump.Value)
                        yield return item;
        }
    }
}