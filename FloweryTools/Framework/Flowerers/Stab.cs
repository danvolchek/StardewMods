using FloweryTools.ParticleCreator;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    // Used by daggers
    internal class Stab : BaseCreator, IToolFlowerer
    {
        public Stab(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {
            if (frameIndex == 1)
            {
                int times = this.helper.rand.Next(1, 3);
                for (int i = 0; i < times; i++)
                {
                    this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(1), 10), this.helper.ApplyJitter(this.helper.GetDefaultMotion(), 0.1f), location);
                }
            }
        }

        public bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames)
        {
            //FarmerSprite: cases 276, 274, and 272 (278 is identical to 274 but flipped)
            return this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(40, -1), new Tuple<int, int>(38, -1), new Tuple<int, int>(38, 0) }) ||
                this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(34, -1), new Tuple<int, int>(33, -1), new Tuple<int, int>(33, 0) }) ||
                this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(25, -1), new Tuple<int, int>(27, -1), new Tuple<int, int>(27, 0) });
        }
    }
}
