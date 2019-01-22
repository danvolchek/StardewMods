using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace NoFenceDecay
{
    /// <summary>Stops fences from decaying.</summary>
    internal class NoFenceDecayMod : Mod
    {
        /// <summary>Used to find fences.</summary>
        private readonly FenceFinder finder = new FenceFinder();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // repair all fences
            foreach (Fence fence in this.finder.GetFences())
            {
                fence.repair();
                fence.health.Value *= 2.0f;

                fence.maxHealth.Value = fence.health.Value;

                if (fence.isGate.Value)
                    fence.health.Value *= 2.0f;
            }
        }
    }
}