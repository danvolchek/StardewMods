namespace Pong.Game.Framework
{
    internal interface IReactiveDrawableCollideable : IDrawableCollideable
    {
        void CollideWith(INonReactiveDrawableCollideable other);
    }
}
