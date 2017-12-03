using System;


using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

using System.Collections.Generic;
using System.Linq;
using StardewValley.Locations;

namespace NoFenceDecay
{
    class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            TimeEvents.AfterDayStarted += this.DayStarted;
        }

        private void DayStarted(object sender, EventArgs e)
        {
            List<Fence> fences = getFences();
            if (fences != null)
            {
                foreach (Fence fence in fences)
                {
                    fence.health = fence.maxHealth;
                }
            }
        }

        private List<Fence> getFences()
        {
            if (!Game1.hasLoadedGame || Game1.currentLocation == null )
                return null;

            List<Fence> fences = new List<Fence>();

            foreach (GameLocation loc in Game1.locations)
            {
                fences.AddRange(loc.Objects.Values.OfType<Fence>());

                if(loc is BuildableGameLocation)
                {
                    foreach (GameLocation innerLoc in ((BuildableGameLocation)loc).buildings.Select(item => item.indoors))
                        if(innerLoc != null)
                            fences.AddRange(innerLoc.Objects.Values.OfType<Fence>());
                }
            }

            return fences;
        }
    }
}

