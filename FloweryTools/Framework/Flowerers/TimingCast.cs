using FloweryTools.ParticleCreator;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    // Used by fishing poles
    internal class TimingCast : BaseCreator, IToolFlowerer
    {
        // Trigger flowers once every creationTrigger frames (higher = fewer flowers)
        private int creationCounter = 0;
        private const int creationTrigger = 10;

        // Number of intervals flowers should be positioned at (higher = more closely packed together)
        private int positionCounter = 0;
        private const int positionTrigger = 10;

        public TimingCast(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {
            if (frameIndex != 0)
                return;

            creationCounter++;
            if (creationCounter < creationTrigger)
                return;
            creationCounter = 0;

            positionCounter++;
            positionCounter %= positionTrigger;

            int max = this.helper.rand.Next(1, 3);
            for (int i = 0; i < max; i++)
            {
                this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(i), 10), this.helper.ApplyJitter(this.GetRadiusMotion(), 0.1f), location);
            }
        }

        private Vector2 GetRadiusMotion()
        {
            return 0.3f * new Vector2((float)Math.Cos(Math.PI * 2 * (((float)positionCounter) / positionTrigger)), (float)Math.Sin(Math.PI * 2 * (((float)positionCounter) / positionTrigger)));
        }

        public bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames)
        {
            //FarmerSprite: cases 295, 296, and 297 (298 is identical to 296 but flipped)
            return this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(76, 100), new Tuple<int, int>(38, 40), new Tuple<int, int>(63, 40), new Tuple<int, int>(62, 80), new Tuple<int, int>(63, 200) }) ||
                this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(48, 100), new Tuple<int, int>(49, 40), new Tuple<int, int>(50, 40), new Tuple<int, int>(51, 80), new Tuple<int, int>(52, 200) }) ||
                this.helper.MatchHelper(timer, 1, frames, new List<Tuple<int, int>> { new Tuple<int, int>(66, 100), new Tuple<int, int>(67, 40), new Tuple<int, int>(68, 40), new Tuple<int, int>(69, 80), new Tuple<int, int>(70, 200) });
        }
    }
}
