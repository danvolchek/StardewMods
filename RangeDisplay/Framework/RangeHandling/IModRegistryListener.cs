using StardewModdingAPI;

namespace RangeDisplay.Framework.RangeHandling
{
    /// <summary>Listens for the mod registry to be ready.</summary>
    internal interface IModRegistryListener
    {
        /*********
        ** Methods
        *********/

        /// <summary>Called when the mod registry is ready.</summary>
        /// <param name="registry">The mod registry.</param>
        void ModRegistryReady(IModRegistry registry);
    }
}
