using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace NoFenceDecay
{
    /// <summary>Stops fences from decaying.</summary>
    internal class NoFenceDecayMod : Mod
    {
        /// <summary>Used to find fences.</summary>
        private readonly FenceFinder finder = new FenceFinder();

        public override void Entry(IModHelper helper)
        {
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
        }

        /// <summary>When every day starts, repair all fences.</summary>
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
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