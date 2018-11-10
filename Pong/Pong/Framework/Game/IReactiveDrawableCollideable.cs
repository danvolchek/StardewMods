namespace Pong.Framework.Game
{
    internal interface IReactiveDrawableCollideable : IDrawableCollideable
    {
        void CollideWith(INonReactiveDrawableCollideable other);
    }
}
