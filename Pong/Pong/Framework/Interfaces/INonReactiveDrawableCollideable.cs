using Pong.Game;

namespace Pong.Framework.Interfaces
{
    internal interface INonReactiveDrawableCollideable : IDrawableCollideable
    {
        CollideInfo GetCollideInfo(IReactiveDrawableCollideable other);

        void Resize();
    }
}
