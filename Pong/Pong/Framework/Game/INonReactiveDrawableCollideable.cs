using Pong.Game;

namespace Pong.Framework.Game
{
    internal interface INonReactiveDrawableCollideable : IDrawableCollideable
    {
        CollideInfo GetCollideInfo(IReactiveDrawableCollideable other);

        void Resize();
    }
}
