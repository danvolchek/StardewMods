using FloweryTools.ParticleCreator;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    // Used by watering can
    internal class Watering : BaseCreator, IToolFlowerer
    {
        public Watering(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {

            if (frameIndex == 2)
            {
                int times = this.helper.rand.Next(1, 4);
                for (int i = 0; i < times; i++)
                {
                    this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(1), 10), this.helper.ApplyJitter(this.helper.GetExplosionMotion(), 0.1f), location);
                }
            }
        }

        public bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames)
        {
            //FarmerSprite: cases 180, 172, and 164 (188 is identical to 172 but flipped)
            return this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(62, 0), new Tuple<int, int>(62, 75), new Tuple<int, int>(63, 100), new Tuple<int, int>(46, 500) }) ||
                this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(58, 0), new Tuple<int, int>(58, 75), new Tuple<int, int>(59, 100), new Tuple<int, int>(45, 500) }) ||
                this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(54, 0), new Tuple<int, int>(54, 75), new Tuple<int, int>(55, 100), new Tuple<int, int>(25, 500) });
        }
    }
}
