using Microsoft.Xna.Framework;
using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewValley;
using System;

namespace SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers
{
    /// <summary>
    /// Base class for classes that handle side effects of re-adding removed <see cref="StardewValley.TerrainFeatures.TerrainFeature"/>.
    /// </summary>
    internal abstract class BaseHandler : ISideEffectHandler
    {
        public abstract bool CanHandle(BaseFeatureSaveData featureSaveData);

        public abstract void Handle(BaseFeatureSaveData featureSaveData, GameLocation location);

        protected IMonitor monitor;

        public BaseHandler(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        /// <summary>
        /// Checks whether the two <see cref="Vector2"/>s are within a certain Euclidean distance of each other.
        /// </summary>
        /// <param name="a">The first <see cref="Vector2"/></param>
        /// <param name="b">The second <see cref="Vector2"/></param>
        /// <param name="range">The distance they should be within</param>
        /// <returns>Are <paramref name="a"/> <paramref name="b"/> within <paramref name="range"/></returns>
        protected bool WithinRange(Vector2 a, Vector2 b, double range)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)) <= range;
        }
    }
}