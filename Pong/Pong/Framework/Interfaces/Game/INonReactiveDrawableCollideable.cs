using Pong.Game;

namespace Pong.Framework.Interfaces.Game
{
    internal interface INonReactiveDrawableCollideable : IDrawableCollideable
    {
        CollideInfo GetCollideInfo(IReactiveDrawableCollideable other);

        void Resize();
    }
}
