using StardewValley;
using System.Collections.Generic;

namespace FloweryTools.ParticleCreator
{
    internal interface IFrameMatcher
    {
        bool Matches(float timer, IList<FarmerSprite.AnimationFrame> frames);
    }
}
