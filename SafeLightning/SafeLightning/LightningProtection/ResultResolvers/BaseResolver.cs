using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using SafeLightning.LightningProtection.ResultResolvers.SideEffectHandlers;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace SafeLightning.LightningProtection.ResultResolvers
{
    /// <summary>
    /// Base class for classes that undo lightning strike results.
    /// </summary>
    internal abstract class BaseResolver : IResultResolver
    {
        protected IMonitor monitor;
        protected IList<ISideEffectHandler> sideEffectHandlers = new List<ISideEffectHandler>();

        public BaseResolver(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public abstract LightningStrikeResult Result { get; }

        public abstract void Resolve(GameLocation location, BaseFeatureSaveData featureSaveData);
    }
}