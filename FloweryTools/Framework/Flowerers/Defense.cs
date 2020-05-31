using FloweryTools.ParticleCreator;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    // Used by the sword special
    internal class Defense : BaseCreator, IToolFlowerer
    {
        public Defense(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(i), 10), this.helper.ApplyJitter(this.helper.GetDefaultMotion(), 0.1f), location);
            }
        }

        public bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames)
        {
            //FarmerSprite: cases 252, 243, and 234 (259 is identical to 243 but flipped)
            //One frame passes between the game starting the animation and the animation animating
            return this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(40, -1) }) ||
                this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(34, -1) }) ||
                this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(28, -1) });
        }
    }
}
