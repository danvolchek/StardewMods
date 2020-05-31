using FloweryTools.ParticleCreator;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    // Used by pickaxes, axes, and the club special
    internal class Swing : BaseCreator, IToolFlowerer
    {
        public Swing(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {

            if (frameIndex > 0)
            {
                int min = frameIndex < 4 ? 1 : 1;
                int max = frameIndex < 4 ? 1 : 3;

                int times = this.helper.rand.Next(min, max);
                for (int i = 0; i < times; i++)
                {
                    this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(1), 10), this.helper.ApplyJitter(this.GetMotion(frameIndex < 4), 0.1f), location);
                }
            }
        }

        private Vector2 GetMotion(bool explosion)
        {
            if (explosion)
            {
                return this.helper.GetDefaultMotion();
            } else
            {
                return this.helper.GetExplosionMotion();
            }
        }

        public bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames)
        {
            //FarmerSprite: cases 176, 168, and 160 (184 is identical to 168 but flipped)
            return this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(36, 100), new Tuple<int, int>(37, 40), new Tuple<int, int>(38, 40), new Tuple<int, int>(63, -1), new Tuple<int, int>(62, 75) }) ||
                this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(48, 100), new Tuple<int, int>(49, 40), new Tuple<int, int>(50, 40), new Tuple<int, int>(51, -1), new Tuple<int, int>(52, 75) }) ||
                this.helper.MatchHelper(timer, 0, frames, new List<Tuple<int, int>> { new Tuple<int, int>(66, 150), new Tuple<int, int>(67, 40), new Tuple<int, int>(68, 40), new Tuple<int, int>(69, -1), new Tuple<int, int>(70, 75) });
        }
    }
}
