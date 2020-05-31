using StardewValley;

namespace FloweryTools.ParticleCreator
{
    internal interface IParticleCreator
    {
        void CreateParticles(GameLocation location, int frameIndex);
    }
}
