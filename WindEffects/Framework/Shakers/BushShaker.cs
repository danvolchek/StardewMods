﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System;
using StardewValley;

namespace WindEffects.Framework.Shakers
{
    internal class BushShaker : IShaker
    {
        private readonly Bush bush;
        private readonly bool left;

        public BushShaker(Bush bush, bool left)
        {
            this.bush = bush;
            this.left = left;
        }

        public void Shake(IReflectionHelper helper, Vector2 tile)
        {
            // can't just call shake because it drops items. We don't want to drop items.
            // see Bush::shake for the logic this replicates

            // deactivated via config
	    if (!ModEntry.config.ShakeBushes)
		return;
            
            // not outdoors
            if (!Game1.player.currentLocation.IsOutdoors)
                return;

            // already shaking
            if (helper.GetField<float>(this.bush, "maxShake").GetValue() != 0)
                return;

            helper.GetField<bool>(this.bush, "shakeLeft").SetValue(this.left);
            helper.GetField<float>(this.bush, "maxShake").SetValue((float)Math.PI / 128f);
        }
    }
}
