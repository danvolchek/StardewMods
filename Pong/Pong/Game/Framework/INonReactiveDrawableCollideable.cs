namespace Pong.Game.Framework
{
    internal interface INonReactiveDrawableCollideable : IDrawableCollideable
    {
        CollideInfo GetCollideInfo(IReactiveDrawableCollideable other);

        void Resize();
    }
}
