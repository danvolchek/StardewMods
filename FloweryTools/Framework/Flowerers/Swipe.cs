using FloweryTools.ParticleCreator;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    // Used by swords and clubs
    internal class Swipe : BaseCreator, IToolFlowerer
    {
        public Swipe(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {
            if (frameIndex % 2 == 0)
            {
                int times = this.helper.rand.Next(1, 3);
                for (int i = 0; i < times; i++)
                {
                    this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(frameIndex / 2), 10), this.helper.ApplyJitter(this.helper.GetDefaultMotion(), 0.1f), location);
                }
            }
        }

        public bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames)
        {
            //FarmerSprite: cases 248, 240, and 232 (256 is identical to 240 but flipped)
            return this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(36, 55), new Tuple<int, int>(37, 45), new Tuple<int, int>(38, 25), new Tuple<int, int>(39, 25), new Tuple<int, int>(40, 25), new Tuple<int, int>(41, -1), new Tuple<int, int>(41, 0) }) ||
                this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(30, 55), new Tuple<int, int>(31, 45), new Tuple<int, int>(32, 25), new Tuple<int, int>(33, 25), new Tuple<int, int>(34, 25), new Tuple<int, int>(35, -1), new Tuple<int, int>(35, 0) }) ||
                this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(24, 55), new Tuple<int, int>(25, 45), new Tuple<int, int>(26, 25), new Tuple<int, int>(27, 25), new Tuple<int, int>(28, 25), new Tuple<int, int>(29, -1), new Tuple<int, int>(29, 0) });
        }
    }
}
