using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System;

namespace WindEffects.Framework.Shakers
{
    internal class HoeDirtShaker : IShaker
    {
        private readonly HoeDirt dirt;
        private readonly bool left;
        public HoeDirtShaker(HoeDirt dirt, bool left)
        {
            this.dirt = dirt;
            this.left = left;

        }
        public void Shake(IReflectionHelper helper, Vector2 tile)
        {
            // not outdoors
            if (!Game1.player.currentLocation.IsOutdoors)
                return;
              
            if (dirt.crop == null)
                return;

            helper.GetMethod(this.dirt, "shake").Invoke((float)(0.392699092626572 / 2), (float)(Math.PI / 80f), this.left);
        }
    }
}
