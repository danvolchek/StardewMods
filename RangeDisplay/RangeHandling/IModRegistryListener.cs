using StardewModdingAPI;

namespace RangeDisplay.RangeHandling
{
    internal interface IModRegistryListener
    {
        void ModRegistryReady(IModRegistry registry);
    }
}
